namespace Kidzgo.API.Requests;

public sealed class ChangeSessionRoomRequest
{
    public Guid? SessionId { get; set; }
    public List<Guid>? SessionIds { get; set; }
    public Guid RoomId { get; set; }
}

