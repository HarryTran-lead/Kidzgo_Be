using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.API.Requests;

public sealed class CreateLearningTicketTypeRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public TicketCompatibilityMode CompatibilityMode { get; set; } = TicketCompatibilityMode.AllowAll;
    public List<SlotDayGroup> AllowedDayGroups { get; set; } = new();
    public List<SlotTimeBand> AllowedTimeBands { get; set; } = new();
    public List<SlotTeacherType> AllowedTeacherTypes { get; set; } = new();
    public List<SlotUsageType> AllowedUsageTypes { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

