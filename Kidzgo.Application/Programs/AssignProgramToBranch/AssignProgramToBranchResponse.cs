namespace Kidzgo.Application.Programs.AssignProgramToBranch;

public sealed class AssignProgramToBranchResponse
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public bool IsActive { get; init; }
    public Guid? DefaultMakeupClassId { get; init; }
}
