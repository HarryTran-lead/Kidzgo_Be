using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilities;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketTypeCompatibilityById;

public sealed class GetTicketTypeCompatibilityByIdQuery : IQuery<TicketTypeCompatibilityDto>
{
    public Guid Id { get; init; }
}

