using System.Text.Json;
using System.Text.Json.Serialization;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.Shared;

internal sealed class ReportTemplateRuntime
{
    private const string DefaultRecommendation =
        "Follow up with student and parent for corrective action.";

    private readonly ReportTemplateContentSchema _content;

    private ReportTemplateRuntime(ReportTemplateContentSchema content)
    {
        _content = content;
    }

    public static ReportTemplateRuntime Create(ReportTemplateType templateType, string? contentSchema)
    {
        var defaults = CreateDefaultSchema(templateType);

        if (string.IsNullOrWhiteSpace(contentSchema))
        {
            return new ReportTemplateRuntime(defaults);
        }

        try
        {
            var custom = JsonSerializer.Deserialize<ReportTemplateContentSchema>(
                contentSchema,
                BuildJsonOptions());

            if (custom is null)
            {
                return new ReportTemplateRuntime(defaults);
            }

            return new ReportTemplateRuntime(Merge(defaults, custom));
        }
        catch
        {
            return new ReportTemplateRuntime(defaults);
        }
    }

    public static string CreateDefaultSchemaJson(ReportTemplateType templateType)
    {
        return JsonSerializer.Serialize(
            CreateDefaultSchema(templateType),
            BuildJsonOptions());
    }

    public string GetRiskReason(RiskType riskType, IReadOnlyDictionary<string, string>? tokens = null)
    {
        var key = riskType.ToString();
        if (_content.RiskReasons.TryGetValue(key, out var template))
        {
            return RenderTemplate(template, tokens);
        }

        return $"Detected risk: {key}.";
    }

    public string GetRecommendation(RiskType riskType)
    {
        var key = riskType.ToString();
        if (_content.Recommendations.TryGetValue(key, out var template))
        {
            return template;
        }

        if (_content.Recommendations.TryGetValue("default", out var fallback))
        {
            return fallback;
        }

        return DefaultRecommendation;
    }

    public string GetParentMessage(
        RiskType? primaryRiskType,
        IReadOnlyDictionary<string, string>? tokens = null)
    {
        if (primaryRiskType.HasValue &&
            _content.ParentMessages.TryGetValue(primaryRiskType.Value.ToString(), out var riskMessage))
        {
            return RenderTemplate(riskMessage, tokens);
        }

        if (_content.ParentMessages.TryGetValue("default", out var defaultMessage))
        {
            return RenderTemplate(defaultMessage, tokens);
        }

        return "Student is maintaining stable learning momentum and can continue with the next module goals.";
    }

    public string GetStrength(string key)
    {
        if (_content.Strengths.TryGetValue(key, out var value))
        {
            return value;
        }

        return key;
    }

    public string GetWeakness(string key)
    {
        if (_content.Weaknesses.TryGetValue(key, out var value))
        {
            return value;
        }

        return key;
    }

    public string GetInternalNote(string key, string fallback)
    {
        if (_content.InternalNotes.TryGetValue(key, out var value))
        {
            return value;
        }

        return fallback;
    }

