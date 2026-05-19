using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Sessions;

public class ClassSessionLesson : Entity
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid? SessionTemplateId { get; set; }
    public Guid? LessonPlanTemplateId { get; set; }
    public decimal? CoveragePercent { get; set; }
    public SessionCoverageStatus ProgressStatus { get; set; }
    public int OrderIndex { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Session Session { get; set; } = null!;
    public SessionTemplate? SessionTemplate { get; set; }
    public LessonPlanTemplate? LessonPlanTemplate { get; set; }
}
