namespace Kidzgo.Application.Programs.GetProgramById;

public sealed class GetProgramByIdResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Code { get; init; }
    public bool IsMakeup { get; init; }
    public bool IsSupplementary { get; init; }
    public decimal DefaultTuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int TotalSessions { get; init; }
    public IReadOnlyList<ProgramBranchAssignmentDto> BranchAssignments { get; init; } =
        Array.Empty<ProgramBranchAssignmentDto>();
    public decimal BaseFee => DefaultTuitionAmount;
    public decimal Fee => DefaultTuitionAmount;
    public int ClassCount { get; init; }
    public int StudentCount { get; init; }
    public string Status => IsActive ? "Active" : "Inactive";
}

public sealed class ProgramBranchAssignmentDto
{
    public Guid BranchId { get; init; }
    public string BranchName { get; init; } = null!;
    public bool IsActive { get; init; }
    public Guid? DefaultMakeupClassId { get; init; }
}

