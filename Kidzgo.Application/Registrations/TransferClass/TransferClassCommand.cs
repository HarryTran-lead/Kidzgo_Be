using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Registrations.TransferClass;

public sealed class TransferClassCommand : ICommand<TransferClassResponse>
{
    public Guid RegistrationId { get; init; }
    public Guid NewClassId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string Track { get; init; } = "primary";
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
}
