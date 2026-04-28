using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.GetRegistrationDiscountCampaigns;

public sealed class GetRegistrationDiscountCampaignsQuery : IQuery<GetRegistrationDiscountCampaignsResponse>
{
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
