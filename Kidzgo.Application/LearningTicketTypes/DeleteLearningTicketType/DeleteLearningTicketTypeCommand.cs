using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LearningTicketTypes.DeleteLearningTicketType;

public sealed class DeleteLearningTicketTypeCommand : ICommand
{
    public Guid Id { get; init; }
}

