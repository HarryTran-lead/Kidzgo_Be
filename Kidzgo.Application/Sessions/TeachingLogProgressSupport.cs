using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions;

internal static class TeachingLogProgressSupport
{
    public static bool TryMapProgressStatus(
        string rawStatus,
        out SessionCoverageStatus coverageStatus,
        out bool consumeLesson,
        out decimal coveragePercent)
    {
        switch (rawStatus.Trim().ToLowerInvariant())
        {
            case "completed":
                coverageStatus = SessionCoverageStatus.Completed;
                consumeLesson = true;
                coveragePercent = 100;
                return true;
            case "partial":
                coverageStatus = SessionCoverageStatus.Partial;
                consumeLesson = false;
                coveragePercent = 50;
                return true;
            case "not_started":
                coverageStatus = SessionCoverageStatus.Planned;
                consumeLesson = false;
                coveragePercent = 0;
                return true;
            case "skipped":
                coverageStatus = SessionCoverageStatus.Skipped;
                consumeLesson = true;
                coveragePercent = 100;
                return true;
            default:
                coverageStatus = default;
                consumeLesson = false;
                coveragePercent = 0;
                return false;
        }
    }

    public static bool ShouldConsumeLesson(SessionCoverageStatus status)
    {
        return status is SessionCoverageStatus.Completed or SessionCoverageStatus.Skipped;
    }
}
