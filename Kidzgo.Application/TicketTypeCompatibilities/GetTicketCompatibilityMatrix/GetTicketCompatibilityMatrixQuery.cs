using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketCompatibilityMatrix;

public sealed class GetTicketCompatibilityMatrixQuery : IQuery<GetTicketCompatibilityMatrixResponse>
{
    public Guid? LearningTicketTypeId { get; init; }
    public bool OnlyActive { get; init; } = true;
}
