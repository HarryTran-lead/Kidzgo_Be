using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Sessions;

public class SlotType : Entity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public SlotDayGroup DayGroup { get; set; }
    public SlotTimeBand TimeBand { get; set; }
    public SlotTeacherType TeacherType { get; set; }
    public SlotUsageType UsageType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
