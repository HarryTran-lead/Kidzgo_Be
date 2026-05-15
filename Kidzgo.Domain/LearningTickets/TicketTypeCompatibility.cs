using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Domain.LearningTickets;

public class TicketTypeCompatibility : Entity
{
    public Guid Id { get; set; }
    public Guid LearningTicketTypeId { get; set; }
    public Guid SlotTypeId { get; set; }
    public bool IsCompatible { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public LearningTicketType LearningTicketType { get; set; } = null!;
    public SlotType SlotType { get; set; } = null!;
}
