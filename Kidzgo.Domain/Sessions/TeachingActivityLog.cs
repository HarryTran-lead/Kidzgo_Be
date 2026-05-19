using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Sessions;

public class TeachingActivityLog : Entity
{
    public Guid Id { get; set; }
    public Guid TeachingLogId { get; set; }
    public Guid? PlannedActivityId { get; set; }
    public string? ActualActivityText { get; set; }
    public int? ActualDurationMinutes { get; set; }
    public bool WasCompleted { get; set; }
    public int OrderIndex { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TeachingLog TeachingLog { get; set; } = null!;
    public LessonPlanTemplateActivity? PlannedActivity { get; set; }
}
