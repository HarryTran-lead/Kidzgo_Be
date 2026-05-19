namespace Kidzgo.API.Requests;

/// <summary>
/// Payload to create a syllabus or curriculum version manually.
/// </summary>
public sealed class CreateSyllabusRequest
{
    /// <summary>
    /// Program that owns this syllabus.
    /// </summary>
    public Guid ProgramId { get; init; }

    /// <summary>
    /// Level inside the program.
    /// </summary>
    public Guid LevelId { get; init; }

    /// <summary>
    /// Business curriculum code, for example GET_READY_STARTER.
    /// </summary>
    public string Code { get; init; } = null!;

    /// <summary>
    /// Version label, for example v1.
    /// </summary>
    public string Version { get; init; } = null!;

    /// <summary>
    /// Display title.
    /// </summary>
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

    /// <summary>
    /// Optional raw JSON snapshot imported from external source.
    /// </summary>
    public string? RawContentJson { get; init; }
    public bool IsActive { get; init; } = true;
}
