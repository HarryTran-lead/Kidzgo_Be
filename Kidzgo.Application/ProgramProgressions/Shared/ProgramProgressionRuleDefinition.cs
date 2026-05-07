using System.Text.Json;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.Shared;

public sealed class ProgramProgressionShieldRange
{
    public ProgramProgressionSkillType Skill { get; init; }
    public decimal MinScore { get; init; }
    public decimal? MaxScore { get; init; }
    public int ShieldCount { get; init; }
}

public sealed class ProgramProgressionClassificationBand
{
    public decimal MinScore { get; init; }
    public decimal? MaxScore { get; init; }
    public string Label { get; init; } = null!;
    public string? CefrLevel { get; init; }
    public string? Description { get; init; }
}

internal static class ProgramProgressionRuleDefinition
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string? SerializeShieldMappings(IReadOnlyCollection<ProgramProgressionShieldRange>? mappings)
        => mappings is null || mappings.Count == 0
            ? null
            : JsonSerializer.Serialize(mappings, JsonOptions);

    public static string? SerializeClassificationBands(IReadOnlyCollection<ProgramProgressionClassificationBand>? bands)
        => bands is null || bands.Count == 0
            ? null
            : JsonSerializer.Serialize(bands, JsonOptions);

    public static string? SerializePracticeTestScoreMappings(IReadOnlyCollection<PracticeTestScoreMapping>? mappings)
        => mappings is null || mappings.Count == 0
            ? null
            : JsonSerializer.Serialize(mappings, JsonOptions);

    public static IReadOnlyList<ProgramProgressionShieldRange> DeserializeShieldMappings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<ProgramProgressionShieldRange>();
        }

        return JsonSerializer.Deserialize<List<ProgramProgressionShieldRange>>(json, JsonOptions) is { } mappings
            ? mappings
            : Array.Empty<ProgramProgressionShieldRange>();
    }

    public static IReadOnlyList<ProgramProgressionClassificationBand> DeserializeClassificationBands(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<ProgramProgressionClassificationBand>();
        }

        return JsonSerializer.Deserialize<List<ProgramProgressionClassificationBand>>(json, JsonOptions) is { } bands
            ? bands
            : Array.Empty<ProgramProgressionClassificationBand>();
    }

    public static IReadOnlyList<PracticeTestScoreMapping> DeserializePracticeTestScoreMappings(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<PracticeTestScoreMapping>();
        }

        return JsonSerializer.Deserialize<List<PracticeTestScoreMapping>>(json, JsonOptions) is { } mappings
            ? mappings
            : Array.Empty<PracticeTestScoreMapping>();
    }

    public static Result Validate(
        ProgramProgressionMethod method,
        int? minimumShieldCount,
        int? minimumSkillShieldCount,
        decimal? minimumOverallScore,
        IReadOnlyCollection<ProgramProgressionShieldRange> shieldMappings,
        IReadOnlyCollection<ProgramProgressionClassificationBand> classificationBands)
    {
        foreach (var mapping in shieldMappings)
        {
            if (mapping.MaxScore.HasValue && mapping.MaxScore.Value < mapping.MinScore)
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.InvalidShieldRange",
                    $"Shield mapping for skill '{mapping.Skill}' has MaxScore smaller than MinScore."));
            }

            if (mapping.ShieldCount is < 0 or > 5)
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.InvalidShieldCount",
                    $"Shield count for skill '{mapping.Skill}' must be between 0 and 5."));
            }
        }

        foreach (var band in classificationBands)
        {
            if (string.IsNullOrWhiteSpace(band.Label))
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.InvalidClassificationBand",
                    "Classification band label is required."));
            }

            if (band.MaxScore.HasValue && band.MaxScore.Value < band.MinScore)
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.InvalidClassificationBand",
                    $"Classification band '{band.Label}' has MaxScore smaller than MinScore."));
            }
        }

        return method switch
        {
            ProgramProgressionMethod.PassFail => Result.Success(),
            ProgramProgressionMethod.Shields => ValidateShieldRule(
                minimumShieldCount,
                minimumSkillShieldCount,
                shieldMappings),
            ProgramProgressionMethod.CambridgeScale => ValidateCambridgeRule(
                minimumOverallScore,
                classificationBands),
            _ => Result.Failure(Error.Validation(
                "ProgramProgression.UnsupportedMethod",
                $"Unsupported progression method '{method}'."))
        };
    }

    public static int? ResolveShield(
        IReadOnlyCollection<ProgramProgressionShieldRange> shieldMappings,
        ProgramProgressionSkillType skill,
        decimal score)
    {
        return shieldMappings
            .Where(mapping => mapping.Skill == skill)
            .OrderBy(mapping => mapping.MinScore)
            .FirstOrDefault(mapping =>
                score >= mapping.MinScore &&
                (!mapping.MaxScore.HasValue || score <= mapping.MaxScore.Value))
            ?.ShieldCount;
    }

    public static ProgramProgressionClassificationBand? ResolveClassificationBand(
        IReadOnlyCollection<ProgramProgressionClassificationBand> bands,
        decimal score)
    {
        return bands
            .OrderBy(band => band.MinScore)
            .FirstOrDefault(band =>
                score >= band.MinScore &&
                (!band.MaxScore.HasValue || score <= band.MaxScore.Value));
    }

    public static decimal? ConvertPracticeScoreToCambridgeScale(
        int practiceScore,
        ProgramProgressionSkillType skillType,
        IReadOnlyCollection<PracticeTestScoreMapping> mappings)
    {
        var mapping = mappings
            .Where(m => m.SkillType == skillType)
            .OrderBy(m => m.MinPracticeScore)
            .FirstOrDefault(m =>
                practiceScore >= m.MinPracticeScore &&
                practiceScore <= m.MaxPracticeScore);

        return mapping?.CambridgeScaleScore;
    }

    private static Result ValidateShieldRule(
        int? minimumShieldCount,
        int? minimumSkillShieldCount,
        IReadOnlyCollection<ProgramProgressionShieldRange> shieldMappings)
    {
        if (!minimumShieldCount.HasValue || minimumShieldCount.Value is < 0 or > 15)
        {
            return Result.Failure(Error.Validation(
                "ProgramProgression.MinimumShieldCountRequired",
                "MinimumShieldCount is required for shield-based progression and must be between 0 and 15."));
        }

        if (minimumSkillShieldCount.HasValue && minimumSkillShieldCount.Value is < 0 or > 5)
        {
            return Result.Failure(Error.Validation(
                "ProgramProgression.MinimumSkillShieldCountInvalid",
                "MinimumSkillShieldCount must be between 0 and 5."));
        }

        var requiredSkills = new[]
        {
            ProgramProgressionSkillType.Listening,
            ProgramProgressionSkillType.ReadingWriting
        };

        foreach (var skill in requiredSkills)
        {
            var skillMappings = shieldMappings
                .Where(mapping => mapping.Skill == skill)
                .OrderBy(mapping => mapping.MinScore)
                .ToList();

            if (skillMappings.Count == 0)
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.ShieldMappingsMissing",
                    $"Shield mappings for skill '{skill}' are required."));
            }

            for (int i = 1; i < skillMappings.Count; i++)
            {
                var previous = skillMappings[i - 1];
                var current = skillMappings[i];

                if (!previous.MaxScore.HasValue)
                {
                    return Result.Failure(Error.Validation(
                        "ProgramProgression.ShieldMappingsOverlap",
                        $"Skill '{skill}' contains an open-ended range before a later range."));
                }

                if (current.MinScore <= previous.MaxScore.Value)
                {
                    return Result.Failure(Error.Validation(
                        "ProgramProgression.ShieldMappingsOverlap",
                        $"Shield mappings for skill '{skill}' contain overlapping ranges."));
                }
            }
        }

        return Result.Success();
    }

    private static Result ValidateCambridgeRule(
        decimal? minimumOverallScore,
        IReadOnlyCollection<ProgramProgressionClassificationBand> classificationBands)
    {
        if (!minimumOverallScore.HasValue || minimumOverallScore.Value <= 0)
        {
            return Result.Failure(Error.Validation(
                "ProgramProgression.MinimumOverallScoreRequired",
                "MinimumOverallScore is required for Cambridge-scale progression."));
        }

        var orderedBands = classificationBands
            .OrderBy(band => band.MinScore)
            .ToList();

        for (int i = 1; i < orderedBands.Count; i++)
        {
            var previous = orderedBands[i - 1];
            var current = orderedBands[i];

            if (!previous.MaxScore.HasValue)
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.ClassificationBandsOverlap",
                    "Classification bands contain an open-ended range before a later range."));
            }

            if (current.MinScore <= previous.MaxScore.Value)
            {
                return Result.Failure(Error.Validation(
                    "ProgramProgression.ClassificationBandsOverlap",
                    "Classification bands contain overlapping ranges."));
            }
        }

        return Result.Success();
    }
}
