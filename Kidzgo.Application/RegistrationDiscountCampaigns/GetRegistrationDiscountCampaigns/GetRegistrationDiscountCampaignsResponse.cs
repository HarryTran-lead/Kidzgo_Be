using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaigns;

public sealed class GetRegistrationDiscountCampaignsResponse
{
    public Page<RegistrationDiscountCampaignModel> Campaigns { get; init; } = null!;
}
