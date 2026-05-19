using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.GetCurriculumImportConfiguration;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.UpsertCurriculumImportConfiguration;

public sealed class UpsertCurriculumImportConfigurationCommandHandler(IDbContext context)
    : ICommandHandler<UpsertCurriculumImportConfigurationCommand, CurriculumImportConfigurationResponse>
{
    public async Task<Result<CurriculumImportConfigurationResponse>> Handle(
        UpsertCurriculumImportConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(command);
        if (validationError is not null)
        {
            return Result.Failure<CurriculumImportConfigurationResponse>(validationError);
        }

        var level = await context.Levels
            .Where(x => x.Id == command.LevelId && x.IsActive)
            .Select(x => new { x.Id, x.ProgramId })
            .FirstOrDefaultAsync(cancellationToken);

        if (level is null)
        {
            return Result.Failure<CurriculumImportConfigurationResponse>(SyllabusErrors.LevelNotFound(command.LevelId));
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<CurriculumImportConfigurationResponse>(
                SyllabusErrors.LevelDoesNotBelongToProgram(command.LevelId, command.ProgramId));
        }

        var modules = await context.Modules
            .Where(x => x.LevelId == command.LevelId && x.IsActive)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);

        var modulesById = modules.ToDictionary(x => x.Id);
        foreach (var rule in command.Rules)
        {
            if (!modulesById.ContainsKey(rule.ModuleId))
            {
                return Result.Failure<CurriculumImportConfigurationResponse>(
                    SyllabusErrors.InvalidImportConfiguration(
                        $"Module '{rule.ModuleId}' does not belong to Level '{command.LevelId}'"));
            }
        }

        var configuration = await context.CurriculumImportConfigurations
            .Include(x => x.ModuleRules)
            .FirstOrDefaultAsync(
                x => x.ProgramId == command.ProgramId &&
                     x.LevelId == command.LevelId,
                cancellationToken);

        var now = VietnamTime.UtcNow();
        if (configuration is null)
        {
            configuration = new CurriculumImportConfiguration
            {
                Id = Guid.NewGuid(),
                ProgramId = command.ProgramId,
                LevelId = command.LevelId,
                CreatedAt = now
            };

            context.CurriculumImportConfigurations.Add(configuration);
        }
        else
        {
            await context.CurriculumImportModuleRules
                .Where(x => x.CurriculumImportConfigurationId == configuration.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        configuration.RegularUnitLessonPlanCount = command.RegularUnitLessonPlanCount;
        configuration.StarterUnitLessonPlanCount = command.StarterUnitLessonPlanCount;
        configuration.RevisionLessonPlanCount = command.RevisionLessonPlanCount;
        configuration.IsActive = command.IsActive;
        configuration.UpdatedAt = now;

        var ruleEntities = command.Rules
            .OrderBy(x => x.OrderIndex)
            .Select(rule => new CurriculumImportModuleRule
            {
                Id = Guid.NewGuid(),
                CurriculumImportConfigurationId = configuration.Id,
                ModuleId = rule.ModuleId,
                IncludeStarterUnit = rule.IncludeStarterUnit,
                UnitFrom = rule.UnitFrom,
                UnitTo = rule.UnitTo,
                RevisionNumber = rule.RevisionNumber,
                OrderIndex = rule.OrderIndex
            })
            .ToList();

        context.CurriculumImportModuleRules.AddRange(ruleEntities);

        foreach (var rule in command.Rules)
        {
            var module = modulesById[rule.ModuleId];
            module.PlannedSessionCount = CalculateExpectedLessonPlanCount(command, rule);
            module.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CurriculumImportConfigurationResponse
        {
            Id = configuration.Id,
            ProgramId = configuration.ProgramId,
            LevelId = configuration.LevelId,
            RegularUnitLessonPlanCount = configuration.RegularUnitLessonPlanCount,
            StarterUnitLessonPlanCount = configuration.StarterUnitLessonPlanCount,
            RevisionLessonPlanCount = configuration.RevisionLessonPlanCount,
            IsActive = configuration.IsActive,
            Rules = ruleEntities.Select(rule =>
            {
                var module = modulesById[rule.ModuleId];
                return new CurriculumImportModuleRuleResponse
                {
                    Id = rule.Id,
                    ModuleId = module.Id,
                    ModuleCode = module.Code,
                    ModuleName = module.Name,
                    ModuleOrder = module.Order,
                    IncludeStarterUnit = rule.IncludeStarterUnit,
                    UnitFrom = rule.UnitFrom,
                    UnitTo = rule.UnitTo,
                    RevisionNumber = rule.RevisionNumber,
                    OrderIndex = rule.OrderIndex,
                    ExpectedLessonPlanCount = CalculateExpectedLessonPlanCount(command, new UpsertCurriculumImportModuleRuleModel
                    {
                        ModuleId = rule.ModuleId,
                        IncludeStarterUnit = rule.IncludeStarterUnit,
                        UnitFrom = rule.UnitFrom,
                        UnitTo = rule.UnitTo,
                        RevisionNumber = rule.RevisionNumber,
                        OrderIndex = rule.OrderIndex
                    })
                };
            }).ToList()
        };
    }

    private static Error? Validate(UpsertCurriculumImportConfigurationCommand command)
    {
        if (command.RegularUnitLessonPlanCount <= 0)
        {
            return SyllabusErrors.InvalidImportConfiguration("RegularUnitLessonPlanCount must be greater than 0.");
        }

        if (command.StarterUnitLessonPlanCount <= 0)
        {
            return SyllabusErrors.InvalidImportConfiguration("StarterUnitLessonPlanCount must be greater than 0.");
        }

        if (command.RevisionLessonPlanCount <= 0)
        {
            return SyllabusErrors.InvalidImportConfiguration("RevisionLessonPlanCount must be greater than 0.");
        }

        if (command.Rules.Count == 0)
        {
            return SyllabusErrors.InvalidImportConfiguration("At least one curriculum import rule is required.");
        }

        if (command.Rules.Select(x => x.ModuleId).Distinct().Count() != command.Rules.Count)
        {
            return SyllabusErrors.InvalidImportConfiguration("Each module can only appear once in curriculum import rules.");
        }

        if (command.Rules.Select(x => x.OrderIndex).Distinct().Count() != command.Rules.Count)
        {
            return SyllabusErrors.InvalidImportConfiguration("Each rule must have a unique OrderIndex.");
        }

        if (command.Rules.Count(x => x.IncludeStarterUnit) > 1)
        {
            return SyllabusErrors.InvalidImportConfiguration("Only one module rule can include the starter unit.");
        }

        var orderedRanges = command.Rules
            .Where(x => x.UnitFrom.HasValue || x.UnitTo.HasValue)
            .OrderBy(x => x.UnitFrom ?? int.MinValue)
            .ToList();

        foreach (var rule in command.Rules)
        {
            if (rule.UnitFrom.HasValue != rule.UnitTo.HasValue)
            {
                return SyllabusErrors.InvalidImportConfiguration(
                    $"Module '{rule.ModuleId}' must provide both UnitFrom and UnitTo together.");
            }

            if (rule.UnitFrom.HasValue && rule.UnitTo.HasValue)
            {
                if (rule.UnitFrom.Value <= 0 || rule.UnitTo.Value <= 0 || rule.UnitFrom.Value > rule.UnitTo.Value)
                {
                    return SyllabusErrors.InvalidImportConfiguration(
                        $"Module '{rule.ModuleId}' has an invalid unit range.");
                }
            }

            if (rule.RevisionNumber.HasValue && rule.RevisionNumber.Value <= 0)
            {
                return SyllabusErrors.InvalidImportConfiguration(
                    $"Module '{rule.ModuleId}' has an invalid revision number.");
            }
        }

        for (var i = 1; i < orderedRanges.Count; i++)
        {
            var previous = orderedRanges[i - 1];
            var current = orderedRanges[i];

            if (previous.UnitTo.HasValue &&
                current.UnitFrom.HasValue &&
                current.UnitFrom.Value <= previous.UnitTo.Value)
            {
                return SyllabusErrors.InvalidImportConfiguration("Unit ranges in curriculum import rules cannot overlap.");
            }
        }

        var revisionNumbers = command.Rules
            .Where(x => x.RevisionNumber.HasValue)
            .Select(x => x.RevisionNumber!.Value)
            .ToList();

        if (revisionNumbers.Distinct().Count() != revisionNumbers.Count)
        {
            return SyllabusErrors.InvalidImportConfiguration("Each revision number can only be assigned to one module rule.");
        }

        return null;
    }

    private static int CalculateExpectedLessonPlanCount(
        UpsertCurriculumImportConfigurationCommand command,
        UpsertCurriculumImportModuleRuleModel rule)
    {
        var total = 0;

        if (rule.IncludeStarterUnit)
        {
            total += command.StarterUnitLessonPlanCount;
        }

        if (rule.UnitFrom.HasValue && rule.UnitTo.HasValue)
        {
            total += (rule.UnitTo.Value - rule.UnitFrom.Value + 1) * command.RegularUnitLessonPlanCount;
        }

        if (rule.RevisionNumber.HasValue)
        {
            total += command.RevisionLessonPlanCount;
        }

        return total;
    }
}
