using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.UpdateLessonPlan;

public sealed class UpdateLessonPlanCommandHandler(
    IDbContext context,
    IUserContext userContext,
    Services.ProgressionService progressionService
) : ICommandHandler<UpdateLessonPlanCommand, UpdateLessonPlanResponse>
{
    public async Task<Result<UpdateLessonPlanResponse>> Handle(
        UpdateLessonPlanCommand command,
        CancellationToken cancellationToken)
    {
        var lessonPlan = await context.LessonPlans
            .Include(lp => lp.Session)
            .FirstOrDefaultAsync(lp => lp.Id == command.Id && !lp.IsDeleted, cancellationToken);

        if (lessonPlan is null)
        {
            return Result.Failure<UpdateLessonPlanResponse>(
                LessonPlanErrors.NotFound(command.Id));
        }

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<UpdateLessonPlanResponse>(LessonPlanErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher &&
            (lessonPlan.Session is null ||
             (lessonPlan.Session.PlannedTeacherId != currentUser.Id &&
              lessonPlan.Session.ActualTeacherId != currentUser.Id)))
        {
            return Result.Failure<UpdateLessonPlanResponse>(LessonPlanErrors.Unauthorized);
        }

        // Validate template if provided
        if (command.TemplateId.HasValue)
        {
            var template = await context.LessonPlanTemplates
                .FirstOrDefaultAsync(t => t.Id == command.TemplateId.Value, cancellationToken);

            if (template is null)
            {
                return Result.Failure<UpdateLessonPlanResponse>(
                    LessonPlanErrors.TemplateNotFound(command.TemplateId));
            }
        }

        // Update fields
        if (command.TemplateId.HasValue)
        {
            lessonPlan.TemplateId = command.TemplateId.Value;
        }

        if (command.PlannedContent != null)
        {
            lessonPlan.PlannedContent = command.PlannedContent;
        }

        if (command.ActualContent != null)
        {
            lessonPlan.ActualContent = command.ActualContent;
        }

        if (command.ActualHomework != null)
        {
            lessonPlan.ActualHomework = command.ActualHomework;
        }

        if (command.TeacherNotes != null)
        {
            lessonPlan.TeacherNotes = command.TeacherNotes;
        }

        if (command.CompletionPercent.HasValue)
        {
            lessonPlan.CompletionPercent = Math.Clamp(command.CompletionPercent.Value, 0, 100);
        }

        if (command.CarryForwardContent != null)
        {
            lessonPlan.CarryForwardContent = command.CarryForwardContent;
        }

        await context.SaveChangesAsync(cancellationToken);

        await RecalculateStudentProgressAsync(
            lessonPlan.SessionId,
            lessonPlan.TemplateId,
            cancellationToken);

        return new UpdateLessonPlanResponse
        {
            Id = lessonPlan.Id,
            SessionId = lessonPlan.SessionId,
            TemplateId = lessonPlan.TemplateId,
            PlannedContent = lessonPlan.PlannedContent,
            ActualContent = lessonPlan.ActualContent,
            ActualHomework = lessonPlan.ActualHomework,
            TeacherNotes = lessonPlan.TeacherNotes,
            CompletionPercent = lessonPlan.CompletionPercent,
            CarryForwardContent = lessonPlan.CarryForwardContent
        };
    }

    private async Task RecalculateStudentProgressAsync(
        Guid sessionId,
        Guid? templateId,
        CancellationToken cancellationToken)
    {
        if (!templateId.HasValue)
        {
            return;
        }

        var moduleId = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => x.Id == templateId.Value)
            .Select(x => x.ModuleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!moduleId.HasValue)
        {
            return;
        }

        var studentIds = await context.Attendances
            .AsNoTracking()
            .Where(x => x.SessionId == sessionId
                        && (x.AttendanceStatus == Domain.Sessions.AttendanceStatus.Present
                            || x.AttendanceStatus == Domain.Sessions.AttendanceStatus.Makeup))
            .Select(x => x.StudentProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        foreach (var studentId in studentIds)
        {
            await progressionService.UpsertStudentProgressAsync(
                studentId,
                moduleId.Value,
                templateId,
                null,
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

