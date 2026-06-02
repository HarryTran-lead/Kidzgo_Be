using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.RemoveProgramFromBranch;

public sealed class RemoveProgramFromBranchCommand : ICommand<RemoveProgramFromBranchResponse>
{
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
}

public sealed class RemoveProgramFromBranchResponse
{
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
}
