namespace Kidzgo.API.Requests;

public sealed class ShareStudentReportRequest
{
    public string Channel { get; set; } = "app";
    public string RecipientName { get; set; } = null!;
    public string RecipientContact { get; set; } = null!;
    public string? ProviderMessageId { get; set; }
}
