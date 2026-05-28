using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class UpdateShareCallbackCommand : ICommand<ReportShareLogDto>
{
    public string ProviderMessageId { get; init; } = string.Empty;
    public ReportShareStatus Status { get; init; }
    public DateTime? ViewedAt { get; init; }
    public string? ErrorMessage { get; init; }
}
