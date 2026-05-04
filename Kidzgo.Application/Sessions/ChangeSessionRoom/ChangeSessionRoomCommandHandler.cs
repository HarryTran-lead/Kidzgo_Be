using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.ChangeSessionRoom;

public sealed class ChangeSessionRoomCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker
) : ICommandHandler<ChangeSessionRoomCommand, ChangeSessionRoomResponse>
{
    public async Task<Result<ChangeSessionRoomResponse>> Handle(
        ChangeSessionRoomCommand command,
        CancellationToken cancellationToken)
    {
        var requestedIds = command.SessionIds.Distinct().ToList();
        var sessions = await context.Sessions
            .Where(session => requestedIds.Contains(session.Id))
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        var response = new ChangeSessionRoomResponse
        {
            SkippedSessionIds = requestedIds.Except(sessions.Select(session => session.Id)).ToList(),
            Errors = requestedIds
                .Except(sessions.Select(session => session.Id))
                .Select(sessionId => FormatSessionError(sessionId, SessionErrors.NotFound(sessionId)))
                .ToList()
        };

        foreach (var session in sessions)
        {
            if (session.Status is SessionStatus.Cancelled or SessionStatus.Completed)
            {
                response.SkippedSessionIds.Add(session.Id);
                response.Errors.Add(FormatSessionError(
                    session.Id,
                    SessionErrors.CannotChangeCancelledOrCompleted(session.Id)));
                continue;
            }

            if (session.PlannedDatetime.AddMinutes(session.DurationMinutes) < now)
            {
                response.SkippedSessionIds.Add(session.Id);
                response.Errors.Add(FormatSessionError(
                    session.Id,
                    SessionErrors.CannotChangePastSession(session.Id)));
                continue;
            }

            var resourceValidation = await SessionResourceValidator.ValidateAsync(
                context,
                session.BranchId,
                command.RoomId,
                session.PlannedTeacherId,
                session.PlannedAssistantId,
                cancellationToken);

            if (resourceValidation.IsFailure)
            {
                response.SkippedSessionIds.Add(session.Id);
                response.Errors.Add(FormatSessionError(session.Id, resourceValidation.Error));
                continue;
            }

            var conflictResult = await conflictChecker.CheckConflictsAsync(
                session.Id,
                session.PlannedDatetime,
                session.DurationMinutes,
                command.RoomId,
                session.PlannedTeacherId,
                session.PlannedAssistantId,
                cancellationToken);

            if (conflictResult.HasConflicts)
            {
                response.SkippedSessionIds.Add(session.Id);
                response.Errors.Add(FormatSessionError(
                    session.Id,
                    ToSessionConflictError(conflictResult.Conflicts.First())));
                continue;
            }

            if (session.PlannedRoomId == command.RoomId)
            {
                response.SkippedSessionIds.Add(session.Id);
                continue;
            }

            session.PlannedRoomId = command.RoomId;
            session.UpdatedAt = now;
            response.UpdatedSessionIds.Add(session.Id);
        }

        if (response.UpdatedSessionIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        response.UpdatedSessionsCount = response.UpdatedSessionIds.Count;
        return Result.Success(response);
    }

    private static Error ToSessionConflictError(SessionConflict conflict)
    {
        return conflict.Type switch
        {
            ConflictType.Room => SessionErrors.RoomOccupied(
                conflict.ClassCode,
                conflict.ClassTitle,
                conflict.ConflictDatetime),
            ConflictType.Teacher => SessionErrors.TeacherOccupied(
                conflict.ClassCode,
                conflict.ClassTitle,
                conflict.ConflictDatetime),
            ConflictType.Assistant => SessionErrors.AssistantOccupied(
                conflict.ClassCode,
                conflict.ClassTitle,
                conflict.ConflictDatetime),
            _ => SessionErrors.InvalidStatus
        };
    }

    private static string FormatSessionError(Guid sessionId, Error error)
    {
        return $"Session {sessionId}: {error.Description}";
    }
}
