using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Enrollments.AddEnrollmentScheduleSegment;

public sealed class AddEnrollmentScheduleSegmentResponse
{
    public Guid Id { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public List<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public List<WeeklyPatternEntry>? ActiveWeeklyPattern { get; init; }
}
