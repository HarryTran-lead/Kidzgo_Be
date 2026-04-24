using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Enrollments.UpdateEnrollment;

public sealed class UpdateEnrollmentCommand : ICommand<UpdateEnrollmentResponse>
{
    public Guid Id { get; init; }
    public DateOnly? EnrollDate { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? Track { get; init; }
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public bool ClearWeeklyPattern { get; init; }
}

