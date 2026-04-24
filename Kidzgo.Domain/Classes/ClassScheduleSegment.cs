using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Classes;

public class ClassScheduleSegment : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string WeeklyScheduleJson { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Class Class { get; set; } = null!;
}
