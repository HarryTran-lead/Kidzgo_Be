namespace Kidzgo.Application.MakeupCredits.Settings;

public sealed class MakeupSettingsResponse
{
    public int CreditExpiryDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
