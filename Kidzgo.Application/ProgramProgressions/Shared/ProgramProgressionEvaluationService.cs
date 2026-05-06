using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.Shared;

public sealed record ProgramProgressionAssessmentInput(
    bool? PassedInClass,
    decimal? ListeningScore,
    decimal? SpeakingScore,
    decimal? ReadingWritingScore,
    decimal? ReadingScore,
    decimal? WritingScore);

public sealed record ProgramProgressionComputedResult(
    bool IsEligible,
    int? ListeningShieldCount,
    int? SpeakingShieldCount,
    int? ReadingWritingShieldCount,
    int? TotalShieldCount,
    decimal? OverallScore,
    string? ResultBand,
    string? ResultLevel);

public sealed class ProgramProgressionEvaluationService
{
    public Result<ProgramProgressionComputedResult> Evaluate(
        ProgramProgressionRule rule,
        ProgramProgressionAssessmentInput input)
    {
        return rule.Method switch
        {
            ProgramProgressionMethod.PassFail => EvaluatePassFail(input),
            ProgramProgressionMethod.Shields => EvaluateShieldRule(rule, input),
            ProgramProgressionMethod.CambridgeScale => EvaluateCambridgeRule(rule, input),
            _ => Result.Failure<ProgramProgressionComputedResult>(Error.Validation(
                "ProgramProgression.UnsupportedMethod",
                $"Unsupported progression method '{rule.Method}'."))
        };
    }

    private static Result<ProgramProgressionComputedResult> EvaluatePassFail(
        ProgramProgressionAssessmentInput input)
    {
        if (!input.PassedInClass.HasValue)
        {
            return Result.Failure<ProgramProgressionComputedResult>(Error.Validation(
                "ProgramProgression.PassFailRequired",
                "PassedInClass is required for pass/fail progression."));
        }

        return Result.Success(new ProgramProgressionComputedResult(
            input.PassedInClass.Value,
            null,
            null,
            null,
            null,
            null,
            input.PassedInClass.Value ? "Pass" : "Fail",
            null));
    }

    private static Result<ProgramProgressionComputedResult> EvaluateShieldRule(
        ProgramProgressionRule rule,
        ProgramProgressionAssessmentInput input)
    {
        if (!input.ListeningScore.HasValue ||
            !input.SpeakingScore.HasValue ||
            !input.ReadingWritingScore.HasValue)
        {
            return Result.Failure<ProgramProgressionComputedResult>(Error.Validation(
                "ProgramProgression.ShieldScoresRequired",
                "ListeningScore, SpeakingScore and ReadingWritingScore are required for shield-based progression."));
        }

        if (input.SpeakingScore.Value is < 0 or > 5)
        {
            return Result.Failure<ProgramProgressionComputedResult>(Error.Validation(
                "ProgramProgression.SpeakingScoreInvalid",
                "SpeakingScore must be between 0 and 5 for shield-based progression."));
        }

        var shieldMappings = ProgramProgressionRuleDefinition.DeserializeShieldMappings(rule.ShieldMappingJson);
        var listeningShieldCount = ProgramProgressionRuleDefinition.ResolveShield(
            shieldMappings,
            ProgramProgressionSkillType.Listening,
            input.ListeningScore.Value);
        var readingWritingShieldCount = ProgramProgressionRuleDefinition.ResolveShield(
            shieldMappings,
            ProgramProgressionSkillType.ReadingWriting,
            input.ReadingWritingScore.Value);

        if (!listeningShieldCount.HasValue || !readingWritingShieldCount.HasValue)
        {
            return Result.Failure<ProgramProgressionComputedResult>(Error.Validation(
                "ProgramProgression.ShieldRangeNotMatched",
                "At least one entered score does not match the configured shield ranges."));
        }

        var speakingShieldCount = (int)Math.Round(input.SpeakingScore.Value, MidpointRounding.AwayFromZero);
        var totalShieldCount = listeningShieldCount.Value + speakingShieldCount + readingWritingShieldCount.Value;
        var hasMinimumPerSkill = !rule.MinimumSkillShieldCount.HasValue ||
            (listeningShieldCount.Value >= rule.MinimumSkillShieldCount.Value &&
             speakingShieldCount >= rule.MinimumSkillShieldCount.Value &&
             readingWritingShieldCount.Value >= rule.MinimumSkillShieldCount.Value);
        var isEligible = rule.MinimumShieldCount.HasValue &&
            totalShieldCount >= rule.MinimumShieldCount.Value &&
            hasMinimumPerSkill;

        return Result.Success(new ProgramProgressionComputedResult(
            isEligible,
            listeningShieldCount.Value,
            speakingShieldCount,
            readingWritingShieldCount.Value,
            totalShieldCount,
            null,
            $"{totalShieldCount} shields",
            null));
    }

    private static Result<ProgramProgressionComputedResult> EvaluateCambridgeRule(
        ProgramProgressionRule rule,
        ProgramProgressionAssessmentInput input)
    {
        if (!input.ListeningScore.HasValue ||
            !input.SpeakingScore.HasValue ||
            !input.ReadingScore.HasValue ||
            !input.WritingScore.HasValue)
        {
            return Result.Failure<ProgramProgressionComputedResult>(Error.Validation(
                "ProgramProgression.CambridgeScoresRequired",
                "ListeningScore, SpeakingScore, ReadingScore and WritingScore are required for Cambridge-scale progression."));
        }

        var overallScore = decimal.Round(
            (input.ListeningScore.Value +
             input.SpeakingScore.Value +
             input.ReadingScore.Value +
             input.WritingScore.Value) / 4m,
            2,
            MidpointRounding.AwayFromZero);

        var classificationBands = ProgramProgressionRuleDefinition.DeserializeClassificationBands(rule.ClassificationBandsJson);
        var matchedBand = ProgramProgressionRuleDefinition.ResolveClassificationBand(
            classificationBands,
            overallScore);
        var isEligible = rule.MinimumOverallScore.HasValue &&
            overallScore >= rule.MinimumOverallScore.Value;

        return Result.Success(new ProgramProgressionComputedResult(
            isEligible,
            null,
            null,
            null,
            null,
            overallScore,
            matchedBand?.Label,
            matchedBand?.CefrLevel));
    }
}
