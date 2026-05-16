using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.AcademicProgression;

public class RemedialPlan : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ModuleId { get; set; }
    public string WeakSkills { get; set; } = null!;
    public int RecommendedSessionCount { get; set; }
    public string? Notes { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public User CreatedByUser { get; set; } = null!;
}
