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
            .Include(x => x.LessonPlan)
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
            LessonPlanId = session.LessonPlan?.Id,
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
        SyncLessonPlanFromTeachingLog(session.LessonPlan, teachingLog, coveragePercent, consumeLesson);

        session.Status = SessionStatus.Completed;
        session.ActualDatetime ??= now;
        session.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

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
            ActualContent = command.ActualContent,
            ActualHomework = command.ActualHomework,
            TeacherNote = command.TeacherNote,
            ClassId = session.ClassId,
            CurrentModuleId = syncResult?.CurrentModuleId ?? session.Class.CurrentModuleId,
            CurrentSessionIndex = syncResult?.CurrentSessionIndex ?? session.Class.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = syncResult?.CurrentLessonPlanTemplateId ?? session.Class.CurrentLessonPlanTemplateId,
            UpdatedFutureSessionCount = syncResult?.UpdatedFutureSessionCount ?? 0
        };
    }

    private static void SyncLessonPlanFromTeachingLog(
        Domain.LessonPlans.LessonPlan? lessonPlan,
        TeachingLog teachingLog,
        decimal coveragePercent,
        bool consumeLesson)
    {
        if (lessonPlan is null)
        {
            return;
        }

        lessonPlan.TemplateId = teachingLog.ActualLessonPlanTemplateId ?? lessonPlan.TemplateId;
        lessonPlan.ActualContent = teachingLog.ActualContent;
        lessonPlan.ActualHomework = teachingLog.ActualHomework;
        lessonPlan.TeacherNotes = teachingLog.TeacherNote;
        lessonPlan.CompletionPercent = coveragePercent;
        lessonPlan.CarryForwardContent = consumeLesson ? null : teachingLog.ActualContent;
        lessonPlan.SubmittedBy = teachingLog.SubmittedBy;
        lessonPlan.SubmittedAt = teachingLog.SubmittedAt;
    }
}
