using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Enrollments.AddEnrollmentScheduleSegment;

public sealed class AddEnrollmentScheduleSegmentCommand : ICommand<AddEnrollmentScheduleSegmentResponse>
{
    public Guid EnrollmentId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public bool ClearWeeklyPattern { get; init; }
}
