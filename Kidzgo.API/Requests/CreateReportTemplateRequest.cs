namespace Kidzgo.API.Requests;

public sealed class CreateReportTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "parent";
    public string? ContentSchema { get; set; }
    public bool IsActive { get; set; } = true;
}
