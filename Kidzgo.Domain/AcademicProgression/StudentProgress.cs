using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.AcademicProgression;

public class StudentProgress : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ModuleId { get; set; }
    public StudentProgressStatus Status { get; set; }
    public decimal CompletionPercent { get; set; }
    public StudentProgressAssessmentStatus AssessmentStatus { get; set; }
    public PromotionStatus PromotionStatus { get; set; }
    public Guid? LastAssessmentId { get; set; }
    public Guid? CurrentLessonPlanTemplateId { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public Assessment? LastAssessment { get; set; }
    public LessonPlanTemplate? CurrentLessonPlanTemplate { get; set; }
}
