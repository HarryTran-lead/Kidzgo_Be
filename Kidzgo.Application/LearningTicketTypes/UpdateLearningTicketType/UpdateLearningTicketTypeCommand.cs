using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LearningTicketTypes.GetLearningTicketTypes;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LearningTicketTypes.UpdateLearningTicketType;

public sealed class UpdateLearningTicketTypeCommand : ICommand<LearningTicketTypeDto>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public TicketCompatibilityMode CompatibilityMode { get; init; } = TicketCompatibilityMode.AllowAll;
    public List<SlotDayGroup> AllowedDayGroups { get; init; } = new();
    public List<SlotTimeBand> AllowedTimeBands { get; init; } = new();
    public List<SlotTeacherType> AllowedTeacherTypes { get; init; } = new();
    public List<SlotUsageType> AllowedUsageTypes { get; init; } = new();
    public bool IsActive { get; init; }
}

