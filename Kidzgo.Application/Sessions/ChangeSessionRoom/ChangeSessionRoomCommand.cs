using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.ChangeSessionRoom;

public sealed class ChangeSessionRoomCommand : ICommand<ChangeSessionRoomResponse>
{
    public IReadOnlyList<Guid> SessionIds { get; init; } = [];
    public Guid RoomId { get; init; }
}

