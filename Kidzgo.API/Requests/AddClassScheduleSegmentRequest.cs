using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class AddClassScheduleSegmentRequest
{
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public List<ScheduleSlot>? WeeklyScheduleSlots { get; set; }
    public bool GenerateSessions { get; set; } = true;
    public bool OnlyFutureSessions { get; set; } = true;
}
