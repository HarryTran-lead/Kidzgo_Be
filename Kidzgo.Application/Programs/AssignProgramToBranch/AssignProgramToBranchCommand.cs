using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.AssignProgramToBranch;

public sealed class AssignProgramToBranchCommand : ICommand<AssignProgramToBranchResponse>
{
    public Guid ProgramId { get; init; }
    public Guid BranchId { get; init; }
}
