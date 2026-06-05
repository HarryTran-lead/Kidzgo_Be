using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.Shared;

public sealed class TuitionPlanModuleDto
{
    public Guid ModuleId { get; init; }
    public string? ModuleCode { get; init; }
    public string? ModuleName { get; init; }
    public int ModuleOrder { get; init; }
    public int PlannedSessionCount { get; init; }
}

public sealed class TuitionPlanSyllabusDto
{
    public Guid SyllabusId { get; init; }
    public string SyllabusCode { get; init; } = null!;
    public int SyllabusVersion { get; init; }
    public string SyllabusTitle { get; init; } = null!;
}

internal sealed class ValidatedTuitionPlanSelectionResult
{
    public Syllabus? Syllabus { get; init; }
    public IReadOnlyList<Module> OrderedSelectedModules { get; init; } = Array.Empty<Module>();
    public int ResolvedTotalSessions { get; init; }
    public Guid? StartModuleId { get; init; }
}

internal static class TuitionPlanSelectionSupport
{
    internal static IReadOnlyList<Guid> NormalizeRequestedModuleIds(
        IEnumerable<Guid>? moduleIds)
    {
        var orderedIds = new List<Guid>();
        var seen = new HashSet<Guid>();

        if (moduleIds is not null)
        {
            foreach (var moduleId in moduleIds)
            {
                if (seen.Add(moduleId))
                {
                    orderedIds.Add(moduleId);
                }
            }
        }

        return orderedIds;
    }

    internal static async Task<Result<ValidatedTuitionPlanSelectionResult>> ValidateSelectionAsync(
        IDbContext context,
        Guid programId,
        Guid levelId,
        Guid? syllabusId,
        IReadOnlyCollection<Guid> requestedModuleIds,
        int requestedTotalSessions,
        CancellationToken cancellationToken)
    {
        if (requestedModuleIds.Count == 0)
        {
            if (syllabusId.HasValue)
            {
                return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                    TuitionPlanErrors.ModuleSelectionRequiredForSyllabus);
            }

            if (requestedTotalSessions <= 0)
            {
                return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                    Error.Validation(
                        "TuitionPlan.TotalSessionsRequired",
                        "Total sessions must be greater than 0 when no modules are selected."));
            }

