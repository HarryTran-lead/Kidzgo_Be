namespace Kidzgo.Application.Abstraction.Services;

public sealed class ScheduleSlot
{
    public string DayOfWeek { get; set; } = null!;
    public string StartTime { get; set; } = null!;
    public int DurationMinutes { get; set; }
}

public sealed class WeeklyPatternEntry
{
    public List<string> DayOfWeeks { get; set; } = [];
    public string StartTime { get; set; } = null!;
    public int DurationMinutes { get; set; }
}

public sealed class ScheduleOccurrence
{
    public DateTime PlannedDatetime { get; set; }
    public int DurationMinutes { get; set; }
}

public readonly record struct WeeklyScheduleSegmentWindow(
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    string WeeklyScheduleJson);
