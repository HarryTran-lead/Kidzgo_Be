using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.ReorderLessonPlanTemplateSessionOrders;

public sealed class ReorderLessonPlanTemplateSessionOrdersCommandHandler(IDbContext context)
    : ICommandHandler<ReorderLessonPlanTemplateSessionOrdersCommand, ReorderLessonPlanTemplateSessionOrdersResponse>
{
    public async Task<Result<ReorderLessonPlanTemplateSessionOrdersResponse>> Handle(
        ReorderLessonPlanTemplateSessionOrdersCommand command,
        CancellationToken cancellationToken)
    {
        var levelExists = await context.Levels
            .AnyAsync(x => x.Id == command.LevelId, cancellationToken);
        if (!levelExists)
        {
            return Result.Failure<ReorderLessonPlanTemplateSessionOrdersResponse>(
                LessonPlanTemplateErrors.LevelNotFound(command.LevelId));
        }

        if (command.Items.Count == 0)
        {
            return new ReorderLessonPlanTemplateSessionOrdersResponse
            {
                LevelId = command.LevelId,
                Items = []
            };
        }

        var duplicateOrder = command.Items
            .GroupBy(x => x.SessionOrder)
            .Where(x => x.Key > 0 && x.Count() > 1)
            .Select(x => (int?)x.Key)
            .FirstOrDefault();
        if (duplicateOrder.HasValue)
        {
            return Result.Failure<ReorderLessonPlanTemplateSessionOrdersResponse>(
                LessonPlanTemplateErrors.DuplicateSessionOrder(duplicateOrder.Value));
        }

        var totalPlannedSessions = await context.Modules
            .Where(x => x.LevelId == command.LevelId && x.IsActive)
            .SumAsync(x => x.PlannedSessionCount, cancellationToken);

        foreach (var item in command.Items)
        {
            if (item.SessionOrder <= 0 ||
                (totalPlannedSessions > 0 && item.SessionOrder > totalPlannedSessions))
            {
                return Result.Failure<ReorderLessonPlanTemplateSessionOrdersResponse>(
                    LessonPlanTemplateErrors.SessionOrderOutOfRange(item.SessionOrder, totalPlannedSessions));
            }
        }

        var itemIds = command.Items.Select(x => x.Id).Distinct().ToList();
        var templates = await context.LessonPlanTemplates
            .Include(x => x.Module)
            .Where(x => itemIds.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var itemId in itemIds)
        {
            var template = templates.FirstOrDefault(x => x.Id == itemId);
            if (template is null)
            {
                return Result.Failure<ReorderLessonPlanTemplateSessionOrdersResponse>(
                    LessonPlanTemplateErrors.NotFound(itemId));
            }

            if (template.Module.LevelId != command.LevelId)
            {
                return Result.Failure<ReorderLessonPlanTemplateSessionOrdersResponse>(
                    LessonPlanTemplateErrors.DoesNotBelongToLevel(itemId, command.LevelId));
            }
        }

        var now = VietnamTime.UtcNow();
        var sessionOrderById = command.Items.ToDictionary(x => x.Id, x => x.SessionOrder);
        foreach (var template in templates)
        {
            template.SessionOrder = sessionOrderById[template.Id];
            template.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ReorderLessonPlanTemplateSessionOrdersResponse
        {
            LevelId = command.LevelId,
            Items = templates
                .OrderBy(x => x.SessionOrder)
                .Select(x => new ReorderedLessonPlanTemplateSessionOrderDto
                {
                    Id = x.Id,
                    ModuleId = x.ModuleId,
                    SessionIndex = x.SessionIndex,
                    SessionOrder = x.SessionOrder,
                    UpdatedAt = x.UpdatedAt
                })
                .ToList()
        };
    }
}
