using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Sessions;

public class TeachingLog : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? LessonPlanId { get; set; }
    public Guid? PlannedLessonPlanTemplateId { get; set; }
    public Guid? ActualLessonPlanTemplateId { get; set; }
    public TeachingLogTeachingType ActualTeachingType { get; set; }
    public string? ActualContent { get; set; }
    public string? ActualHomework { get; set; }
    public string? TeacherNote { get; set; }
    public Guid? SubmittedBy { get; set; }
    public TeachingLogStatus Status { get; set; }
    public string? GeneralNote { get; set; }
    public string? HomeworkAssigned { get; set; }
    public string? CarryForwardContent { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? LockedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Session Session { get; set; } = null!;
    public LessonPlan? LessonPlan { get; set; }
    public LessonPlanTemplate? PlannedLessonPlanTemplate { get; set; }
    public LessonPlanTemplate? ActualLessonPlanTemplate { get; set; }
    public User? SubmittedByUser { get; set; }
    public ICollection<TeachingLogLesson> Lessons { get; set; } = new List<TeachingLogLesson>();
    public ICollection<TeachingActivityLog> ActivityLogs { get; set; } = new List<TeachingActivityLog>();
}
