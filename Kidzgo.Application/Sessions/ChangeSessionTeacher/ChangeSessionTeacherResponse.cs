namespace Kidzgo.Application.Sessions.ChangeSessionTeacher;

public sealed class ChangeSessionTeacherResponse
{
    public int UpdatedSessionsCount { get; set; }
    public List<Guid> UpdatedSessionIds { get; init; } = new();
    public List<Guid> SkippedSessionIds { get; init; } = new();
    public List<string> Errors { get; init; } = new();
}
