using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.DeleteReportPeriod;

public sealed class DeleteReportPeriodCommand : ICommand
{
    public Guid Id { get; init; }
}
