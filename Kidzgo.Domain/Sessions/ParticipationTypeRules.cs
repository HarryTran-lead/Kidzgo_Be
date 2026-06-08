namespace Kidzgo.Domain.Sessions;

public static class ParticipationTypeRules
{
    private static readonly ParticipationType[] SelectableParticipationTypes =
    [
        ParticipationType.Main,
        ParticipationType.Free
    ];

    public static IReadOnlyList<ParticipationType> SelectableValues => SelectableParticipationTypes;

    public static bool IsSupportedForSessionManagement(ParticipationType participationType)
    {
        return SelectableParticipationTypes.Contains(participationType);
    }

    public static bool ShouldConsumeTicket(ParticipationType participationType)
    {
        return participationType == ParticipationType.Main;
    }
}
