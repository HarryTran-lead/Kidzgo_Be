namespace Kidzgo.Application.Branches.UpsertBranchSyllabus;

public sealed class UpsertBranchSyllabusResponse
{
    public Guid CurriculumAssignmentId { get; init; }
    public Guid BranchId { get; init; }
    public Guid SyllabusId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public int Version { get; init; }
    public string Title { get; init; } = null!;
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}
