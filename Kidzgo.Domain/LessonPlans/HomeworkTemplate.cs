using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans;

public class HomeworkTemplate : Entity
{
    public Guid Id { get; set; }
    public Guid LessonPlanTemplateId { get; set; }
    public string Title { get; set; } = null!;
    public string? Instructions { get; set; }
    public string? MaterialReference { get; set; }
    public int OrderIndex { get; set; }
    public bool IsRequired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LessonPlanTemplate LessonPlanTemplate { get; set; } = null!;
}
