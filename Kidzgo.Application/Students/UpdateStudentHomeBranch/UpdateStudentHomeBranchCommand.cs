using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Students.Shared;

namespace Kidzgo.Application.Students.UpdateStudentHomeBranch;

public sealed class UpdateStudentHomeBranchCommand : ICommand<StudentBranchStateDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
}
