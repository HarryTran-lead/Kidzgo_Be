using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class Recommendation : Entity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid? ClassId { get; set; }
    public RiskType RecommendationType { get; set; }
    public string Content { get; set; } = null!;
    public RecommendationPriority Priority { get; set; }
    public UserRole AssignedRole { get; set; }
    public RecommendationStatus Status { get; set; } = RecommendationStatus.Pending;
    public DateTime DueAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public Profile Student { get; set; } = null!;
    public Class? Class { get; set; }
}
