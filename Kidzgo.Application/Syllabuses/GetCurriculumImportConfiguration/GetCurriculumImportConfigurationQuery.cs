using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.GetCurriculumImportConfiguration;

public sealed class GetCurriculumImportConfigurationQuery : IQuery<CurriculumImportConfigurationResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
}

public sealed class CurriculumImportConfigurationResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public int RegularUnitLessonPlanCount { get; init; }
    public int StarterUnitLessonPlanCount { get; init; }
    public int RevisionLessonPlanCount { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyCollection<CurriculumImportModuleRuleResponse> Rules { get; init; } = Array.Empty<CurriculumImportModuleRuleResponse>();
}

public sealed class CurriculumImportModuleRuleResponse
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public int ModuleOrder { get; init; }
    public bool IncludeStarterUnit { get; init; }
    public int? UnitFrom { get; init; }
    public int? UnitTo { get; init; }
    public int? RevisionNumber { get; init; }
    public int OrderIndex { get; init; }
    public int ExpectedLessonPlanCount { get; init; }
}
