using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.MakeupCredits.Settings;

public sealed class UpdateMakeupSettingsCommand : ICommand<MakeupSettingsResponse>
{
    public int CreditExpiryDays { get; set; }
}
