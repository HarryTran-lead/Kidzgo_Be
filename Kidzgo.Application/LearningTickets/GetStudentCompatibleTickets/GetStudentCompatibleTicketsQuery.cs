using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LearningTickets.GetStudentCompatibleTickets;

public sealed class GetStudentCompatibleTicketsQuery : IQuery<GetStudentCompatibleTicketsResponse>
{
    public Guid StudentProfileId { get; init; }
    public Guid SessionId { get; init; }
}
