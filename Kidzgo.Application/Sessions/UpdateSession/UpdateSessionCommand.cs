using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.UpdateSession;

public sealed class UpdateSessionCommand : ICommand<UpdateSessionResponse>
{
    public Guid SessionId { get; init; }
    public DateTime PlannedDatetime { get; init; }
    public int DurationMinutes { get; init; }
    public Guid? PlannedRoomId { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public Guid? PlannedAssistantId { get; init; }
    public Guid? SlotTypeId { get; init; }
    public ParticipationType ParticipationType { get; init; }
    public SectionType? SectionType { get; init; }
}


