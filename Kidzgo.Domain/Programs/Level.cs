using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Programs;

public class Level : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Program Program { get; set; } = null!;
    public ICollection<Module> Modules { get; set; } = new List<Module>();
    public ICollection<LessonPlanTemplate> LessonPlanTemplates { get; set; } = new List<LessonPlanTemplate>();
}
