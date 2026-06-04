namespace Kidzgo.API.Requests;

/// <summary>
/// Payload to update syllabus metadata.
/// </summary>
public sealed class UpdateSyllabusRequest
{
    public string Code { get; init; } = null!;
    public int Version { get; init; }
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public string? PacingSchemeJson { get; init; }
    public string? Overview { get; init; }
    public string? OverallObjectives { get; init; }
    public string? SpecificObjectives { get; init; }
    public string? EthicsAndAttitudes { get; init; }
    public string? BookOverview { get; init; }
    public int? TotalPeriods { get; init; }
    public int? MinutesPerPeriod { get; init; }
    public int? TotalLessons { get; init; }
    public string? SourceFileName { get; init; }
    public string? AttachmentUrl { get; init; }
    public string? RawContentJson { get; init; }
    public bool IsActive { get; init; } = true;
}
