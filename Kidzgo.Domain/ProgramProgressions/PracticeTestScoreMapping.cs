namespace Kidzgo.Domain.ProgramProgressions;

/// <summary>
/// Mapping between Practice Test Score (raw score) and Cambridge English Scale Score
/// Used for Cambridge Scale progression method
/// </summary>
public sealed class PracticeTestScoreMapping
{
    /// <summary>
    /// Minimum practice test score in range (inclusive)
    /// Example: 17 for A2 range 17-23
    /// </summary>
    public int MinPracticeScore { get; set; }

    /// <summary>
    /// Maximum practice test score in range (inclusive)
    /// Example: 23 for A2 range 17-23
    /// </summary>
    public int MaxPracticeScore { get; set; }

    /// <summary>
    /// Corresponding Cambridge English Scale Score
    /// Example: 120 for A2 level
    /// </summary>
    public int CambridgeScaleScore { get; set; }

    /// <summary>
    /// CEFR level corresponding to this score range
    /// Example: "A2", "B1", etc.
    /// </summary>
    public string CefrLevel { get; set; } = string.Empty;

    /// <summary>
    /// Which skill this mapping applies to
    /// </summary>
    public ProgramProgressionSkillType SkillType { get; set; }
}
