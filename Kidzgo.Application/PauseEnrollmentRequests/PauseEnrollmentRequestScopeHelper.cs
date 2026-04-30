namespace Kidzgo.Application.PauseEnrollmentRequests;

public static class PauseEnrollmentRequestScopeHelper
{
    public const string AllEligible = "AllEligible";
    public const string SingleClass = "SingleClass";

    public static bool TryNormalize(string? scope, Guid? classId, out string normalizedScope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            normalizedScope = classId.HasValue ? SingleClass : AllEligible;
            return true;
        }

        if (string.Equals(scope, AllEligible, StringComparison.OrdinalIgnoreCase))
        {
            normalizedScope = AllEligible;
            return true;
        }

        if (string.Equals(scope, SingleClass, StringComparison.OrdinalIgnoreCase))
        {
            normalizedScope = SingleClass;
            return true;
        }

        normalizedScope = string.Empty;
        return false;
    }

    public static string ResolveFromClassId(Guid? classId)
    {
        return classId.HasValue ? SingleClass : AllEligible;
    }
}
