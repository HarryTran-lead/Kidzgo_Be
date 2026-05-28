using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class RiskAlert : Entity
{
    public Guid Id { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid ReportPeriodId { get; set; }
    public RiskType RiskType { get; set; }
    public RiskSeverity Severity { get; set; }
    public string Reason { get; set; } = null!;
    public string? Source { get; set; }
    public RiskAlertStatus Status { get; set; } = RiskAlertStatus.Open;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public Profile? Student { get; set; }
    public Class? Class { get; set; }
    public Branch? Branch { get; set; }
    public ReportPeriod ReportPeriod { get; set; } = null!;
}
