using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.SlotTypes.GetSlotTypes;

namespace Kidzgo.Application.SlotTypes.GetSlotTypeById;

public sealed class GetSlotTypeByIdQuery : IQuery<SlotTypeDto>
{
    public Guid Id { get; init; }
}

