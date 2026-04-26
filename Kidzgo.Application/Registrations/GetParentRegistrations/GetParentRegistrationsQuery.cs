using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.GetRegistrations;

namespace Kidzgo.Application.Registrations.GetParentRegistrations;

public sealed class GetParentRegistrationsQuery : IQuery<GetRegistrationsResponse>
{
    public Guid? StudentProfileId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public string? Status { get; init; }
    public Guid? ClassId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
