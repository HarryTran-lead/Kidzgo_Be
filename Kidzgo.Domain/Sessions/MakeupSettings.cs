namespace Kidzgo.Domain.Sessions;

public class MakeupSettings
{
    public int Id { get; set; }
    public int CreditExpiryDays { get; set; } = 7;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
