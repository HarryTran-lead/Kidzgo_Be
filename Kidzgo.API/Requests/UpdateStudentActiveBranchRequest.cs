namespace Kidzgo.API.Requests;

public sealed class UpdateStudentActiveBranchRequest
{
    public Guid BranchId { get; init; }
    public bool AllowCrossBranchEnrollment { get; init; }
}
