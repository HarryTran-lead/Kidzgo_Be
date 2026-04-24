using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.PauseEnrollmentRequests.ReassignEquivalentClass;

public sealed class ReassignEquivalentClassCommand : ICommand<ReassignEquivalentClassResponse>
{
    public Guid PauseEnrollmentRequestId { get; init; }
    public Guid RegistrationId { get; init; }
    public Guid NewClassId { get; init; }
    public string Track { get; init; } = "primary";
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public DateTime? EffectiveDate { get; init; }
}
