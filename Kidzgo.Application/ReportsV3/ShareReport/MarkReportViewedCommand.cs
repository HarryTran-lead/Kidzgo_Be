using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class MarkReportViewedCommand : ICommand<ReportShareLogDto>
{
    public Guid ReportId { get; init; }
}
