using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportPeriods.Shared;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.GetReportPeriodById;

public sealed class GetReportPeriodByIdQuery : IQuery<ReportPeriodDto>
{
    public Guid Id { get; init; }
}
