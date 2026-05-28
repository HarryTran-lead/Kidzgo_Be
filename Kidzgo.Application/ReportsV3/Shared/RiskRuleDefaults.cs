using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Users;

namespace Kidzgo.Application.ReportsV3.Shared;

internal static class RiskRuleDefaults
{
    private static readonly IReadOnlyDictionary<RiskType, int> DefaultScores = new Dictionary<RiskType, int>
    {
        [RiskType.AcademicFail] = 100,
        [RiskType.LowAttendance] = 90,
        [RiskType.AttendanceDiscipline] = 80,
        [RiskType.LearningDelay] = 75,
        [RiskType.WeakCommunication] = 70,
        [RiskType.PackageExpiring] = 60,
        [RiskType.ClassCurriculumDelay] = 55,
        [RiskType.HighReviewRatio] = 50
    };

    private static readonly IReadOnlyDictionary<RiskType, string> DefaultParameters = new Dictionary<RiskType, string>
    {
        [RiskType.LowAttendance] = """
                                   {"attendanceRateBelow":70,"forceHighAttendanceBelow":50}
                                   """,
        [RiskType.AttendanceDiscipline] = """
                                           {"absentWithoutNoticeAtLeast":2}
                                           """,
        [RiskType.LearningDelay] = """
                                   {"delayBufferPercent":10}
                                   """,
        [RiskType.AcademicFail] = "{}",
        [RiskType.WeakCommunication] = """
                                       {"speakingAtMost":2,"confidenceAtMost":2}
                                       """,
        [RiskType.PackageExpiring] = """
                                     {"remainingTicketsAtMost":3}
                                     """,
        [RiskType.ClassCurriculumDelay] = """
                                           {"progressLagTolerancePercent":0}
                                           """,
        [RiskType.HighReviewRatio] = """
                                     {"reviewRatioAtLeast":40}
                                     """
    };

    public static int GetScore(RiskType riskType, IReadOnlyDictionary<RiskType, int> overrideScores)
    {
        if (overrideScores.TryGetValue(riskType, out var configuredScore))
        {
            return configuredScore;
        }

        return GetDefaultScore(riskType);
    }

    public static int GetDefaultScore(RiskType riskType)
    {
        return DefaultScores[riskType];
    }

    public static string GetDefaultParametersJson(RiskType riskType)
    {
        return DefaultParameters[riskType];
    }

    public static RiskSeverity ToSeverity(
        int score,
        decimal attendanceRate,
        bool hasAssessmentFail,
        decimal highAttendanceThreshold)
    {
        if (hasAssessmentFail || attendanceRate < highAttendanceThreshold)
        {
            return RiskSeverity.High;
        }

        return score switch
        {
            >= 90 => RiskSeverity.High,
            >= 60 => RiskSeverity.Medium,
            _ => RiskSeverity.Low
        };
    }

    public static UserRole GetAssignedRole(RiskType riskType)
    {
        return riskType switch
        {
            RiskType.LearningDelay => UserRole.Teacher,
            RiskType.WeakCommunication => UserRole.Teacher,
            RiskType.AttendanceDiscipline => UserRole.Admin,
            _ => UserRole.ManagementStaff
        };
    }

    public static DateTime CalculateDueAt(DateTime nowUtc, RiskSeverity severity)
    {
        return severity switch
        {
            RiskSeverity.High => nowUtc.AddHours(24),
            RiskSeverity.Medium => nowUtc.AddHours(72),
            _ => nowUtc.AddDays(7)
        };
    }

    public static RecommendationPriority ToPriority(RiskSeverity severity)
    {
        return severity switch
        {
            RiskSeverity.High => RecommendationPriority.High,
            RiskSeverity.Medium => RecommendationPriority.Medium,
            _ => RecommendationPriority.Low
        };
    }
}
