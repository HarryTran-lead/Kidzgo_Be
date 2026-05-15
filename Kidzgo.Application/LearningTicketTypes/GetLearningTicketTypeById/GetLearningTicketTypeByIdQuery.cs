using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;

namespace Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypeById;

public sealed class GetLearningTicketTypeByIdQuery : IQuery<LearningTicketTypeDto>
{
    public Guid Id { get; init; }
}

