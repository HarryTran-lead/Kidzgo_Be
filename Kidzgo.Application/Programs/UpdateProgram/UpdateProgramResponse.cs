namespace Kidzgo.Application.Programs.UpdateProgram;

public sealed class UpdateProgramResponse
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
}

