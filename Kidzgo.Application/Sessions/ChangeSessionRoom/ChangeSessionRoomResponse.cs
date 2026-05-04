namespace Kidzgo.Application.Sessions.ChangeSessionRoom;

public sealed class ChangeSessionRoomResponse
{
    public int UpdatedSessionsCount { get; set; }
    public List<Guid> UpdatedSessionIds { get; init; } = new();
    public List<Guid> SkippedSessionIds { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}
