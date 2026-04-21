namespace Kidzgo.API.Requests;

public sealed class BackfillStudentSessionAssignmentsRequest
{
    public Guid? EnrollmentId { get; init; }
    public Guid? ClassId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public int? BatchSize { get; init; }
}
