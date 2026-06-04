using Kidzgo.Domain.Sessions;

namespace Kidzgo.API.Requests;

public sealed class CreateSlotTypeRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public SlotDayGroup DayGroup { get; set; }
    public SlotTimeBand TimeBand { get; set; }
    public SlotTeacherType TeacherType { get; set; }
    public SlotUsageType UsageType { get; set; }
    public bool IsActive { get; set; } = true;
}

