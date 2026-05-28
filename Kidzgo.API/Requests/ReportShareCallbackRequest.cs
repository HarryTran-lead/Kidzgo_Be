namespace Kidzgo.API.Requests;

public sealed class ReportShareCallbackRequest
{
    public string ProviderMessageId { get; set; } = null!;
    public string Status { get; set; } = "viewed";
    public string? ErrorMessage { get; set; }
    public DateTime? ViewedAt { get; set; }
}
