using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions.Errors;

public static class MakeupSettingsErrors
{
    public static readonly Error InvalidCreditExpiryDays = Error.Validation(
        "MakeupSettings.InvalidCreditExpiryDays",
        "CreditExpiryDays must be greater than 0.");
}
