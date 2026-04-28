using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.ToggleRegistrationDiscountCampaignStatus;

public sealed class ToggleRegistrationDiscountCampaignStatusCommandHandler(
    IDbContext context
) : ICommandHandler<ToggleRegistrationDiscountCampaignStatusCommand, ToggleRegistrationDiscountCampaignStatusResponse>
{
    public async Task<Result<ToggleRegistrationDiscountCampaignStatusResponse>> Handle(
        ToggleRegistrationDiscountCampaignStatusCommand command,
        CancellationToken cancellationToken)
    {
        var campaign = await context.RegistrationDiscountCampaigns
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<ToggleRegistrationDiscountCampaignStatusResponse>(
                RegistrationDiscountCampaignErrors.NotFound(command.Id));
        }

        campaign.IsActive = !campaign.IsActive;
        campaign.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new ToggleRegistrationDiscountCampaignStatusResponse
        {
            Id = campaign.Id,
            IsActive = campaign.IsActive,
            UpdatedAt = campaign.UpdatedAt
        };
    }
}
