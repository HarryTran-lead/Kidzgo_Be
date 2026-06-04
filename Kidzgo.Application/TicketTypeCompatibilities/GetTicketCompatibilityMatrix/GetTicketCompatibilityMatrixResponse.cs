using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.TicketTypeCompatibilities.GetTicketCompatibilityMatrix;

public sealed class GetTicketCompatibilityMatrixResponse
{
    public List<TicketCompatibilityMatrixLearningTicketTypeDto> LearningTicketTypes { get; init; } = new();
    public List<TicketCompatibilityMatrixSlotTypeDto> SlotTypes { get; init; } = new();
    public List<TicketCompatibilityMatrixCellDto> Cells { get; init; } = new();
}

public sealed class TicketCompatibilityMatrixLearningTicketTypeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public TicketCompatibilityMode CompatibilityMode { get; init; }
    public bool IsActive { get; init; }
}

public sealed class TicketCompatibilityMatrixSlotTypeDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public SlotDayGroup DayGroup { get; init; }
    public SlotTimeBand TimeBand { get; init; }
    public SlotTeacherType TeacherType { get; init; }
    public SlotUsageType UsageType { get; init; }
    public bool IsActive { get; init; }
}

public sealed class TicketCompatibilityMatrixCellDto
{
    public Guid LearningTicketTypeId { get; init; }
    public Guid SlotTypeId { get; init; }
    public bool IsCompatible { get; init; }
    public bool? OverrideValue { get; init; }
    public string Source { get; init; } = null!;
    public string Reason { get; init; } = null!;
}
