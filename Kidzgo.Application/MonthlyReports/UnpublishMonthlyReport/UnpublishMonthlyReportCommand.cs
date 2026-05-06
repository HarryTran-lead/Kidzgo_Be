using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MonthlyReports.UnpublishMonthlyReport;

public sealed class UnpublishMonthlyReportCommand : ICommand<UnpublishMonthlyReportResponse>
{
    public Guid ReportId { get; init; }
}
