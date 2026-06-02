using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.API.Requests;

public sealed class CreateEnrollmentRequest
{
    public Guid ClassId { get; set; }
    public Guid StudentProfileId { get; set; }
    public DateOnly EnrollDate { get; set; }
    public Guid? TuitionPlanId { get; set; }
    public string? Track { get; set; }
    public List<WeeklyPatternEntry>? WeeklyPattern { get; set; }
    public bool AllowCrossBranchEnrollment { get; set; }
}

