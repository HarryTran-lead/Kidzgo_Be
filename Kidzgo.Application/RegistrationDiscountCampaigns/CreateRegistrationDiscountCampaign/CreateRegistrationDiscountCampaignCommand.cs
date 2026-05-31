using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.RegistrationDiscountCampaigns.Shared;
using Kidzgo.Domain.Registrations;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.CreateRegistrationDiscountCampaign;

public sealed class CreateRegistrationDiscountCampaignCommand : ICommand<RegistrationDiscountCampaignModel>
{
    public string Name { get; init; } = null!;
    public string? Code { get; init; }
    public string? Description { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public Guid? LevelId { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public RegistrationDiscountType DiscountType { get; init; }
    public decimal DiscountValue { get; init; }
    public int Priority { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public bool ApplyForInitialRegistration { get; init; }
    public bool ApplyForRenewal { get; init; }
    public bool ApplyForUpgrade { get; init; }
}
