using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;

namespace Kidzgo.Application.LearningTicketTypes.UpdateLearningTicketType;

public sealed class UpdateLearningTicketTypeCommand : ICommand<LearningTicketTypeDto>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}

