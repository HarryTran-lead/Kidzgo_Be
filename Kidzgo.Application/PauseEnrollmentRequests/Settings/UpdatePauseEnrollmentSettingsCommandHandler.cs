using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.PauseEnrollmentRequests.Settings;

public sealed class UpdatePauseEnrollmentSettingsCommandHandler(
    IDbContext context)
    : ICommandHandler<UpdatePauseEnrollmentSettingsCommand, PauseEnrollmentSettingsResponse>
{
    public async Task<Result<PauseEnrollmentSettingsResponse>> Handle(
        UpdatePauseEnrollmentSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.ReservationLimitMonths <= 0)
        {
            return Result.Failure<PauseEnrollmentSettingsResponse>(Error.Validation(
                "PauseEnrollmentSettings.InvalidReservationLimitMonths",
                "Reservation limit months must be greater than 0"));
        }

        var settings = await PauseEnrollmentSettingsHelper.GetOrCreateAsync(context, cancellationToken);
        settings.ReservationLimitMonths = command.ReservationLimitMonths;
        settings.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return PauseEnrollmentSettingsHelper.ToResponse(settings);
    }
}
