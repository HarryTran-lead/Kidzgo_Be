using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GetWaitingList;

public sealed class GetWaitingListQuery : IQuery<GetWaitingListResponse>
{
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public Guid? LevelId { get; init; }
    public string? Track { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
