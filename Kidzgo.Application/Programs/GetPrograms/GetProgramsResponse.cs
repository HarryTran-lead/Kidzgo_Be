using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Programs.GetPrograms;

public sealed class GetProgramsResponse
{
    public Page<ProgramDto> Programs { get; init; } = null!;
}

public sealed class ProgramDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Code { get; init; }
    public bool IsMakeup { get; init; }
    public bool IsSupplementary { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int AssignedBranchCount { get; init; }
    public decimal BaseFee { get; init; }
    public decimal Fee { get; init; }
    public int ClassCount { get; init; }
    public int StudentCount { get; init; }
    public string Status => IsActive ? "Active" : "Inactive";
}

