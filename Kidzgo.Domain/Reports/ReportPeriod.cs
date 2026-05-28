using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Reports;

public class ReportPeriod : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public ReportPeriodType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<ReportRun> ReportRuns { get; set; } = new List<ReportRun>();
    public ICollection<StudentReport> StudentReports { get; set; } = new List<StudentReport>();
    public ICollection<RiskAlert> RiskAlerts { get; set; } = new List<RiskAlert>();
}
