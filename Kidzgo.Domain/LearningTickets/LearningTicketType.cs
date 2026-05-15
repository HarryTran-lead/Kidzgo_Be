using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LearningTickets;

public class LearningTicketType : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TicketTypeCompatibility> Compatibilities { get; set; } = new List<TicketTypeCompatibility>();
}
