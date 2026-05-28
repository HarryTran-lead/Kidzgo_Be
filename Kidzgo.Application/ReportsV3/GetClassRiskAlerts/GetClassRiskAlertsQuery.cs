using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.GetClassRiskAlerts;

public sealed class GetClassRiskAlertsQuery : IQuery<PagedResult<RiskAlertDto>>
{
    public Guid ClassId { get; init; }
    public RiskType? RiskType { get; init; }
    public RiskSeverity? Severity { get; init; }
    public RiskAlertStatus? Status { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
