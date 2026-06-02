using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.GetSyllabusVersionHistory;

public sealed class GetSyllabusVersionHistoryQuery : IQuery<GetSyllabusVersionHistoryResponse>
{
    public Guid SyllabusId { get; init; }
}

public sealed class GetSyllabusVersionHistoryResponse
{
    public Guid SyllabusId { get; init; }
    public string Code { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public IReadOnlyList<SyllabusVersionHistoryItemDto> Versions { get; init; } = Array.Empty<SyllabusVersionHistoryItemDto>();
}

public sealed class SyllabusVersionHistoryItemDto
{
    public Guid SyllabusId { get; init; }
    public string Version { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public string DocumentStatus { get; init; } = null!;
    public bool IsActive { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
