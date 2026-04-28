using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;

namespace Kidzgo.Application.MakeupCredits.Settings;

public sealed class UpdateMakeupSettingsCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateMakeupSettingsCommand, MakeupSettingsResponse>
{
    public async Task<Result<MakeupSettingsResponse>> Handle(
        UpdateMakeupSettingsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.CreditExpiryDays <= 0)
        {
            return Result.Failure<MakeupSettingsResponse>(MakeupSettingsErrors.InvalidCreditExpiryDays);
        }

        var settings = await MakeupSettingsHelper.GetOrCreateAsync(context, cancellationToken);
        settings.CreditExpiryDays = command.CreditExpiryDays;
        settings.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return MakeupSettingsHelper.ToResponse(settings);
    }
}
