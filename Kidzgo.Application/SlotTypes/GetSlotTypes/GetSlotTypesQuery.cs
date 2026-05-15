using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.SlotTypes.GetSlotTypes;

public sealed class GetSlotTypesQuery : IQuery<GetSlotTypesResponse>
{
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
}

