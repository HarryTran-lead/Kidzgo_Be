using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Domain.LearningTickets;

public class LearningTicketType : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public TicketCompatibilityMode CompatibilityMode { get; set; }
    public SlotDayGroup AllowedDayGroups { get; set; }
    public SlotTimeBand AllowedTimeBands { get; set; }
    public SlotTeacherType AllowedTeacherTypes { get; set; }
    public SlotUsageType AllowedUsageTypes { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<TicketTypeCompatibility> Compatibilities { get; set; } = new List<TicketTypeCompatibility>();
}
