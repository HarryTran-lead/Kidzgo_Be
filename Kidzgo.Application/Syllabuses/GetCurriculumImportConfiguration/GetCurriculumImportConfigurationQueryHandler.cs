using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetCurriculumImportConfiguration;

public sealed class GetCurriculumImportConfigurationQueryHandler(IDbContext context)
    : IQueryHandler<GetCurriculumImportConfigurationQuery, CurriculumImportConfigurationResponse>
{
    public async Task<Result<CurriculumImportConfigurationResponse>> Handle(
        GetCurriculumImportConfigurationQuery query,
        CancellationToken cancellationToken)
    {
        var configuration = await context.CurriculumImportConfigurations
            .AsNoTracking()
            .Include(x => x.ModuleRules)
            .FirstOrDefaultAsync(
                x => x.ProgramId == query.ProgramId &&
                     x.LevelId == query.LevelId,
                cancellationToken);

        if (configuration is null)
        {
            return Result.Failure<CurriculumImportConfigurationResponse>(
                SyllabusErrors.ImportConfigurationNotFound(query.ProgramId, query.LevelId));
        }

        var modules = await context.Modules
            .AsNoTracking()
            .Where(x => x.LevelId == query.LevelId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        return new CurriculumImportConfigurationResponse
        {
            Id = configuration.Id,
            ProgramId = configuration.ProgramId,
            LevelId = configuration.LevelId,
            RegularUnitLessonPlanCount = configuration.RegularUnitLessonPlanCount,
            StarterUnitLessonPlanCount = configuration.StarterUnitLessonPlanCount,
            RevisionLessonPlanCount = configuration.RevisionLessonPlanCount,
            IsActive = configuration.IsActive,
            Rules = configuration.ModuleRules
                .OrderBy(x => x.OrderIndex)
                .Select(rule =>
                {
                    var module = modules[rule.ModuleId];
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
                        ExpectedLessonPlanCount = CalculateExpectedLessonPlanCount(
                            configuration.RegularUnitLessonPlanCount,
                            configuration.StarterUnitLessonPlanCount,
                            configuration.RevisionLessonPlanCount,
                            rule)
                    };
                })
                .ToList()
        };
    }

    private static int CalculateExpectedLessonPlanCount(
        int regularUnitLessonPlanCount,
        int starterUnitLessonPlanCount,
        int revisionLessonPlanCount,
        Domain.LessonPlans.CurriculumImportModuleRule rule)
    {
        var total = 0;

        if (rule.IncludeStarterUnit)
        {
            total += starterUnitLessonPlanCount;
        }

        if (rule.UnitFrom.HasValue && rule.UnitTo.HasValue)
        {
            total += (rule.UnitTo.Value - rule.UnitFrom.Value + 1) * regularUnitLessonPlanCount;
        }

        if (rule.RevisionNumber.HasValue)
        {
            total += revisionLessonPlanCount;
        }

        return total;
    }
}
