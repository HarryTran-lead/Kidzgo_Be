using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Classes.AddClassScheduleSegment;

public sealed class AddClassScheduleSegmentResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public List<ScheduleSlot> WeeklyScheduleSlots { get; init; } = [];
    public int GeneratedSessionsCount { get; init; }
}
