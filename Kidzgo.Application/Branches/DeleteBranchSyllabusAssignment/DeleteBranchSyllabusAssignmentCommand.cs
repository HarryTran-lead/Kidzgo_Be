using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.DeleteBranchSyllabusAssignment;

public sealed class DeleteBranchSyllabusAssignmentCommand : ICommand<DeleteBranchSyllabusAssignmentResponse>
{
    public Guid BranchId { get; init; }
    public Guid AssignmentId { get; init; }
}

public sealed class DeleteBranchSyllabusAssignmentResponse
{
    public Guid BranchId { get; init; }
    public Guid AssignmentId { get; init; }
    public Guid SyllabusId { get; init; }
}
