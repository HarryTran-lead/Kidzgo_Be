using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.LessonPlans;

public class SyllabusLesson : Entity
{
    public Guid Id { get; set; }
    public Guid SyllabusId { get; set; }
    public Guid? ModuleId { get; set; }
    public int? PeriodFrom { get; set; }
    public int? PeriodTo { get; set; }
    public string? Topic { get; set; }
    public int? LessonNumber { get; set; }
    public string? ContentSummary { get; set; }
    public string? StructureSummary { get; set; }
    public string? StudentBookPages { get; set; }
    public string? TeacherBookPages { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Syllabus Syllabus { get; set; } = null!;
    public Module? Module { get; set; }
}
