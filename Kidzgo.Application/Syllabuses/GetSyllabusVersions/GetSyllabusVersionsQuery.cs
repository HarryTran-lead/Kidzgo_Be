using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.GetSyllabusVersions;

public sealed class GetSyllabusVersionsQuery : IQuery<GetSyllabusVersionsResponse>
{
    public Guid? BranchId { get; init; }
    public Guid? ProgramId { get; init; }
    public Guid? LevelId { get; init; }
    public bool ActiveOnly { get; init; } = true;
}

public sealed class GetSyllabusVersionsResponse
{
    public IReadOnlyList<SyllabusVersionDto> Versions { get; init; } = [];
}

public sealed class SyllabusVersionDto
{
    public Guid SyllabusId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public int Version { get; init; }
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public bool IsActive { get; init; }
}
