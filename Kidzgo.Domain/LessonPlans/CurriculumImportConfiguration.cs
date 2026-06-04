namespace Kidzgo.Domain.LessonPlans;

public class CurriculumImportConfiguration
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public int RegularUnitLessonPlanCount { get; set; }
    public int RevisionLessonPlanCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Programs.Program Program { get; set; } = null!;
    public Programs.Level Level { get; set; } = null!;
    public ICollection<CurriculumImportModuleRule> ModuleRules { get; set; } = new List<CurriculumImportModuleRule>();
}