            return Result.Success(new ValidatedTuitionPlanSelectionResult
            {
                ResolvedTotalSessions = requestedTotalSessions
            });
        }

        if (!syllabusId.HasValue)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                TuitionPlanErrors.SyllabusRequiredForModuleSelection);
        }

        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == syllabusId.Value && x.IsActive && !x.IsDeleted,
                cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(TuitionPlanErrors.SyllabusNotFound);
        }

        if (syllabus.ProgramId != programId)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(TuitionPlanErrors.SyllabusProgramMismatch);
        }

        if (syllabus.LevelId != levelId)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(TuitionPlanErrors.SyllabusLevelMismatch);
        }

        var modules = await context.Modules
            .AsNoTracking()
            .Where(x => requestedModuleIds.Contains(x.Id) && x.IsActive)
            .ToListAsync(cancellationToken);

        if (modules.Count != requestedModuleIds.Count)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(TuitionPlanErrors.ModuleNotFound);
        }

        if (modules.Any(x => x.LevelId != levelId))
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(TuitionPlanErrors.ModuleLevelMismatch);
        }

        var syllabusModuleIds = await context.SessionTemplates
            .AsNoTracking()
            .Where(x => x.SyllabusId == syllabusId.Value && x.ModuleId.HasValue && x.IsActive)
            .Select(x => x.ModuleId!.Value)
            .Concat(
                context.SyllabusUnits
                    .AsNoTracking()
                    .Where(x => x.SyllabusId == syllabusId.Value && x.ModuleId.HasValue)
                    .Select(x => x.ModuleId!.Value))
            .Concat(
                context.SyllabusLessons
                    .AsNoTracking()
                    .Where(x => x.SyllabusId == syllabusId.Value && x.ModuleId.HasValue)
                    .Select(x => x.ModuleId!.Value))
            .Distinct()
            .ToListAsync(cancellationToken);

        var syllabusModuleIdSet = syllabusModuleIds.ToHashSet();
        var invalidModuleId = requestedModuleIds.FirstOrDefault(x => !syllabusModuleIdSet.Contains(x));
        if (invalidModuleId != Guid.Empty)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                TuitionPlanErrors.SelectedModuleNotInSyllabus(syllabusId.Value, invalidModuleId));
        }

        var orderedSelectedModules = modules
            .OrderBy(x => x.Order)
            .ToList();

        var orderedSyllabusModuleIds = modules.Count == 0
            ? new List<Guid>()
            : await context.Modules
                .AsNoTracking()
                .Where(x => syllabusModuleIdSet.Contains(x.Id))
                .OrderBy(x => x.Order)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

        if (!FormsConsecutiveSyllabusSequence(orderedSyllabusModuleIds, orderedSelectedModules.Select(x => x.Id)))
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                TuitionPlanErrors.SelectedModulesMustBeConsecutive(syllabusId.Value));
        }

        var expectedTotalSessions = orderedSelectedModules.Sum(x => x.PlannedSessionCount);
        if (expectedTotalSessions <= 0)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                Error.Validation(
                    "TuitionPlan.SelectedModulesHaveNoSessions",
                    "Selected modules must have a positive total planned session count."));
        }

        var resolvedTotalSessions = requestedTotalSessions > 0
            ? requestedTotalSessions
            : expectedTotalSessions;

        if (resolvedTotalSessions != expectedTotalSessions)
        {
            return Result.Failure<ValidatedTuitionPlanSelectionResult>(
                TuitionPlanErrors.ModuleSelectionSessionCountMismatch(expectedTotalSessions));
        }

        return Result.Success(new ValidatedTuitionPlanSelectionResult
        {
            Syllabus = syllabus,
            OrderedSelectedModules = orderedSelectedModules,
            ResolvedTotalSessions = resolvedTotalSessions,
            StartModuleId = orderedSelectedModules[0].Id
        });
    }

    internal static void ReplaceSelectedModules(
        TuitionPlan tuitionPlan,
        IReadOnlyList<Module> orderedSelectedModules,
        DateTime now)
    {
        tuitionPlan.ModuleId = orderedSelectedModules.Count == 0
            ? null
            : orderedSelectedModules[0].Id;

        tuitionPlan.SelectedModules.Clear();
        for (var index = 0; index < orderedSelectedModules.Count; index++)
        {
            tuitionPlan.SelectedModules.Add(new TuitionPlanModuleSelection
            {
                Id = Guid.NewGuid(),
                TuitionPlanId = tuitionPlan.Id,
                ModuleId = orderedSelectedModules[index].Id,
                OrderIndex = index,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    internal static IReadOnlyList<Guid> ResolveModuleIds(TuitionPlan tuitionPlan)
    {
        var selectedModuleIds = tuitionPlan.SelectedModules
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.ModuleId)
            .ToList();

        if (selectedModuleIds.Count > 0)
        {
            return selectedModuleIds;
        }

        return tuitionPlan.ModuleId.HasValue
            ? new[] { tuitionPlan.ModuleId.Value }
            : Array.Empty<Guid>();
    }

    internal static IReadOnlyList<TuitionPlanModuleDto> ResolveModules(TuitionPlan tuitionPlan)
    {
        var selectedModules = tuitionPlan.SelectedModules
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.Module)
            .Where(x => x is not null)
            .Select(x => new TuitionPlanModuleDto
            {
                ModuleId = x!.Id,
                ModuleCode = x.Code,
                ModuleName = x.Name,
                ModuleOrder = x.Order,
                PlannedSessionCount = x.PlannedSessionCount
            })
            .ToList();

        if (selectedModules.Count > 0)
        {
            return selectedModules;
        }

        if (tuitionPlan.ModuleId.HasValue)
        {
            return new[]
            {
                new TuitionPlanModuleDto
                {
                    ModuleId = tuitionPlan.ModuleId.Value,
                    ModuleCode = tuitionPlan.Module?.Code,
                    ModuleName = tuitionPlan.Module?.Name,
                    ModuleOrder = tuitionPlan.Module?.Order ?? 0,
                    PlannedSessionCount = tuitionPlan.Module?.PlannedSessionCount ?? 0
                }
            };
        }

        return Array.Empty<TuitionPlanModuleDto>();
    }
    internal static TuitionPlanSyllabusDto? ResolveActiveSyllabus(TuitionPlan tuitionPlan)
    {
        var mapping = tuitionPlan.CurriculumMappings
            .Where(x => x.IsActive && x.Syllabus is not null && !x.Syllabus.IsDeleted)
            .OrderByDescending(x => x.Syllabus.IsActive)
            .ThenByDescending(x => x.Syllabus.Version)
            .FirstOrDefault();

        if (mapping?.Syllabus is null)
        {
            return null;
        }

        return new TuitionPlanSyllabusDto
        {
            SyllabusId = mapping.SyllabusId,
            SyllabusCode = mapping.Syllabus.Code,
            SyllabusVersion = mapping.Syllabus.Version,
            SyllabusTitle = mapping.Syllabus.Title
        };
    }

    private static bool FormsConsecutiveSyllabusSequence(
        IReadOnlyList<Guid> orderedSyllabusModuleIds,
        IEnumerable<Guid> orderedSelectedModuleIds)
    {
        var selected = orderedSelectedModuleIds.ToList();
        if (selected.Count == 0)
        {
            return true;
        }

        var startIndex = -1;
        for (var index = 0; index < orderedSyllabusModuleIds.Count; index++)
        {
            if (orderedSyllabusModuleIds[index] == selected[0])
            {
                startIndex = index;
                break;
            }
        }

        if (startIndex < 0 || startIndex + selected.Count > orderedSyllabusModuleIds.Count)
        {
            return false;
        }

        for (var index = 0; index < selected.Count; index++)
        {
            if (orderedSyllabusModuleIds[startIndex + index] != selected[index])
            {
                return false;
            }
        }

        return true;
    }
}
