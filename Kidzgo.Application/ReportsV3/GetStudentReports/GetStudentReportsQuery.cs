using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.GetStudentReports;

public sealed class GetStudentReportsQuery : IQuery<PagedResult<StudentReportListItemDto>>
{
    public Guid StudentId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? PeriodId { get; init; }
    public StudentReportType? ReportType { get; init; }
    public StudentReportStatus? Status { get; init; }
    public string? Q { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
