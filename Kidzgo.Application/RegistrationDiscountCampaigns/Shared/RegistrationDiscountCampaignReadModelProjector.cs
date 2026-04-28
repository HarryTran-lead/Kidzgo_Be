using Kidzgo.Domain.Registrations;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.Shared;

internal static class RegistrationDiscountCampaignReadModelProjector
{
    internal static IQueryable<RegistrationDiscountCampaignModel> Project(
        IQueryable<RegistrationDiscountCampaign> query,
        DateOnly today)
        => query.Select(campaign => new RegistrationDiscountCampaignModel
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Code = campaign.Code,
            Description = campaign.Description,
            BranchId = campaign.BranchId,
            BranchName = campaign.Branch != null ? campaign.Branch.Name : null,
            ProgramId = campaign.ProgramId,
            ProgramName = campaign.Program != null ? campaign.Program.Name : null,
            TuitionPlanId = campaign.TuitionPlanId,
            TuitionPlanName = campaign.TuitionPlan != null ? campaign.TuitionPlan.Name : null,
            DiscountType = campaign.DiscountType.ToString(),
            DiscountValue = campaign.DiscountValue,
            Priority = campaign.Priority,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            ApplyForInitialRegistration = campaign.ApplyForInitialRegistration,
            ApplyForRenewal = campaign.ApplyForRenewal,
            ApplyForUpgrade = campaign.ApplyForUpgrade,
            IsActive = campaign.IsActive,
            IsCurrentlyApplicable = campaign.IsActive &&
                                    campaign.StartDate <= today &&
                                    campaign.EndDate >= today,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        });

    internal static RegistrationDiscountCampaignModel Map(
        RegistrationDiscountCampaign campaign,
        string? branchName,
        string? programName,
        string? tuitionPlanName,
        DateOnly today)
        => new()
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Code = campaign.Code,
            Description = campaign.Description,
            BranchId = campaign.BranchId,
            BranchName = branchName,
            ProgramId = campaign.ProgramId,
            ProgramName = programName,
            TuitionPlanId = campaign.TuitionPlanId,
            TuitionPlanName = tuitionPlanName,
            DiscountType = campaign.DiscountType.ToString(),
            DiscountValue = campaign.DiscountValue,
            Priority = campaign.Priority,
            StartDate = campaign.StartDate,
            EndDate = campaign.EndDate,
            ApplyForInitialRegistration = campaign.ApplyForInitialRegistration,
            ApplyForRenewal = campaign.ApplyForRenewal,
            ApplyForUpgrade = campaign.ApplyForUpgrade,
            IsActive = campaign.IsActive,
            IsCurrentlyApplicable = campaign.IsActive &&
                                    campaign.StartDate <= today &&
                                    campaign.EndDate >= today,
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt
        };
}
