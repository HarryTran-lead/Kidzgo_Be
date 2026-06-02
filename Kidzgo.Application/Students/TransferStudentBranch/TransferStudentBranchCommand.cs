using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;

namespace Kidzgo.Application.Students.TransferStudentBranch;

public sealed class TransferStudentBranchCommand : ICommand<StudentBranchStateDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid FromBranchId { get; init; }
    public Guid ToBranchId { get; init; }
    public DateOnly EffectiveDate { get; init; }
    public string? Reason { get; init; }
    public bool KeepCurrentClass { get; init; }
    public bool AllowCrossBranchEnrollment { get; init; }
}
