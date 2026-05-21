using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetTeachingLogBySession;

public sealed class GetTeachingLogBySessionQuery : IQuery<GetTeachingLogBySessionResponse>
{
    public Guid SessionId { get; init; }
}
