using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Domain.Programs;

public class PackageCurriculumMapping : Entity
{
    public Guid Id { get; set; }
    public Guid TuitionPlanId { get; set; }
    public Guid SyllabusId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public TuitionPlan TuitionPlan { get; set; } = null!;
    public Syllabus Syllabus { get; set; } = null!;
}
