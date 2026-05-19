using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans;

public class SyllabusResource : Entity
{
    public Guid Id { get; set; }
    public Guid SyllabusId { get; set; }
    public string? DocumentName { get; set; }
    public string? Abbreviation { get; set; }
    public string? IntendedUsers { get; set; }
    public string? Notes { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Syllabus Syllabus { get; set; } = null!;
}
