using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.ReportTemplates.Shared;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.GetReportTemplateById;

public sealed class GetReportTemplateByIdQuery : IQuery<ReportTemplateDto>
{
    public Guid Id { get; init; }
}
