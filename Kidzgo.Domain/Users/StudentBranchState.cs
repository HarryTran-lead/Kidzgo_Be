using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Users;

public class StudentBranchState : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid HomeBranchId { get; set; }
    public Guid ActiveBranchId { get; set; }
    public bool AllowCrossBranchEnrollment { get; set; }
    public DateTime? LastTransferredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Branch HomeBranch { get; set; } = null!;
    public Branch ActiveBranch { get; set; } = null!;
}
