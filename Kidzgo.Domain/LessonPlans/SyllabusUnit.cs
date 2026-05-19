using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.LessonPlans;

public class SyllabusUnit : Entity
{
    public Guid Id { get; set; }
    public Guid SyllabusId { get; set; }
    public Guid? ModuleId { get; set; }
    public string Name { get; set; } = null!;
    public int? AllocatedPeriods { get; set; }
    public int? LessonCount { get; set; }
    public int OrderIndex { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Syllabus Syllabus { get; set; } = null!;
    public Module? Module { get; set; }
}
