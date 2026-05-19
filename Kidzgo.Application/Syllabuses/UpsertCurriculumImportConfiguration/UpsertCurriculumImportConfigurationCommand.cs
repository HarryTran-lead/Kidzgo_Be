using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.GetCurriculumImportConfiguration;

namespace Kidzgo.Application.Syllabuses.UpsertCurriculumImportConfiguration;

public sealed class UpsertCurriculumImportConfigurationCommand : ICommand<CurriculumImportConfigurationResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public int RegularUnitLessonPlanCount { get; init; }
    public int StarterUnitLessonPlanCount { get; init; }
    public int RevisionLessonPlanCount { get; init; }
    public bool IsActive { get; init; } = true;
    public IReadOnlyCollection<UpsertCurriculumImportModuleRuleModel> Rules { get; init; } = Array.Empty<UpsertCurriculumImportModuleRuleModel>();
}

public sealed class UpsertCurriculumImportModuleRuleModel
{
    public Guid ModuleId { get; init; }
    public bool IncludeStarterUnit { get; init; }
    public int? UnitFrom { get; init; }
    public int? UnitTo { get; init; }
    public int? RevisionNumber { get; init; }
    public int OrderIndex { get; init; }
}
