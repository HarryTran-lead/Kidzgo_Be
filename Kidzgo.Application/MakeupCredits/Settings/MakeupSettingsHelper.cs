using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.Settings;

public static class MakeupSettingsHelper
{
    public const int DefaultCreditExpiryDays = 7;

    public static async Task<MakeupSettings> GetOrCreateAsync(
        IDbContext context,
        CancellationToken cancellationToken)
    {
        var settings = await context.MakeupSettings
            .FirstOrDefaultAsync(cancellationToken);

        if (settings is not null)
        {
            return settings;
        }

        var now = VietnamTime.UtcNow();
        settings = new MakeupSettings
        {
            Id = 1,
            CreditExpiryDays = DefaultCreditExpiryDays,
            CreatedAt = now
        };

        context.MakeupSettings.Add(settings);
        return settings;
    }

    public static DateTime CalculateExpiresAt(DateTime sourceSessionUtc, MakeupSettings settings)
    {
        var sourceDate = VietnamTime.ToVietnamDateOnly(sourceSessionUtc);
        var expiryDate = sourceDate.AddDays(settings.CreditExpiryDays);
        return VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(expiryDate.ToDateTime(TimeOnly.MinValue)));
    }

    public static MakeupSettingsResponse ToResponse(MakeupSettings settings)
        => new()
        {
            CreditExpiryDays = settings.CreditExpiryDays,
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt
        };
}
