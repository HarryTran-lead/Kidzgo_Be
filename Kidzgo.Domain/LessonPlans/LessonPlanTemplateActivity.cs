using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlanTemplateActivity : Entity
{
    public Guid Id { get; set; }
    public Guid LessonPlanTemplateId { get; set; }
    public string? Title { get; set; }
    public string? TeacherActivity { get; set; }
    public string? StudentActivity { get; set; }
    public string? Resources { get; set; }
    public int? DurationMinutes { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LessonPlanTemplate LessonPlanTemplate { get; set; } = null!;
    public ICollection<Sessions.TeachingActivityLog> TeachingActivityLogs { get; set; } = new List<Sessions.TeachingActivityLog>();
}
