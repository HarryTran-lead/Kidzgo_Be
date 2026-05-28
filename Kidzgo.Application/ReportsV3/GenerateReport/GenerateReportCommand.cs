using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.GenerateReport;

public sealed class GenerateReportCommand : ICommand<GenerateReportResponse>
{
    public Guid StudentId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid PeriodId { get; init; }
    public StudentReportType ReportType { get; init; } = StudentReportType.Parent;
    public string? IdempotencyKey { get; init; }
}
