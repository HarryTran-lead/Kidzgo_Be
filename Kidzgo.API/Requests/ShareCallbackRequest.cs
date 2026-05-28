namespace Kidzgo.API.Requests;

public sealed class ShareCallbackRequest
{
    public string ProviderMessageId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ViewedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
