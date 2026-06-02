using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Programs.GetBranchPrograms;

public sealed class GetBranchProgramsQuery : IQuery<GetBranchProgramsResponse>
{
    public Guid BranchId { get; init; }
}

public sealed class GetBranchProgramsResponse
{
    public IReadOnlyList<BranchProgramDto> Programs { get; init; } = Array.Empty<BranchProgramDto>();
}

public sealed class BranchProgramDto
{
    public Guid BranchProgramId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string ProgramCode { get; init; } = null!;
    public bool IsActive { get; init; }
    public Guid? DefaultMakeupClassId { get; init; }
}
