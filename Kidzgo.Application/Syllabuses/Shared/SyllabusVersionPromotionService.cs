using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.Shared;

internal static class SyllabusVersionPromotionService
{
    public static async Task PromoteAsync(
        IDbContext context,
        Syllabus target,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var family = await context.Syllabuses
            .Where(x => !x.IsDeleted &&
                        x.ProgramId == target.ProgramId &&
                        x.LevelId == target.LevelId &&
                        x.Code == target.Code)
            .ToListAsync(cancellationToken);

        var previousActiveIds = family
            .Where(x => x.IsActive && x.Id != target.Id)
            .Select(x => x.Id)
            .ToHashSet();

        foreach (var syllabus in family)
        {
            syllabus.IsActive = syllabus.Id == target.Id;
            syllabus.UpdatedAt = now;
        }

        if (previousActiveIds.Count == 0)
        {
            return;
        }

        var activeAssignments = await context.CurriculumAssignments
            .Where(x => x.IsActive && previousActiveIds.Contains(x.SyllabusId))
            .ToListAsync(cancellationToken);

        var targetAssignments = await context.CurriculumAssignments
            .Where(x => x.SyllabusId == target.Id)
            .ToListAsync(cancellationToken);

        foreach (var assignment in activeAssignments)
        {
            var duplicate = targetAssignments.FirstOrDefault(x =>
                x.BranchId == assignment.BranchId &&
                x.ProgramId == assignment.ProgramId &&
                x.LevelId == assignment.LevelId &&
                x.EffectiveFrom == assignment.EffectiveFrom);

            if (duplicate is null)
            {
                assignment.SyllabusId = target.Id;
                assignment.UpdatedAt = now;
                targetAssignments.Add(assignment);
                continue;
            }

            duplicate.IsActive = duplicate.IsActive || assignment.IsActive;
            duplicate.EffectiveTo ??= assignment.EffectiveTo;
            duplicate.UpdatedAt = now;
            context.CurriculumAssignments.Remove(assignment);
        }

        var activeMappings = await context.PackageCurriculumMappings
            .Where(x => x.IsActive && previousActiveIds.Contains(x.SyllabusId))
            .ToListAsync(cancellationToken);

        var targetMappings = await context.PackageCurriculumMappings
            .Where(x => x.SyllabusId == target.Id)
            .ToListAsync(cancellationToken);

        foreach (var mapping in activeMappings)
        {
            var duplicate = targetMappings.FirstOrDefault(x => x.TuitionPlanId == mapping.TuitionPlanId);

            if (duplicate is null)
            {
                mapping.SyllabusId = target.Id;
                mapping.UpdatedAt = now;
                targetMappings.Add(mapping);
                continue;
            }

            duplicate.IsActive = duplicate.IsActive || mapping.IsActive;
            duplicate.UpdatedAt = now;
            context.PackageCurriculumMappings.Remove(mapping);
        }
    }
}
