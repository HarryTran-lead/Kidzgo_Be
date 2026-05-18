using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.AcademicProgression;

public class TeacherEvaluation : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ModuleId { get; set; }
    public int Speaking { get; set; }
    public int Listening { get; set; }
    public int Reading { get; set; }
    public int Writing { get; set; }
    public int Participation { get; set; }
    public int Confidence { get; set; }
    public int Behavior { get; set; }
    public string? Notes { get; set; }
    public Guid EvaluatedBy { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public User EvaluatedByUser { get; set; } = null!;
}
