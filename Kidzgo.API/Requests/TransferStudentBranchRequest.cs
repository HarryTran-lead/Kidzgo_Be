namespace Kidzgo.API.Requests;

public sealed class TransferStudentBranchRequest
{
    public Guid FromBranchId { get; init; }
    public Guid ToBranchId { get; init; }
    public DateOnly EffectiveDate { get; init; }
    public string? Reason { get; init; }
    public bool KeepCurrentClass { get; init; }
    public bool AllowCrossBranchEnrollment { get; init; }
}
