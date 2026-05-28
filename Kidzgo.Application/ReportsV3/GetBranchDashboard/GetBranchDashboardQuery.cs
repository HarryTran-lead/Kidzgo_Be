using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.GetBranchDashboard;

public sealed class GetBranchDashboardQuery : IQuery<BranchDashboardResponse>
{
    public Guid BranchId { get; init; }
}
