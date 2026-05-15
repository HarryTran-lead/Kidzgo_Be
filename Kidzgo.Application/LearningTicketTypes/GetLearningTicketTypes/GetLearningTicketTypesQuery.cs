using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;

public sealed class GetLearningTicketTypesQuery : IQuery<GetLearningTicketTypesResponse>
{
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
}

