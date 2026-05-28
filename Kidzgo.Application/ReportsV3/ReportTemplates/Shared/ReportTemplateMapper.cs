using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.Shared;

internal static class ReportTemplateMapper
{
    public static ReportTemplateDto ToDto(ReportTemplate template)
    {
        return new ReportTemplateDto
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Type = template.Type.ToString(),
            ContentSchema = string.IsNullOrWhiteSpace(template.ContentSchema) ? "{}" : template.ContentSchema,
            IsActive = template.IsActive,
            CreatedBy = template.CreatedBy,
            CreatedAt = template.CreatedAt
        };
    }
}
