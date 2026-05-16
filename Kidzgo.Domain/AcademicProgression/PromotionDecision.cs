using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.AcademicProgression;

public class PromotionDecision : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ModuleId { get; set; }
    public PromotionDecisionResult Decision { get; set; }
    public string? Reason { get; set; }
    public Guid ApprovedBy { get; set; }
    public DateTime ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Module Module { get; set; } = null!;
    public User ApprovedByUser { get; set; } = null!;
}
