using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.AcademicProgression;

public class Assessment : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ModuleId { get; set; }
    public Guid? SessionId { get; set; }
    public string Type { get; set; } = null!;
    public decimal Score { get; set; }
    public AssessmentResult Result { get; set; }
    public string? TeacherComment { get; set; }
    public Guid AssessedBy { get; set; }
    public DateTime AssessedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public Session? Session { get; set; }
    public User AssessedByUser { get; set; } = null!;
}
