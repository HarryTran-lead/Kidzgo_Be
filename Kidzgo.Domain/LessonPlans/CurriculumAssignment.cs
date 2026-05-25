using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.LessonPlans;

public class CurriculumAssignment : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid SyllabusId { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Branch Branch { get; set; } = null!;
    public Program Program { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public Syllabus Syllabus { get; set; } = null!;
}
