namespace Kidzgo.Application.ReportsV3.ReportTemplates.Shared;

public sealed class ReportTemplateDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string ContentSchema { get; init; } = "{}";
    public bool IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public DateTime CreatedAt { get; init; }
}
