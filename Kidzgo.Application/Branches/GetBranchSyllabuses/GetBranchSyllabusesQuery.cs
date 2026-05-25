using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.GetBranchSyllabuses;

public sealed class GetBranchSyllabusesQuery : IQuery<GetBranchSyllabusesResponse>
{
    public Guid BranchId { get; init; }
}

public sealed class GetBranchSyllabusesResponse
{
    public IReadOnlyList<BranchSyllabusDto> Syllabuses { get; init; } = [];
}

public sealed class BranchSyllabusDto
{
    public Guid CurriculumAssignmentId { get; init; }
    public Guid SyllabusId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public string Version { get; init; } = null!;
    public string Title { get; init; } = null!;
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}
