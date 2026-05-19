using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans;

public class LessonPlanTemplateMaterial : Entity
{
    public Guid Id { get; set; }
    public Guid LessonPlanTemplateId { get; set; }
    public string Name { get; set; } = null!;
    public string? MaterialType { get; set; }
    public string? ReferenceCode { get; set; }
    public string? Url { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LessonPlanTemplate LessonPlanTemplate { get; set; } = null!;
}
