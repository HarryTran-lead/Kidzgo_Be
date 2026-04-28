using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaignById;

public sealed class GetRegistrationDiscountCampaignByIdQuery : IQuery<RegistrationDiscountCampaignModel>
{
    public Guid Id { get; init; }
}
