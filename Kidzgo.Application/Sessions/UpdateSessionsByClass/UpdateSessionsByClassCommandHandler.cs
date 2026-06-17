using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSessionsByClass;

public sealed class UpdateSessionsByClassCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker,
    StudentSessionAssignmentService studentSessionAssignmentService
) : ICommandHandler<UpdateSessionsByClassCommand, UpdateSessionsByClassResponse>
{
    public async Task<Result<UpdateSessionsByClassResponse>> Handle(
        UpdateSessionsByClassCommand command,
        CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<UpdateSessionsByClassResponse>(
                ClassErrors.NotFound(command.ClassId));
        }

        var resourceValidation = await SessionResourceValidator.ValidateAsync(
            context,
            classEntity.BranchId,
            command.PlannedRoomId,
            command.PlannedTeacherId,
            command.PlannedAssistantId,
            cancellationToken);

        if (resourceValidation.IsFailure)
        {
            return Result.Failure<UpdateSessionsByClassResponse>(resourceValidation.Error);
        }

        var query = context.Sessions
            .Where(s => s.ClassId == command.ClassId);

        if (command.SessionIds != null && command.SessionIds.Count > 0)
        {
            query = query.Where(s => command.SessionIds.Contains(s.Id));
        }

        if (command.FilterByStatus.HasValue)
        {
            query = query.Where(s => s.Status == command.FilterByStatus.Value);
        }

        if (command.FromDate.HasValue)
        {
            var fromDateUtc = VietnamTime.NormalizeToUtc(command.FromDate.Value);
            query = query.Where(s => s.PlannedDatetime >= fromDateUtc);
        }

        query = query.Where(s => s.Status != SessionStatus.Cancelled && s.Status != SessionStatus.Completed);

        var sessions = await query.ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return Result.Success(new UpdateSessionsByClassResponse
            {
                UpdatedSessionsCount = 0,
                UpdatedSessionIds = new List<Guid>(),
                SkippedSessionIds = new List<Guid>(),
                Errors = new List<string> { "No sessions were found for update" }
            });
        }

        var updatedSessionIds = new List<Guid>();
        var skippedSessionIds = new List<Guid>();
        var errors = new List<string>();
        var now = VietnamTime.UtcNow();

        foreach (var session in sessions)
        {
            try
            {
                bool hasChanges = false;

                var plannedUtc = command.PlannedDatetime.HasValue
                    ? VietnamTime.NormalizeToUtc(command.PlannedDatetime.Value)
                    : session.PlannedDatetime;

                var duration = command.DurationMinutes ?? session.DurationMinutes;
                var roomId = command.PlannedRoomId ?? session.PlannedRoomId;
                var teacherId = command.PlannedTeacherId ?? session.PlannedTeacherId;
                var assistantId = command.PlannedAssistantId ?? session.PlannedAssistantId;

                bool needsConflictCheck = command.PlannedDatetime.HasValue ||
                                          command.PlannedRoomId.HasValue ||
                                          command.PlannedTeacherId.HasValue ||
                                          command.PlannedAssistantId.HasValue ||
                                          command.DurationMinutes.HasValue;

                if (needsConflictCheck)
                {
                    var conflictResult = await conflictChecker.CheckConflictsAsync(
                        session.Id,
                        plannedUtc,
                        duration,
                        roomId,
                        teacherId,
                        assistantId,
                        cancellationToken);

                    if (conflictResult.HasConflicts)
                    {
                        var conflictMessages = conflictResult.Conflicts
                            .Select(c => $"{c.Type}: {c.ClassCode} - {c.ClassTitle} at {c.ConflictDatetime:dd/MM/yyyy HH:mm}")
                            .ToList();
                        errors.Add($"Session {session.Id}: conflict - {string.Join(", ", conflictMessages)}");
                        skippedSessionIds.Add(session.Id);
                        continue;
                    }
                }

                if (command.PlannedDatetime.HasValue)
                {
                    session.PlannedDatetime = plannedUtc;
                    hasChanges = true;
                }

                if (command.DurationMinutes.HasValue)
                {
                    session.DurationMinutes = command.DurationMinutes.Value;
                    hasChanges = true;
                }

                if (command.PlannedRoomId.HasValue)
                {
                    session.PlannedRoomId = command.PlannedRoomId.Value;
                    hasChanges = true;
                }

                if (command.PlannedTeacherId.HasValue)
                {
                    session.PlannedTeacherId = command.PlannedTeacherId.Value;
                    hasChanges = true;
                }

                if (command.PlannedAssistantId.HasValue)
                {
                    session.PlannedAssistantId = command.PlannedAssistantId.Value;
                    hasChanges = true;
                }

                if (command.ParticipationType.HasValue)
                {
                    session.ParticipationType = command.ParticipationType.Value;
                    hasChanges = true;
                }

                if (command.SectionType.HasValue)
                {
                    session.SectionType = command.SectionType.Value;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    session.UpdatedAt = now;
                    await studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
                    updatedSessionIds.Add(session.Id);
                }
                else
                {
                    skippedSessionIds.Add(session.Id);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Session {session.Id}: {ex.Message}");
                skippedSessionIds.Add(session.Id);
            }
        }

        if (updatedSessionIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new UpdateSessionsByClassResponse
        {
            UpdatedSessionsCount = updatedSessionIds.Count,
            UpdatedSessionIds = updatedSessionIds,
            SkippedSessionIds = skippedSessionIds,
            Errors = errors
        });
    }
}
