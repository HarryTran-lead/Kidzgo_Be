using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class StudentReport : Entity
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid ClassId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid? SyllabusId { get; set; }
    public Guid ReportPeriodId { get; set; }
    public Guid ReportRunId { get; set; }
    public StudentReportType ReportType { get; set; }
    public string SnapshotJson { get; set; } = null!;
    public string? SummaryText { get; set; }
    public StudentReportStatus Status { get; set; } = StudentReportStatus.Pending;
    public bool IsParentPublished { get; set; }
    public DateTime? ParentPublishedAt { get; set; }
    public Guid? ParentPublishedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Profile Student { get; set; } = null!;
    public Class Class { get; set; } = null!;
    public Branch Branch { get; set; } = null!;
    public ReportPeriod ReportPeriod { get; set; } = null!;
    public ReportRun ReportRun { get; set; } = null!;
    public User? ParentPublishedByUser { get; set; }
    public ICollection<AIInsight> AIInsights { get; set; } = new List<AIInsight>();
    public ICollection<ReportShareLog> ShareLogs { get; set; } = new List<ReportShareLog>();
}
