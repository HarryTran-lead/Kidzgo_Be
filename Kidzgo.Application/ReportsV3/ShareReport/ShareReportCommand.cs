using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class ShareReportCommand : ICommand<ReportShareLogDto>
{
    public Guid ReportId { get; init; }
    public ReportShareChannel Channel { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientContact { get; init; } = string.Empty;
    public string? ProviderMessageId { get; init; }
}
