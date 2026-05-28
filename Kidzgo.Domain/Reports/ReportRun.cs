using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class ReportRun : Entity
{
    public Guid Id { get; set; }
    public Guid ReportTemplateId { get; set; }
    public Guid ReportPeriodId { get; set; }
    public Guid? ClassId { get; set; }
    public Guid? StudentId { get; set; }
    public Guid? BranchId { get; set; }
    public ReportRunStatus Status { get; set; } = ReportRunStatus.Pending;
    public Guid GeneratedBy { get; set; }
    public DateTime GeneratedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? ScopeHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ReportTemplate ReportTemplate { get; set; } = null!;
    public ReportPeriod ReportPeriod { get; set; } = null!;
    public Class? Class { get; set; }
    public Profile? Student { get; set; }
    public Branch? Branch { get; set; }
    public User GeneratedByUser { get; set; } = null!;
    public ICollection<StudentReport> StudentReports { get; set; } = new List<StudentReport>();
}
