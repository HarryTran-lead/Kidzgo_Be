namespace Kidzgo.Application.PauseEnrollmentRequests;

public static class PauseEnrollmentRequestScopeHelper
{
    public const string AllEligible = "AllEligible";
    public const string SingleClass = "SingleClass";

    public static string ResolveFromClassId(Guid? classId)
    {
        return classId.HasValue ? SingleClass : AllEligible;
    }
}
