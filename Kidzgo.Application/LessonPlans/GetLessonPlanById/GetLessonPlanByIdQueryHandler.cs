using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Sessions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetLessonPlanById;

public sealed class GetLessonPlanByIdQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetLessonPlanByIdQuery, GetLessonPlanByIdResponse>
{
    public async Task<Result<GetLessonPlanByIdResponse>> Handle(
        GetLessonPlanByIdQuery query,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Class)
            .Include(lp => lp.Session)
                .ThenInclude(s => s!.Class)
            .Include(lp => lp.Session)
                .ThenInclude(s => s!.TeachingLog)
                    .ThenInclude(t => t!.Lessons)
            .Include(lp => lp.Session)
                .ThenInclude(s => s!.SessionLessons)
            .Include(lp => lp.Template)
                .ThenInclude(t => t!.Module)
                    .ThenInclude(m => m.Level)
            .Include(lp => lp.SubmittedByUser)
            .FirstOrDefaultAsync(lp => lp.Id == query.Id && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<GetLessonPlanByIdResponse>(
                LessonPlanErrors.NotFound(query.Id));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GetLessonPlanByIdResponse>(LessonPlanErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher &&
            (lessonPlan.Session is null ||
             (lessonPlan.Session.PlannedTeacherId != currentUser.Id &&
              lessonPlan.Session.ActualTeacherId != currentUser.Id)))
        {
            return Result.Failure<GetLessonPlanByIdResponse>(LessonPlanErrors.Unauthorized);
        }

        LessonPlanTemplate? resolvedTemplate = lessonPlan.Template;
        Guid? resolvedTemplateId = lessonPlan.TemplateId;

        if (lessonPlan.Session is not null)
        {
            var linkageSnapshot = new SessionLessonPlanLinkageSnapshot(
                lessonPlan.Session.LessonPlanTemplateId,
                lessonPlan.TemplateId,
                lessonPlan.Session.TeachingLog?.PlannedLessonPlanTemplateId,
                lessonPlan.Session.TeachingLog?.ActualLessonPlanTemplateId,
                lessonPlan.Session.SessionLessons
                    .OrderBy(x => x.OrderIndex)
                    .Select(x => x.LessonPlanTemplateId)
                    .FirstOrDefault(),
                lessonPlan.Session.Class?.SyllabusId,
                lessonPlan.Session.ModuleId,
                lessonPlan.Session.SessionIndexInModule);

            var canonicalResolution = await SessionLessonPlanCanonicalResolver.ResolveAsync(
                context,
                linkageSnapshot,
                cancellationToken);

            resolvedTemplateId = canonicalResolution.Linkage.LessonPlanTemplateId ?? lessonPlan.TemplateId;

            if (resolvedTemplateId.HasValue &&
                (resolvedTemplate is null || resolvedTemplate.Id != resolvedTemplateId.Value))
            {
                resolvedTemplate = await context.LessonPlanTemplates
                    .AsNoTracking()
                    .Include(t => t.Module)
                        .ThenInclude(m => m.Level)
                    .FirstOrDefaultAsync(
                        t => t.Id == resolvedTemplateId.Value && !t.IsDeleted,
                        cancellationToken);
            }
        }

        var plannedContent = resolvedTemplateId.HasValue &&
                             lessonPlan.TemplateId.HasValue &&
                             resolvedTemplateId.Value != lessonPlan.TemplateId.Value &&
                             !string.IsNullOrWhiteSpace(resolvedTemplate?.SyllabusContent)
            ? resolvedTemplate.SyllabusContent
            : lessonPlan.PlannedContent;
        var lessonProgress = lessonPlan.Session?.TeachingLog?.Lessons
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefault();
        var actualContent = lessonPlan.ActualContent ?? lessonPlan.Session?.TeachingLog?.ActualContent;
        var actualHomework = lessonPlan.ActualHomework ?? lessonPlan.Session?.TeachingLog?.ActualHomework;
        var teacherNotes = lessonPlan.TeacherNotes ?? lessonPlan.Session?.TeachingLog?.TeacherNote;

        return new GetLessonPlanByIdResponse
        {
            Id = lessonPlan.Id,
            ClassId = lessonPlan.ClassId,
            ClassCode = lessonPlan.Class?.Code,
            SessionId = lessonPlan.SessionId,
            SessionTitle = lessonPlan.Session != null
                ? $"Session {VietnamTime.FormatInVietnam(lessonPlan.Session.PlannedDatetime, "dd/MM/yyyy HH:mm")}"
                : null,
            SessionDate = lessonPlan.Session is null
                ? null
                : VietnamTime.ToVietnamDateTime(lessonPlan.Session.PlannedDatetime),
            TemplateId = resolvedTemplateId,
            TemplateLevel = resolvedTemplate?.Module?.Level?.Name,
            TemplateSessionIndex = resolvedTemplate?.SessionIndex,
            PlannedContent = plannedContent,
            ActualContent = actualContent,
            ActualHomework = actualHomework,
            TeacherNotes = teacherNotes,
            CompletionPercent = lessonPlan.CompletionPercent,
            CarryForwardContent = lessonPlan.CarryForwardContent,
            SubmittedBy = lessonPlan.SubmittedBy,
            SubmittedByName = lessonPlan.SubmittedByUser?.Name,
            SubmittedAt = lessonPlan.SubmittedAt,
            TeachingLog = lessonPlan.Session?.TeachingLog is null
                ? null
                : new TeachingLogSnapshotDto
                {
                    TeachingLogId = lessonPlan.Session.TeachingLog.Id,
                    SessionId = lessonPlan.Session.Id,
                    TeachingLogStatus = lessonPlan.Session.TeachingLog.Status.ToString(),
                    ProgressStatus = lessonProgress?.ProgressStatus.ToString(),
                    ActualTeachingType = lessonPlan.Session.TeachingLog.ActualTeachingType.ToString(),
                    ActualContent = lessonPlan.Session.TeachingLog.ActualContent,
                    ActualHomework = lessonPlan.Session.TeachingLog.ActualHomework,
                    TeacherNote = lessonPlan.Session.TeachingLog.TeacherNote,
                    SubmittedBy = lessonPlan.Session.TeachingLog.SubmittedBy,
                    SubmittedAt = lessonPlan.Session.TeachingLog.SubmittedAt,
                    UpdatedAt = lessonPlan.Session.TeachingLog.UpdatedAt
                },
        };
    }
}

