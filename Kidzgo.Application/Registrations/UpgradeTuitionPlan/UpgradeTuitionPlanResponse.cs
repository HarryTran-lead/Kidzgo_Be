namespace Kidzgo.Application.Registrations.UpgradeTuitionPlan;

public sealed class UpgradeTuitionPlanResponse
{
    public Guid OriginalRegistrationId { get; init; }
    public Guid NewRegistrationId { get; init; }
    public string OldTuitionPlanName { get; init; } = null!;
    public string NewTuitionPlanName { get; init; } = null!;
    public int OldTotalSessions { get; init; }
    public int NewTotalSessions { get; init; }
    public int AddedSessions { get; init; }
    public Guid? DiscountCampaignId { get; init; }
    public string? DiscountCampaignName { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal OriginalTuitionAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CarryOverCreditAmount { get; init; }
    public decimal FinalTuitionAmount { get; init; }
    public string Status { get; init; } = null!;
}
