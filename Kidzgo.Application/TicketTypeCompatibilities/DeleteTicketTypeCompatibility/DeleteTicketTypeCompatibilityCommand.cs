using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.TicketTypeCompatibilities.DeleteTicketTypeCompatibility;

public sealed class DeleteTicketTypeCompatibilityCommand : ICommand
{
    public Guid Id { get; init; }
}

