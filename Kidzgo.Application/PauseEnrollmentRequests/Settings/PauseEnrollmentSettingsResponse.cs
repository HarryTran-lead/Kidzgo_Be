namespace Kidzgo.Application.PauseEnrollmentRequests.Settings;

public sealed class PauseEnrollmentSettingsResponse
{
    public int ReservationLimitMonths { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
