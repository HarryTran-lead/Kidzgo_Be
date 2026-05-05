using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PauseEnrollmentRequests.Settings;

public sealed class UpdatePauseEnrollmentSettingsCommand : ICommand<PauseEnrollmentSettingsResponse>
{
    public int ReservationLimitMonths { get; init; }
}
