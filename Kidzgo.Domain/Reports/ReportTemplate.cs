using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Reports;

public class ReportTemplate : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public ReportTemplateType Type { get; set; }
    public string? ContentSchema { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public User? CreatedByUser { get; set; }
    public ICollection<ReportRun> ReportRuns { get; set; } = new List<ReportRun>();
}
