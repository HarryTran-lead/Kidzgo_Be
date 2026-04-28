using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.CreateRegistrationDiscountCampaign;

public sealed class CreateRegistrationDiscountCampaignCommandHandler(
    IDbContext context
) : ICommandHandler<CreateRegistrationDiscountCampaignCommand, RegistrationDiscountCampaignModel>
{
    public async Task<Result<RegistrationDiscountCampaignModel>> Handle(
        CreateRegistrationDiscountCampaignCommand command,
        CancellationToken cancellationToken)
    {
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

        var now = VietnamTime.UtcNow();
        var campaign = new RegistrationDiscountCampaign
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Code = string.IsNullOrWhiteSpace(command.Code) ? null : command.Code.Trim(),
            Description = string.IsNullOrWhiteSpace(command.Description) ? null : command.Description.Trim(),
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            TuitionPlanId = command.TuitionPlanId,
            DiscountType = command.DiscountType,
            DiscountValue = Math.Round(command.DiscountValue, 2, MidpointRounding.AwayFromZero),
            Priority = command.Priority,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            ApplyForInitialRegistration = command.ApplyForInitialRegistration,
            ApplyForRenewal = command.ApplyForRenewal,
            ApplyForUpgrade = command.ApplyForUpgrade,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.RegistrationDiscountCampaigns.Add(campaign);
        await context.SaveChangesAsync(cancellationToken);

        var today = VietnamTime.ToVietnamDateOnly(now);

        return RegistrationDiscountCampaignReadModelProjector.Map(
            campaign,
            scopeValidation.Value.Branch?.Name,
            scopeValidation.Value.Program?.Name,
            scopeValidation.Value.TuitionPlan?.Name,
            today);
    }
}
