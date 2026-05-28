using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.PublishReportToParent;

public sealed class PublishReportToParentCommand : ICommand<PublishReportToParentResponse>
{
    public Guid ReportId { get; init; }
}
