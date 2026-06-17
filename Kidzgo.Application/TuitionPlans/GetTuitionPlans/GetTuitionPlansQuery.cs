using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.TuitionPlans.GetTuitionPlans;

public sealed class GetTuitionPlansQuery : IQuery<GetTuitionPlansResponse>, IPageableQuery
{
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public Guid? LevelId { get; init; }
    public bool? IsActive { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

