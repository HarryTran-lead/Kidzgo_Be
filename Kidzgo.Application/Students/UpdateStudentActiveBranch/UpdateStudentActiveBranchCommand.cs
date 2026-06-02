using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;

namespace Kidzgo.Application.Students.UpdateStudentActiveBranch;

public sealed class UpdateStudentActiveBranchCommand : ICommand<StudentBranchStateDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
    public bool AllowCrossBranchEnrollment { get; init; }
}
