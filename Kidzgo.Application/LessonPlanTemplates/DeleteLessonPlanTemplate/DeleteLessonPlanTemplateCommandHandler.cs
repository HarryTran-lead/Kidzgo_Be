using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.DeleteLessonPlanTemplate;

public sealed class DeleteLessonPlanTemplateCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<DeleteLessonPlanTemplateCommand, DeleteLessonPlanTemplateResponse>
{
    public async Task<Result<DeleteLessonPlanTemplateResponse>> Handle(
        DeleteLessonPlanTemplateCommand command,
        CancellationToken cancellationToken)
    {
        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (currentUser is null || currentUser.Role == UserRole.Teacher)
        {
            return Result.Failure<DeleteLessonPlanTemplateResponse>(LessonPlanTemplateErrors.Unauthorized);
        }

        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);
        if (template is null)
        {
            return Result.Failure<DeleteLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.NotFound(command.Id));
        }

        var hasActiveLessonPlans = await context.LessonPlans
            .AnyAsync(x => x.TemplateId == command.Id && !x.IsDeleted, cancellationToken);
        if (hasActiveLessonPlans)
        {
            return Result.Failure<DeleteLessonPlanTemplateResponse>(
                LessonPlanTemplateErrors.HasActiveLessonPlans);
        }

        var now = VietnamTime.UtcNow();
        template.IsDeleted = true;
        template.IsActive = false;
        template.UpdatedAt = now;

        await context.SessionTemplates
            .Where(x => x.LessonPlanTemplateId == command.Id)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.LessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteLessonPlanTemplateResponse
        {
            Id = template.Id,
            IsDeleted = template.IsDeleted,
            UpdatedAt = template.UpdatedAt
        };
    }
}
