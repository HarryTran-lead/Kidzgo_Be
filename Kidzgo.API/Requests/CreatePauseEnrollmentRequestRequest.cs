namespace Kidzgo.API.Requests;

public sealed class CreatePauseEnrollmentRequestRequest
{
    public Guid StudentProfileId { get; set; }
    public DateOnly PauseFrom { get; set; }
    public DateOnly PauseTo { get; set; }
    public string? Reason { get; set; }
    public Guid? ClassId { get; set; }
}

public sealed class UpdatePauseEnrollmentSettingsRequest
{
    public int ReservationLimitMonths { get; init; }
}
