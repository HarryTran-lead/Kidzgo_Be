using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Classes;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PauseEnrollmentRequests.Settings;

public static class PauseEnrollmentSettingsHelper
{
    public const int DefaultReservationLimitMonths = 3;

    public static async Task<PauseEnrollmentSettings> GetOrCreateAsync(
        IDbContext context,
        CancellationToken cancellationToken)
    {
        var settings = await context.PauseEnrollmentSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        settings = new PauseEnrollmentSettings
        {
            Id = 1,
            ReservationLimitMonths = DefaultReservationLimitMonths,
            CreatedAt = VietnamTime.UtcNow()
        };

        context.PauseEnrollmentSettings.Add(settings);
        return settings;
    }

    public static async Task<int> GetReservationLimitMonthsAsync(
        IDbContext context,
        CancellationToken cancellationToken)
        => await context.PauseEnrollmentSettings
            .AsNoTracking()
            .Select(settings => (int?)settings.ReservationLimitMonths)
            .FirstOrDefaultAsync(cancellationToken) ?? DefaultReservationLimitMonths;

    public static PauseEnrollmentSettingsResponse ToResponse(PauseEnrollmentSettings settings)
        => new()
        {
            ReservationLimitMonths = settings.ReservationLimitMonths,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
}
