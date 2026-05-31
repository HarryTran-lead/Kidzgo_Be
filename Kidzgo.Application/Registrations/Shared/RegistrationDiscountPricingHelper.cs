using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.Shared;

internal static class RegistrationDiscountPricingHelper
{
    internal static async Task<OperationType> ResolveInitialOrRenewalOperationTypeAsync(
        IDbContext context,
        Guid studentProfileId,
        Guid? excludeRegistrationId,
        CancellationToken cancellationToken)
    {
        var hasPreviousRegistration = await context.Registrations
            .AsNoTracking()
            .AnyAsync(
                r => r.StudentProfileId == studentProfileId &&
                     (!excludeRegistrationId.HasValue || r.Id != excludeRegistrationId.Value),
                cancellationToken);

        return hasPreviousRegistration
            ? OperationType.Renewal
            : OperationType.Initial;
    }

    internal static async Task<RegistrationDiscountPricingResult> ResolveAsync(
        IDbContext context,
        Guid branchId,
        Guid programId,
        Guid levelId,
        Guid tuitionPlanId,
        OperationType operationType,
        DateTime registrationDate,
        decimal originalTuitionAmount,
        decimal carryOverCreditAmount,
        CancellationToken cancellationToken)
    {
        var normalizedOriginalAmount = Math.Max(originalTuitionAmount, 0m);
        var normalizedCarryOver = Math.Max(carryOverCreditAmount, 0m);
        var registrationDateOnly = VietnamTime.ToVietnamDateOnly(registrationDate);

        var campaign = await context.RegistrationDiscountCampaigns
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Where(c => c.StartDate <= registrationDateOnly && c.EndDate >= registrationDateOnly)
            .Where(c => !c.BranchId.HasValue || c.BranchId == branchId)
            .Where(c => !c.ProgramId.HasValue || c.ProgramId == programId)
            .Where(c => !c.LevelId.HasValue || c.LevelId == levelId)
            .Where(c => !c.TuitionPlanId.HasValue || c.TuitionPlanId == tuitionPlanId)
            .Where(c =>
                operationType == OperationType.Initial && c.ApplyForInitialRegistration ||
                operationType == OperationType.Renewal && c.ApplyForRenewal ||
                (operationType == OperationType.Upgrade || operationType == OperationType.Promotion) && c.ApplyForUpgrade)
            .OrderByDescending(c => c.Priority)
            .ThenByDescending(c => c.StartDate)
            .ThenByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var discountAmount = CalculateDiscountAmount(campaign, normalizedOriginalAmount);
        var finalAmount = Math.Max(0m, normalizedOriginalAmount - discountAmount - normalizedCarryOver);

        return new RegistrationDiscountPricingResult
        {
            OperationType = operationType,
            DiscountCampaignId = campaign?.Id,
            DiscountCampaignName = campaign?.Name,
            DiscountType = campaign?.DiscountType,
            DiscountValue = campaign?.DiscountValue,
            OriginalTuitionAmount = normalizedOriginalAmount,
            DiscountAmount = discountAmount,
            CarryOverCreditAmount = normalizedCarryOver,
            FinalTuitionAmount = finalAmount,
            PricingAppliedAt = registrationDate
        };
    }

    internal static void ApplyToRegistration(
        Registration registration,
        RegistrationDiscountPricingResult pricing)
    {
        registration.OperationType = pricing.OperationType;
        registration.DiscountCampaignId = pricing.DiscountCampaignId;
        registration.DiscountCampaignName = pricing.DiscountCampaignName;
        registration.DiscountType = pricing.DiscountType;
        registration.DiscountValue = pricing.DiscountValue;
        registration.OriginalTuitionAmount = pricing.OriginalTuitionAmount;
        registration.DiscountAmount = pricing.DiscountAmount;
        registration.CarryOverCreditAmount = pricing.CarryOverCreditAmount;
        registration.FinalTuitionAmount = pricing.FinalTuitionAmount;
        registration.PricingAppliedAt = pricing.PricingAppliedAt;
    }

    private static decimal CalculateDiscountAmount(
        RegistrationDiscountCampaign? campaign,
        decimal originalTuitionAmount)
    {
        if (campaign is null || originalTuitionAmount <= 0m)
        {
            return 0m;
        }

        return campaign.DiscountType switch
        {
            RegistrationDiscountType.Percentage => Math.Round(
                Math.Clamp(campaign.DiscountValue, 0m, 100m) * originalTuitionAmount / 100m,
                2,
                MidpointRounding.AwayFromZero),
            RegistrationDiscountType.FixedAmount => Math.Min(
                Math.Max(campaign.DiscountValue, 0m),
                originalTuitionAmount),
            _ => 0m
        };
    }
}

internal sealed class RegistrationDiscountPricingResult
{
    public OperationType OperationType { get; init; }
    public Guid? DiscountCampaignId { get; init; }
    public string? DiscountCampaignName { get; init; }
    public RegistrationDiscountType? DiscountType { get; init; }
    public decimal? DiscountValue { get; init; }
    public decimal OriginalTuitionAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CarryOverCreditAmount { get; init; }
    public decimal FinalTuitionAmount { get; init; }
    public DateTime PricingAppliedAt { get; init; }
}
