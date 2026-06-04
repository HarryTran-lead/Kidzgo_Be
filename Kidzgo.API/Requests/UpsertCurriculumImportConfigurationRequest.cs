namespace Kidzgo.API.Requests;

public sealed class UpsertCurriculumImportConfigurationRequest
{
    public int RegularUnitLessonPlanCount { get; init; }
    public int RevisionLessonPlanCount { get; init; }
    public bool IsActive { get; init; } = true;
    public IReadOnlyCollection<UpsertCurriculumImportModuleRuleRequest> Rules { get; init; } = Array.Empty<UpsertCurriculumImportModuleRuleRequest>();
}

public sealed class UpsertCurriculumImportModuleRuleRequest
{
    public Guid ModuleId { get; init; }
    public int? UnitFrom { get; init; }
    public int? UnitTo { get; init; }
    public int? RevisionNumber { get; init; }
    public int OrderIndex { get; init; }
}
