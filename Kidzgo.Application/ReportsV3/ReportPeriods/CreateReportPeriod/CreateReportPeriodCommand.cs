using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.CreateReportPeriod;

public sealed class CreateReportPeriodCommand : ICommand<ReportPeriodDto>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public ReportPeriodType Type { get; init; } = ReportPeriodType.Monthly;
}
