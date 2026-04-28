using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.UpdateRegistrationDiscountCampaign;

public sealed class UpdateRegistrationDiscountCampaignCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateRegistrationDiscountCampaignCommand, RegistrationDiscountCampaignModel>
{
    public async Task<Result<RegistrationDiscountCampaignModel>> Handle(
        UpdateRegistrationDiscountCampaignCommand command,
        CancellationToken cancellationToken)
    {
        var campaign = await context.RegistrationDiscountCampaigns
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (campaign is null)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(
                RegistrationDiscountCampaignErrors.NotFound(command.Id));
        }

        if (command.EndDate < command.StartDate)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(
                RegistrationDiscountCampaignErrors.InvalidDateRange);
        }

        if (command.DiscountValue <= 0m)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(
                RegistrationDiscountCampaignErrors.InvalidDiscountValue);
        }

        if (command.DiscountType == RegistrationDiscountType.Percentage &&
            (command.DiscountValue <= 0m || command.DiscountValue > 100m))
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(
                RegistrationDiscountCampaignErrors.InvalidPercentageDiscountValue);
        }

        if (!command.ApplyForInitialRegistration &&
            !command.ApplyForRenewal &&
            !command.ApplyForUpgrade)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(
                RegistrationDiscountCampaignErrors.MissingApplicability);
        }

        var scopeValidation = await RegistrationDiscountCampaignValidationHelper.ValidateScopeAsync(
            context,
            command.BranchId,
            command.ProgramId,
            command.TuitionPlanId,
            cancellationToken);

        if (scopeValidation.IsFailure)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(scopeValidation.Error);
        }

        var fixedAmountValidation = RegistrationDiscountCampaignValidationHelper.ValidateFixedAmountAgainstTuitionPlan(
            command.DiscountType,
            command.DiscountValue,
            scopeValidation.Value.TuitionPlan);

        if (fixedAmountValidation.IsFailure)
        {
            return Result.Failure<RegistrationDiscountCampaignModel>(fixedAmountValidation.Error);
        }

        campaign.Name = command.Name.Trim();
        campaign.Code = string.IsNullOrWhiteSpace(command.Code) ? null : command.Code.Trim();
        campaign.Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim();
        campaign.BranchId = command.BranchId;
        campaign.ProgramId = command.ProgramId;
        campaign.TuitionPlanId = command.TuitionPlanId;
        campaign.DiscountType = command.DiscountType;
        campaign.DiscountValue = Math.Round(command.DiscountValue, 2, MidpointRounding.AwayFromZero);
        campaign.Priority = command.Priority;
        campaign.StartDate = command.StartDate;
        campaign.EndDate = command.EndDate;
        campaign.ApplyForInitialRegistration = command.ApplyForInitialRegistration;
        campaign.ApplyForRenewal = command.ApplyForRenewal;
        campaign.ApplyForUpgrade = command.ApplyForUpgrade;
        campaign.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        var today = VietnamTime.ToVietnamDateOnly(campaign.UpdatedAt);

        return RegistrationDiscountCampaignReadModelProjector.Map(
            campaign,
            scopeValidation.Value.Branch?.Name,
            scopeValidation.Value.Program?.Name,
            scopeValidation.Value.TuitionPlan?.Name,
            today);
    }
}
