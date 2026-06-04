using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Syllabuses.GetSyllabuses;

public sealed class GetSyllabusesQuery : IQuery<GetSyllabusesResponse>
{
    public Guid? ProgramId { get; init; }
    public Guid? LevelId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
    public bool IncludeDeleted { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class GetSyllabusesResponse
{
    public Page<SyllabusListItemDto> Syllabuses { get; init; } = null!;
}

public sealed class SyllabusListItemDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public string Code { get; init; } = null!;
    public int Version { get; init; }
    public string Title { get; init; } = null!;
    public bool IsActive { get; init; }
    public int UnitCount { get; init; }
    public int SessionTemplateCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
