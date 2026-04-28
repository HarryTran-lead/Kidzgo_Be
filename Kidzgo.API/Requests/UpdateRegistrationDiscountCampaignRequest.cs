using Kidzgo.Domain.Registrations;

namespace Kidzgo.API.Requests;

public sealed class UpdateRegistrationDiscountCampaignRequest
{
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ProgramId { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public RegistrationDiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public int Priority { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool ApplyForInitialRegistration { get; set; }
    public bool ApplyForRenewal { get; set; }
    public bool ApplyForUpgrade { get; set; }
}
