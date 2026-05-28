using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.UpdateReportTemplate;

public sealed class UpdateReportTemplateCommand : ICommand<ReportTemplateDto>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ReportTemplateType Type { get; init; } = ReportTemplateType.Parent;
    public string? ContentSchema { get; init; }
    public bool IsActive { get; init; } = true;
}
