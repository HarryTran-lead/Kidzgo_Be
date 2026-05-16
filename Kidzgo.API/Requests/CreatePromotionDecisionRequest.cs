namespace Kidzgo.API.Requests;

public sealed class CreatePromotionDecisionRequest
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string? Reason { get; init; }
    public DateTime? ApprovedAt { get; init; }
}
