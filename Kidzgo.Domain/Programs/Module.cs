using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Programs;

public class Module : Entity
{
    public Guid Id { get; set; }
    public Guid LevelId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Order { get; set; }
    public string? Description { get; set; }
    public int PlannedSessionCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Level Level { get; set; } = null!;
    public ICollection<LessonPlanTemplate> LessonPlanTemplates { get; set; } = new List<LessonPlanTemplate>();
    public ICollection<StudentProgress> StudentProgresses { get; set; } = new List<StudentProgress>();
    public ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public ICollection<TeacherEvaluation> TeacherEvaluations { get; set; } = new List<TeacherEvaluation>();
    public ICollection<PromotionDecision> PromotionDecisions { get; set; } = new List<PromotionDecision>();
    public ICollection<RemedialPlan> RemedialPlans { get; set; } = new List<RemedialPlan>();
}
