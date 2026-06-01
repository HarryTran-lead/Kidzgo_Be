using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetTeachingLogBySession;

public sealed class GetTeachingLogBySessionQueryHandler(
    IDbContext context
) : IQueryHandler<GetTeachingLogBySessionQuery, GetTeachingLogBySessionResponse>
{
    public async Task<Result<GetTeachingLogBySessionResponse>> Handle(
        GetTeachingLogBySessionQuery query,
        CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .AsNoTracking()
            .Include(x => x.TeachingLog)
                .ThenInclude(x => x!.Lessons)
            .Include(x => x.TeachingLog)
                .ThenInclude(x => x!.PlannedLessonPlanTemplate)
            .Include(x => x.TeachingLog)
                .ThenInclude(x => x!.ActualLessonPlanTemplate)
            .FirstOrDefaultAsync(x => x.Id == query.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<GetTeachingLogBySessionResponse>(SessionErrors.NotFound(query.SessionId));
        }

        if (session.TeachingLog is null)
        {
            return Result.Failure<GetTeachingLogBySessionResponse>(SessionErrors.TeachingLogNotFound(query.SessionId));
        }

        var lessonProgress = session.TeachingLog.Lessons
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();

        return new GetTeachingLogBySessionResponse
        {
            TeachingLogId = session.TeachingLog.Id,
            SessionId = session.Id,
            PlannedLessonPlanTemplateId = session.TeachingLog.PlannedLessonPlanTemplateId,
            PlannedLessonTitle = session.TeachingLog.PlannedLessonPlanTemplate?.Title,
            ActualLessonPlanTemplateId = session.TeachingLog.ActualLessonPlanTemplateId,
            ActualLessonTitle = session.TeachingLog.ActualLessonPlanTemplate?.Title,
            TeachingLogStatus = session.TeachingLog.Status.ToString(),
            ProgressStatus = lessonProgress?.ProgressStatus.ToString(),
            ActualTeachingType = session.TeachingLog.ActualTeachingType.ToString(),
            ActualContent = session.TeachingLog.ActualContent,
            ActualHomework = session.TeachingLog.ActualHomework,
            TeacherNote = session.TeachingLog.TeacherNote,
            SubmittedBy = session.TeachingLog.SubmittedBy,
            SubmittedAt = session.TeachingLog.SubmittedAt,
            UpdatedAt = session.TeachingLog.UpdatedAt,
            TeachingLog = new TeachingLogSnapshotDto
            {
                TeachingLogId = session.TeachingLog.Id,
                SessionId = session.Id,
                TeachingLogStatus = session.TeachingLog.Status.ToString(),
                ProgressStatus = lessonProgress?.ProgressStatus.ToString(),
                ActualTeachingType = session.TeachingLog.ActualTeachingType.ToString(),
                ActualContent = session.TeachingLog.ActualContent,
                ActualHomework = session.TeachingLog.ActualHomework,
                TeacherNote = session.TeachingLog.TeacherNote,
                SubmittedBy = session.TeachingLog.SubmittedBy,
                SubmittedAt = session.TeachingLog.SubmittedAt,
                UpdatedAt = session.TeachingLog.UpdatedAt
            }
        };
    }
}
