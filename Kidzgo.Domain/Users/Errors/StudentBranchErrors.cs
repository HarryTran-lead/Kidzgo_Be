using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Users.Errors;

public static class StudentBranchErrors
{
    public static Error StudentNotFound(Guid studentProfileId) => Error.NotFound(
        "StudentBranch.StudentNotFound",
        $"Student profile '{studentProfileId}' was not found or is not active.");

    public static Error BranchNotFound(Guid branchId) => Error.NotFound(
        "StudentBranch.BranchNotFound",
        $"Branch '{branchId}' was not found or is inactive.");

    public static Error CrossBranchEnrollmentNotAllowed(Guid studentProfileId, Guid activeBranchId, Guid targetBranchId) => Error.Validation(
        "StudentBranch.CrossBranchEnrollmentNotAllowed",
        $"Student '{studentProfileId}' is active in branch '{activeBranchId}' and cannot study in branch '{targetBranchId}' without cross-branch permission.");

    public static Error TransferCurrentBranchMismatch(Guid expectedBranchId, Guid actualBranchId) => Error.Validation(
        "StudentBranch.TransferCurrentBranchMismatch",
        $"Student active branch mismatch. Expected '{expectedBranchId}', actual '{actualBranchId}'.");

    public static Error ActiveEnrollmentsRequireResolution(Guid targetBranchId) => Error.Conflict(
        "StudentBranch.ActiveEnrollmentsRequireResolution",
        $"Student still has active branch enrollments outside target branch '{targetBranchId}'. Transfer or close those enrollments first.");

    public static readonly Error KeepCurrentClassRequiresCrossBranchPermission = Error.Validation(
        "StudentBranch.KeepCurrentClassRequiresCrossBranchPermission",
        "Keeping the current class during branch transfer requires cross-branch enrollment permission.");
}
