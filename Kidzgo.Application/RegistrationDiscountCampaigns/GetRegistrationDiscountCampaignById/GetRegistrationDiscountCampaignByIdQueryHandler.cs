using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaignById;

public sealed class GetRegistrationDiscountCampaignByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationDiscountCampaignByIdQuery, RegistrationDiscountCampaignModel>
{
    public async Task<Result<RegistrationDiscountCampaignModel>> Handle(
        GetRegistrationDiscountCampaignByIdQuery query,
        CancellationToken cancellationToken)
    {
        var today = VietnamTime.ToVietnamDateOnly(VietnamTime.UtcNow());
        var campaign = await RegistrationDiscountCampaignReadModelProjector
            .Project(
                context.RegistrationDiscountCampaigns
                    .AsNoTracking()
                    .Where(x => x.Id == query.Id),
                today)
            .FirstOrDefaultAsync(cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(
                RegistrationDiscountCampaignErrors.NotFound(query.Id));
        }

        return campaign;
    }
}
