namespace Kidzgo.Application.RegistrationDiscountCampaigns.ToggleRegistrationDiscountCampaignStatus;

public sealed class ToggleRegistrationDiscountCampaignStatusResponse
{
    public Guid Id { get; init; }
    public bool IsActive { get; init; }
    public DateTime UpdatedAt { get; init; }
}
