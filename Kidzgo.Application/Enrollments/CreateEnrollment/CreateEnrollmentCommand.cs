using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Enrollments.CreateEnrollment;

public sealed class CreateEnrollmentCommand : ICommand<CreateEnrollmentResponse>
{
    public Guid ClassId { get; init; }
    public Guid StudentProfileId { get; init; }
    public DateOnly EnrollDate { get; init; }
    public Guid? RegistrationId { get; init; }
    public Guid? TuitionPlanId { get; init; }
    public string? Track { get; init; }
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
    public bool AllowCrossBranchEnrollment { get; init; }
}

