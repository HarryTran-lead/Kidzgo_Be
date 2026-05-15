using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.CreateSession;

public sealed class CreateSessionCommand : ICommand<CreateSessionResponse>
{
    public Guid ClassId { get; init; }
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? PlannedRoomId { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public Guid? PlannedAssistantId { get; init; }
    public Guid? SlotTypeId { get; init; }
    public ParticipationType ParticipationType { get; init; } = ParticipationType.Main;
    public SectionType SectionType { get; init; } = SectionType.Normal;
}



