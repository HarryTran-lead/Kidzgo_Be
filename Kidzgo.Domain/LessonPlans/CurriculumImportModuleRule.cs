namespace Kidzgo.Domain.LessonPlans;

public class CurriculumImportModuleRule
{
    public Guid Id { get; set; }
    public Guid CurriculumImportConfigurationId { get; set; }
    public Guid ModuleId { get; set; }
    public bool IncludeStarterUnit { get; set; }
    public int? UnitFrom { get; set; }
    public int? UnitTo { get; set; }
    public int? RevisionNumber { get; set; }
    public int OrderIndex { get; set; }

    public CurriculumImportConfiguration CurriculumImportConfiguration { get; set; } = null!;
    public Programs.Module Module { get; set; } = null!;
}
