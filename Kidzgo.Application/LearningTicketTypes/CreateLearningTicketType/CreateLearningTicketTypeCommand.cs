using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;

namespace Kidzgo.Application.LearningTicketTypes.CreateLearningTicketType;

public sealed class CreateLearningTicketTypeCommand : ICommand<LearningTicketTypeDto>
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; } = true;
}

