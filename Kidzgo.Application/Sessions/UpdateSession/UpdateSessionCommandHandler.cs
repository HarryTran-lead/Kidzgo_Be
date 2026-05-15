using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSession;

public sealed class UpdateSessionCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker,
    StudentSessionAssignmentService studentSessionAssignmentService
) : ICommandHandler<UpdateSessionCommand, UpdateSessionResponse>
{
    public async Task<Result<UpdateSessionResponse>> Handle(UpdateSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<UpdateSessionResponse>(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status is SessionStatus.Cancelled or SessionStatus.Completed)
        {
            return Result.Failure<UpdateSessionResponse>(SessionErrors.InvalidStatus);
        }

        var resourceValidation = await SessionResourceValidator.ValidateAsync(
            context,
            session.BranchId,
            command.PlannedRoomId,
            command.PlannedTeacherId,
            command.PlannedAssistantId,
            cancellationToken);

        if (resourceValidation.IsFailure)
        {
            return Result.Failure<UpdateSessionResponse>(resourceValidation.Error);
        }

        var plannedUtc = VietnamTime.NormalizeToUtc(command.PlannedDatetime);
        var slotTypeId = command.SlotTypeId ?? session.SlotTypeId;
        if (!slotTypeId.HasValue)
        {
            slotTypeId = await context.Classes
                .Where(x => x.Id == session.ClassId)
                .Select(x => x.SlotTypeId)
                .FirstOrDefaultAsync(cancellationToken);
        }
        string? slotTypeCode = null;

        if (slotTypeId.HasValue)
        {
            var slotType = await context.SlotTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == slotTypeId.Value && x.IsActive, cancellationToken);

            if (slotType is null)
            {
                return Result.Failure<UpdateSessionResponse>(
                    Error.Validation(
                        "Session.SlotTypeNotFound",
                        $"Slot type '{slotTypeId.Value}' was not found or inactive."));
            }

            slotTypeCode = slotType.Code;
        }

        var conflictResult = await conflictChecker.CheckConflictsAsync(
            session.Id,
            plannedUtc,
            command.DurationMinutes,
            command.PlannedRoomId,
            command.PlannedTeacherId,
            command.PlannedAssistantId,
            cancellationToken);

        if (conflictResult.HasConflicts)
        {
            return Result.Failure<UpdateSessionResponse>(
                ToSessionConflictError(conflictResult.Conflicts.First()));
        }

        session.PlannedDatetime = plannedUtc;
        session.DurationMinutes = command.DurationMinutes;
        session.PlannedRoomId = command.PlannedRoomId;
        session.PlannedTeacherId = command.PlannedTeacherId;
        session.PlannedAssistantId = command.PlannedAssistantId;
        session.SlotTypeId = slotTypeId;
        session.ParticipationType = command.ParticipationType;
        session.SectionType = command.SectionType;
        session.UpdatedAt = VietnamTime.UtcNow();

        await studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new UpdateSessionResponse
        {
            Id = session.Id,
            PlannedDatetime = session.PlannedDatetime,
            DurationMinutes = session.DurationMinutes,
            SectionType = session.SectionType.ToString(),
            SlotTypeId = session.SlotTypeId,
            SlotTypeCode = slotTypeCode
        };
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
}
