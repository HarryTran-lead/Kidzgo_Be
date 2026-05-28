using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.GetReportTemplates;

public sealed class GetReportTemplatesQuery : IQuery<PagedResult<ReportTemplateDto>>
{
    public ReportTemplateType? Type { get; init; }
    public bool? IsActive { get; init; }
    public string? Q { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
