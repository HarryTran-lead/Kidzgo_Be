using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.DeleteReportTemplate;

public sealed class DeleteReportTemplateCommand : ICommand<bool>
{
    public Guid Id { get; init; }
}
