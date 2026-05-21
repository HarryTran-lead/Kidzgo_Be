using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Application.Sessions;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateTeachingLog;

public sealed class UpdateTeachingLogCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ClassProgressionService classProgressionService
) : ICommandHandler<UpdateTeachingLogCommand, UpdateTeachingLogResponse>
{
    public async Task<Result<UpdateTeachingLogResponse>> Handle(
        UpdateTeachingLogCommand command,
        CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .Include(x => x.Class)
            .Include(x => x.TeachingLog)
                .ThenInclude(x => x!.Lessons)
            .FirstOrDefaultAsync(x => x.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<UpdateTeachingLogResponse>(SessionErrors.NotFound(command.SessionId));
        }

        if (session.TeachingLog is null)
        {
            return Result.Failure<UpdateTeachingLogResponse>(SessionErrors.TeachingLogNotFound(command.SessionId));
        }

        if (session.TeachingLog.Status is TeachingLogStatus.Approved or TeachingLogStatus.Locked)
        {
            return Result.Failure<UpdateTeachingLogResponse>(SessionErrors.TeachingLogLocked(command.SessionId));
        }

        if (!TeachingLogProgressSupport.TryMapProgressStatus(command.ProgressStatus, out var coverageStatus, out var consumeLesson, out var coveragePercent))
        {
            return Result.Failure<UpdateTeachingLogResponse>(SessionErrors.InvalidTeachingProgressStatus(command.ProgressStatus));
        }

        if (coverageStatus == SessionCoverageStatus.Skipped &&
            string.IsNullOrWhiteSpace(command.TeacherNote))
        {
            return Result.Failure<UpdateTeachingLogResponse>(SessionErrors.SkippedRequiresReason);
        }

        var plannedLessonPlanTemplateId = session.TeachingLog.PlannedLessonPlanTemplateId ?? session.LessonPlanTemplateId;
        if (!plannedLessonPlanTemplateId.HasValue)
        {
            return Result.Failure<UpdateTeachingLogResponse>(SessionErrors.MissingLessonTemplateForTeachingLog(command.SessionId));
        }

        var actualLessonPlanTemplateId = command.ActualLessonPlanTemplateId ?? plannedLessonPlanTemplateId;
        var now = VietnamTime.UtcNow();

        session.TeachingLog.PlannedLessonPlanTemplateId = plannedLessonPlanTemplateId;
        session.TeachingLog.ActualLessonPlanTemplateId = actualLessonPlanTemplateId;
        session.TeachingLog.ActualTeachingType = command.ActualTeachingType;
        session.TeachingLog.ActualContent = command.ActualContent;
        session.TeachingLog.ActualHomework = command.ActualHomework;
        session.TeachingLog.TeacherNote = command.TeacherNote;
        session.TeachingLog.GeneralNote = command.TeacherNote;
        session.TeachingLog.HomeworkAssigned = command.ActualHomework;
        session.TeachingLog.CarryForwardContent = !consumeLesson ? command.ActualContent : null;
        session.TeachingLog.SubmittedBy = userContext.UserId;
        session.TeachingLog.SubmittedAt ??= now;
        session.TeachingLog.UpdatedAt = now;

        var lessonEntry = session.TeachingLog.Lessons
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();

        if (lessonEntry is null)
        {
            lessonEntry = new TeachingLogLesson
            {
                Id = Guid.NewGuid(),
                TeachingLogId = session.TeachingLog.Id,
                OrderIndex = 0,
                CreatedAt = now
            };
            session.TeachingLog.Lessons.Add(lessonEntry);
        }

        lessonEntry.LessonPlanTemplateId = actualLessonPlanTemplateId;
        lessonEntry.CoveragePercent = coveragePercent;
        lessonEntry.ProgressStatus = coverageStatus;
        lessonEntry.Notes = command.TeacherNote;
        lessonEntry.UpdatedAt = now;
        if (lessonEntry.CreatedAt == default)
        {
            lessonEntry.CreatedAt = now;
        }

        session.Status = SessionStatus.Completed;
        session.ActualDatetime ??= now;
        session.UpdatedAt = now;

        var syncResult = await classProgressionService.RecalculateAndResyncAsync(
            session.ClassId,
            resyncFutureSessions: true,
            cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateTeachingLogResponse
        {
            TeachingLogId = session.TeachingLog.Id,
            SessionId = session.Id,
            ClassId = session.ClassId,
            PlannedLessonPlanTemplateId = plannedLessonPlanTemplateId,
            ActualLessonPlanTemplateId = actualLessonPlanTemplateId,
            ActualTeachingType = command.ActualTeachingType.ToString(),
            ProgressStatus = coverageStatus.ToString(),
            CurrentModuleId = session.Class.CurrentModuleId,
            CurrentSessionIndex = session.Class.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = session.Class.CurrentLessonPlanTemplateId,
            UpdatedFutureSessionCount = syncResult?.UpdatedFutureSessionCount ?? 0
        };
    }
}