    private static ReportTemplateContentSchema Merge(
        ReportTemplateContentSchema defaults,
        ReportTemplateContentSchema custom)
    {
        return new ReportTemplateContentSchema
        {
            ParentMessages = MergeDictionary(defaults.ParentMessages, custom.ParentMessages ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            RiskReasons = MergeDictionary(defaults.RiskReasons, custom.RiskReasons ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            Recommendations = MergeDictionary(defaults.Recommendations, custom.Recommendations ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            Strengths = MergeDictionary(defaults.Strengths, custom.Strengths ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            Weaknesses = MergeDictionary(defaults.Weaknesses, custom.Weaknesses ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
            InternalNotes = MergeDictionary(defaults.InternalNotes, custom.InternalNotes ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase))
        };
    }

    private static Dictionary<string, string> MergeDictionary(
        IReadOnlyDictionary<string, string> defaults,
        IReadOnlyDictionary<string, string> overrides)
    {
        var merged = new Dictionary<string, string>(defaults, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in overrides)
        {
            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            merged[key] = value.Trim();
        }

        return merged;
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string>? tokens)
    {
        if (string.IsNullOrWhiteSpace(template) || tokens is null || tokens.Count == 0)
        {
            return template;
        }

        var rendered = template;
        foreach (var (key, value) in tokens)
        {
            rendered = rendered.Replace(
                "{" + key + "}",
                value,
                StringComparison.OrdinalIgnoreCase);
        }

        return rendered;
    }

    private static JsonSerializerOptions BuildJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };
    }

    private static ReportTemplateContentSchema CreateDefaultSchema(ReportTemplateType templateType)
    {
        var defaults = new ReportTemplateContentSchema
        {
            ParentMessages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [RiskType.AcademicFail.ToString()] =
                    "Student needs more time to strengthen speaking confidence before the next learning milestone.",
                [RiskType.LowAttendance.ToString()] =
                    "Please support attendance consistency so learning progress can improve steadily.",
                [RiskType.PackageExpiring.ToString()] =
                    "Remaining sessions are low ({remainingTickets}). Please review package renewal options.",
                ["default"] =
                    "Student is maintaining stable learning momentum and can continue with the next module goals."
            },
            RiskReasons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [RiskType.LowAttendance.ToString()] =
                    "Attendance rate ({attendanceRate}%) is below {attendanceRateBelow}%.",
                [RiskType.AttendanceDiscipline.ToString()] =
                    "Absence without notice occurred {absentWithoutNotice} times (threshold: {absentWithoutNoticeAtLeast}).",
                [RiskType.LearningDelay.ToString()] =
                    "Completion ({completionPercent}%) is below expected ({expectedCompletionPercent}%) with {delayBufferPercent}% buffer.",
                [RiskType.AcademicFail.ToString()] =
                    "Latest assessment result is FAIL.",
                [RiskType.WeakCommunication.ToString()] =
                    "Speaking ({speaking}) or confidence ({confidence}) is at or below configured threshold.",
                [RiskType.PackageExpiring.ToString()] =
                    "Remaining learning tickets ({remainingTickets}) are at or below {remainingTicketsAtMost}.",
                [RiskType.ClassCurriculumDelay.ToString()] =
                    "Class progress ({classActualProgressPercent}%) is behind expected progress ({expectedCompletionPercent}%).",
                [RiskType.HighReviewRatio.ToString()] =
                    "Review section ratio ({classReviewRatioPercent}%) is at least {reviewRatioAtLeast}%."
            },
            Recommendations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [RiskType.LowAttendance.ToString()] =
                    "Contact parent to verify attendance schedule and absence reasons.",
                [RiskType.AttendanceDiscipline.ToString()] =
                    "Confirm attendance policy with parent and student.",
                [RiskType.LearningDelay.ToString()] =
                    "Add focused review support for delayed lessons.",
                [RiskType.AcademicFail.ToString()] =
                    "Create remedial recommendation before reassessment.",
                [RiskType.WeakCommunication.ToString()] =
                    "Increase speaking-focused activities in class.",
                [RiskType.PackageExpiring.ToString()] =
                    "Advise parent on package renewal options.",
                [RiskType.ClassCurriculumDelay.ToString()] =
                    "Review class pacing and teaching plan.",
                [RiskType.HighReviewRatio.ToString()] =
                    "Review teaching plan to balance review and new content.",
                ["default"] = DefaultRecommendation
            },
            Strengths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["good_attendance"] = "Good attendance consistency.",
                ["strong_progress"] = "Strong learning progress in current module.",
                ["confident_speaking"] = "Confident speaking participation."
            },
            Weaknesses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["learning_delay"] = "Learning progress is behind expected module pacing.",
                ["assessment_fail"] = "Latest assessment result requires remediation.",
                ["weak_communication"] = "Communication confidence needs additional support."
            },
            InternalNotes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["snapshot_immutable"] = "Snapshot is immutable and generated from read-only sources.",
                ["insight_generated"] = "Rule-based insight generation executed successfully."
            }
        };

        if (templateType == ReportTemplateType.Parent)
        {
            defaults.InternalNotes["snapshot_immutable"] = "Parent report snapshot was generated from read-only sources.";
        }

        return defaults;
    }

    private sealed class ReportTemplateContentSchema
    {
        [JsonPropertyName("parent_messages")]
        public Dictionary<string, string> ParentMessages { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("risk_reasons")]
        public Dictionary<string, string> RiskReasons { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("recommendations")]
        public Dictionary<string, string> Recommendations { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("strengths")]
        public Dictionary<string, string> Strengths { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("weaknesses")]
        public Dictionary<string, string> Weaknesses { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);

        [JsonPropertyName("internal_notes")]
        public Dictionary<string, string> InternalNotes { get; init; } =
            new(StringComparer.OrdinalIgnoreCase);
    }
}
