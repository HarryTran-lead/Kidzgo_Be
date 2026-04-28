using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.ToggleRegistrationDiscountCampaignStatus;

public sealed class ToggleRegistrationDiscountCampaignStatusCommand : ICommand<ToggleRegistrationDiscountCampaignStatusResponse>
{
    public Guid Id { get; init; }
}
