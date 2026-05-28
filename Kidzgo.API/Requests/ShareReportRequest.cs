namespace Kidzgo.API.Requests;

public sealed class ShareReportRequest
{
    public string Channel { get; set; } = "app";
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientContact { get; set; } = string.Empty;
    public string? ProviderMessageId { get; set; }
}
