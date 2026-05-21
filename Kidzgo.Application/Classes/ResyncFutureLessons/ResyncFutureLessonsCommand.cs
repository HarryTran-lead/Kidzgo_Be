using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Classes.ResyncFutureLessons;

public sealed class ResyncFutureLessonsCommand : ICommand<ResyncFutureLessonsResponse>
{
    public Guid ClassId { get; init; }
}
