using Kidzgo.Application.Abstraction.Data;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.Shared;

public static class LessonPlanTemplateHardDeleteHelper
{
    public static async Task<LessonPlanTemplateHardDeleteResult> HardDeleteAsync(
        IDbContext context,
        IReadOnlyCollection<Guid> templateIds,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var normalizedTemplateIds = templateIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedTemplateIds.Count == 0)
        {
            return new LessonPlanTemplateHardDeleteResult();
        }

        var templates = await context.LessonPlanTemplates
            .Where(x => normalizedTemplateIds.Contains(x.Id))
            .Select(x => new
            {
                x.Id,
                x.LessonPlanUnitId
            })
            .ToListAsync(cancellationToken);

        if (templates.Count == 0)
        {
            return new LessonPlanTemplateHardDeleteResult();
        }

        var existingTemplateIds = templates
            .Select(x => x.Id)
            .ToList();

        var candidateUnitIds = templates
            .Where(x => x.LessonPlanUnitId.HasValue)
            .Select(x => x.LessonPlanUnitId!.Value)
            .Distinct()
            .ToList();

        var deletedLessonPlanCount = await context.LessonPlans
            .CountAsync(
                x => x.TemplateId.HasValue && existingTemplateIds.Contains(x.TemplateId.Value),
                cancellationToken);

        await context.LessonPlans
            .Where(x => x.TemplateId.HasValue && existingTemplateIds.Contains(x.TemplateId.Value))
            .ExecuteDeleteAsync(cancellationToken);

        await context.Sessions
            .Where(x => x.LessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.LessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.LessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.ClassSessionLessons
            .Where(x => x.LessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.LessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.LessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.TeachingLogs
            .Where(x => x.PlannedLessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.PlannedLessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.PlannedLessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.TeachingLogs
            .Where(x => x.ActualLessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.ActualLessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.ActualLessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.TeachingLogLessons
            .Where(x => x.LessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.LessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.LessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.Classes
            .Where(x => x.CurrentLessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.CurrentLessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.CurrentLessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.StudentProgresses
            .Where(x => x.CurrentLessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.CurrentLessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.CurrentLessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.SessionTemplates
            .Where(x => x.LessonPlanTemplateId.HasValue && existingTemplateIds.Contains(x.LessonPlanTemplateId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.LessonPlanTemplateId, (Guid?)null)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        await context.LessonPlanTemplateActivities
            .Where(x => existingTemplateIds.Contains(x.LessonPlanTemplateId))
            .ExecuteDeleteAsync(cancellationToken);

        await context.LessonPlanTemplateMaterials
            .Where(x => existingTemplateIds.Contains(x.LessonPlanTemplateId))
            .ExecuteDeleteAsync(cancellationToken);

        await context.HomeworkTemplates
            .Where(x => existingTemplateIds.Contains(x.LessonPlanTemplateId))
            .ExecuteDeleteAsync(cancellationToken);

        await context.LessonPlanTemplates
            .Where(x => existingTemplateIds.Contains(x.Id))
            .ExecuteDeleteAsync(cancellationToken);

        var deletedLessonPlanUnitCount = 0;
        if (candidateUnitIds.Count > 0)
        {
            deletedLessonPlanUnitCount = await context.LessonPlanUnits
                .Where(x => candidateUnitIds.Contains(x.Id))
                .Where(x => !context.LessonPlanTemplates.Any(t => t.LessonPlanUnitId == x.Id))
                .CountAsync(cancellationToken);

            if (deletedLessonPlanUnitCount > 0)
            {
                await context.LessonPlanUnits
                    .Where(x => candidateUnitIds.Contains(x.Id))
                    .Where(x => !context.LessonPlanTemplates.Any(t => t.LessonPlanUnitId == x.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }
        }

        return new LessonPlanTemplateHardDeleteResult
        {
            DeletedLessonPlanTemplateCount = existingTemplateIds.Count,
            DeletedLessonPlanCount = deletedLessonPlanCount,
            DeletedLessonPlanUnitCount = deletedLessonPlanUnitCount
        };
    }
}

public sealed class LessonPlanTemplateHardDeleteResult
{
    public int DeletedLessonPlanTemplateCount { get; init; }
    public int DeletedLessonPlanCount { get; init; }
    public int DeletedLessonPlanUnitCount { get; init; }
}
