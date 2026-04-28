namespace Kidzgo.Application.RegistrationDiscountCampaigns.Shared;

public sealed class RegistrationDiscountCampaignModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public Guid? BranchId { get; init; }
    public string? BranchName { get; init; }
    public Guid? ProgramId { get; init; }
    public string? ProgramName { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? TuitionPlanName { get; init; }
    public string DiscountType { get; init; } = null!;
    public decimal DiscountValue { get; init; }
    public int Priority { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool ApplyForInitialRegistration { get; init; }
    public bool ApplyForRenewal { get; init; }
    public bool ApplyForUpgrade { get; init; }
    public bool IsActive { get; init; }
    public bool IsCurrentlyApplicable { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
