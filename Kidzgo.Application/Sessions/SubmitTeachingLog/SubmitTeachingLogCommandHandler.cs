using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Application.Sessions;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.SubmitTeachingLog;

public sealed class SubmitTeachingLogCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ClassProgressionService classProgressionService
) : ICommandHandler<SubmitTeachingLogCommand, SubmitTeachingLogResponse>
{
    public async Task<Result<SubmitTeachingLogResponse>> Handle(
        SubmitTeachingLogCommand command,
        CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .Include(x => x.Class)
            .Include(x => x.TeachingLog)
                .ThenInclude(x => x!.Lessons)
            .FirstOrDefaultAsync(x => x.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<SubmitTeachingLogResponse>(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status == SessionStatus.Cancelled)
        {
            return Result.Failure<SubmitTeachingLogResponse>(SessionErrors.Cancelled);
        }

        if (session.TeachingLog is not null)
        {
            return Result.Failure<SubmitTeachingLogResponse>(SessionErrors.TeachingLogAlreadyExists(command.SessionId));
        }

        if (!TeachingLogProgressSupport.TryMapProgressStatus(command.ProgressStatus, out var coverageStatus, out var consumeLesson, out var coveragePercent))
        {
            return Result.Failure<SubmitTeachingLogResponse>(SessionErrors.InvalidTeachingProgressStatus(command.ProgressStatus));
        }

        var plannedLessonPlanTemplateId = session.LessonPlanTemplateId;
        if (!plannedLessonPlanTemplateId.HasValue)
        {
            return Result.Failure<SubmitTeachingLogResponse>(SessionErrors.MissingLessonTemplateForTeachingLog(command.SessionId));
        }

        if (coverageStatus == SessionCoverageStatus.Skipped &&
            string.IsNullOrWhiteSpace(command.TeacherNote))
        {
            return Result.Failure<SubmitTeachingLogResponse>(SessionErrors.SkippedRequiresReason);
        }

        var actualLessonPlanTemplateId = command.ActualLessonPlanTemplateId ?? plannedLessonPlanTemplateId;
        var now = VietnamTime.UtcNow();

        var teachingLog = new TeachingLog
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            PlannedLessonPlanTemplateId = plannedLessonPlanTemplateId,
            ActualLessonPlanTemplateId = actualLessonPlanTemplateId,
            ActualTeachingType = command.ActualTeachingType,
            ActualContent = command.ActualContent,
            ActualHomework = command.ActualHomework,
            TeacherNote = command.TeacherNote,
            SubmittedBy = userContext.UserId,
            Status = TeachingLogStatus.Submitted,
            GeneralNote = command.TeacherNote,
            HomeworkAssigned = command.ActualHomework,
            CarryForwardContent = !consumeLesson ? command.ActualContent : null,
            SubmittedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
            Lessons =
            [
                new TeachingLogLesson
                {
                    Id = Guid.NewGuid(),
                    LessonPlanTemplateId = actualLessonPlanTemplateId,
                    CoveragePercent = coveragePercent,
                    ProgressStatus = coverageStatus,
                    OrderIndex = 0,
                    Notes = command.TeacherNote,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            ]
        };

        context.TeachingLogs.Add(teachingLog);

        session.Status = SessionStatus.Completed;
        session.ActualDatetime ??= now;
        session.UpdatedAt = now;

        var syncResult = await classProgressionService.RecalculateAndResyncAsync(
            session.ClassId,
            resyncFutureSessions: true,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new SubmitTeachingLogResponse
        {
            TeachingLogId = teachingLog.Id,
            SessionId = session.Id,
            PlannedLessonPlanTemplateId = plannedLessonPlanTemplateId,
            ActualLessonPlanTemplateId = actualLessonPlanTemplateId,
            ActualTeachingType = command.ActualTeachingType.ToString(),
            ProgressStatus = coverageStatus.ToString(),
            ClassId = session.ClassId,
            CurrentModuleId = session.Class.CurrentModuleId,
            CurrentSessionIndex = session.Class.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = session.Class.CurrentLessonPlanTemplateId,
            UpdatedFutureSessionCount = syncResult?.UpdatedFutureSessionCount ?? 0
        };
    }
}
