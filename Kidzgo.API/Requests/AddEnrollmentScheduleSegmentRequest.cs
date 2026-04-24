using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class AddEnrollmentScheduleSegmentRequest
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public List<WeeklyPatternEntry>? WeeklyPattern { get; set; }
    public bool ClearWeeklyPattern { get; set; }
}
