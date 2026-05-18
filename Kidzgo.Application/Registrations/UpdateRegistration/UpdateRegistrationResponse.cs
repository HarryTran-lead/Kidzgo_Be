namespace Kidzgo.Application.Registrations.UpdateRegistration;

public sealed class UpdateRegistrationResponse
{
    public Guid Id { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? TuitionPlanName { get; init; }
    public Guid? SecondaryLevelId { get; init; }
    public string? SecondaryLevelName { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    public string? OperationType { get; init; }
    public Guid? DiscountCampaignId { get; init; }
    public string? DiscountCampaignName { get; init; }
    public string? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal? OriginalTuitionAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CarryOverCreditAmount { get; init; }
    public decimal? FinalTuitionAmount { get; init; }
    public DateTime UpdatedAt { get; init; }
}
