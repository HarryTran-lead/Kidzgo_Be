using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.ChangeSessionTeacher;

public sealed class ChangeSessionTeacherCommand : ICommand<ChangeSessionTeacherResponse>
{
    public IReadOnlyList<Guid> SessionIds { get; init; } = [];
    public Guid TeacherId { get; init; }
    public SessionTeacherRole Role { get; init; }
}

