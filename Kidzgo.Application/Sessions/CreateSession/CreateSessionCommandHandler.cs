using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionCommandHandler(
    IDbContext context,
    SessionConflictChecker conflictChecker,
    StudentSessionAssignmentService studentSessionAssignmentService,
    ClassSessionPlanningService classSessionPlanningService
) : ICommandHandler<CreateSessionCommand, CreateSessionResponse>
{
    public async Task<Result<CreateSessionResponse>> Handle(CreateSessionCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateSessionResponse>(ClassErrors.NotFound(command.ClassId));
        }

        // Only allow creating sessions for Planned, Recruiting, or Active classes
        if (classEntity.Status is not ClassStatus.Planned and not ClassStatus.Recruiting and not ClassStatus.Active)
        {
            return Result.Failure<CreateSessionResponse>(SessionErrors.InvalidClassStatus);
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
            return Result.Failure<CreateSessionResponse>(resourceValidation.Error);
        }

        var now = VietnamTime.UtcNow();
        var plannedUtc = VietnamTime.NormalizeToUtc(command.PlannedDatetime);
        var resolvedSectionType = command.SectionType ?? SectionType.Normal;

        var conflictResult = await conflictChecker.CheckConflictsAsync(
            Guid.Empty,
            plannedUtc,
            command.DurationMinutes,
            command.PlannedRoomId,
            command.PlannedTeacherId,
            command.PlannedAssistantId,
            cancellationToken);

        if (conflictResult.HasConflicts)
        {
            return Result.Failure<CreateSessionResponse>(
                ToSessionConflictError(conflictResult.Conflicts.First()));
        }

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ClassId = classEntity.Id,
            BranchId = classEntity.BranchId,
            Color = SessionColorPalette.GetRandomColor(),
            PlannedDatetime = plannedUtc,
            PlannedRoomId = command.PlannedRoomId,
            PlannedTeacherId = command.PlannedTeacherId,
            PlannedAssistantId = command.PlannedAssistantId,
            DurationMinutes = command.DurationMinutes,
            ParticipationType = command.ParticipationType,
            SectionType = resolvedSectionType,
            Status = SessionStatus.Scheduled,
            CreatedAt = now,
            UpdatedAt = now
        };

        var planningResult = await classSessionPlanningService.AssignMetadataAsync(
            classEntity.Id,
            [session],
            strictCurriculumCoverage: false,
            cancellationToken);
        if (planningResult.IsFailure)
        {
            return Result.Failure<CreateSessionResponse>(planningResult.Error);
        }
        context.Sessions.Add(session);
        await studentSessionAssignmentService.SyncAssignmentsForSessionAsync(session, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateSessionResponse
        {
            Id = session.Id,
            ClassId = session.ClassId,
            BranchId = session.BranchId,
            ModuleId = session.ModuleId,
            LessonPlanTemplateId = session.LessonPlanTemplateId,
            SessionIndexInModule = session.SessionIndexInModule,
            PlannedDatetime = session.PlannedDatetime,
            DurationMinutes = session.DurationMinutes,
            SectionType = session.SectionType.ToString()
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
