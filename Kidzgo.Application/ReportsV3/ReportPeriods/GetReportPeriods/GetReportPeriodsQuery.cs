using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.GetReportPeriods;

public sealed class GetReportPeriodsQuery : IQuery<PagedResult<ReportPeriodDto>>
{
    public ReportPeriodType? Type { get; init; }
    public DateOnly? From { get; init; }
    public DateOnly? To { get; init; }
    public string? Q { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
