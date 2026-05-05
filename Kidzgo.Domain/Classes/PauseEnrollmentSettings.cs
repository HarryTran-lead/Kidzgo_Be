namespace Kidzgo.Domain.Classes;

public class PauseEnrollmentSettings
{
    public int Id { get; set; }
    public int ReservationLimitMonths { get; set; } = 3;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
