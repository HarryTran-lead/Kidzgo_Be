using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Application.Enrollments.GetEnrollments;

public sealed class GetEnrollmentsResponse
{
    public Page<EnrollmentDto> Enrollments { get; init; } = null!;
}

public sealed class EnrollmentDto
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public Guid? RegistrationId { get; init; }
    public DateOnly EnrollDate { get; init; }
    public string Status { get; init; } = null!;
    public Guid? TuitionPlanId { get; init; }
    public string? TuitionPlanName { get; init; }
}

