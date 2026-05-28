using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.GetParentReport;

public sealed class GetParentReportQuery : IQuery<ParentReportViewResponse>
{
    public Guid StudentId { get; init; }
}
