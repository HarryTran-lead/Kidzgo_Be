namespace Kidzgo.Application.ReportsV3.GetBranchDashboard;

public sealed class BranchDashboardResponse
{
    public Guid BranchId { get; init; }
    public int TotalActiveClasses { get; init; }
    public int TotalActiveStudents { get; init; }
    public int RiskStudents { get; init; }
    public int RiskClasses { get; init; }
    public int PackageExpiringCount { get; init; }
    public int AssessmentFailCount { get; init; }
}
