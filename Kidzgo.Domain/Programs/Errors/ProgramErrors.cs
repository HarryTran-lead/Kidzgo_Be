using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Programs.Errors;

public static class ProgramErrors
{
    public static Error NotFound(Guid? programId) => Error.NotFound(
        "Program.NotFound",
        $"Program with Id = '{programId}' was not found");

    public static readonly Error BranchNotFound = Error.NotFound(
        "Program.BranchNotFound",
        "Branch not found or inactive");

    public static Error AlreadyAssignedToBranch(Guid programId, Guid branchId) => Error.Conflict(
        "Program.AlreadyAssignedToBranch",
        $"Program '{programId}' is already assigned to branch '{branchId}'");

    public static readonly Error HasActiveClasses = Error.Conflict(
        "Program.HasActiveClasses",
        "Cannot delete program with active or planned classes");

    public static readonly Error HasActiveEnrollments = Error.Conflict(
        "Program.HasActiveEnrollments",
        "Cannot delete program with active or paused enrollments");

    public static readonly Error NotMakeupProgram = Error.Validation(
        "Program.NotMakeupProgram",
        "Program is not a makeup program");

    public static Error DefaultMakeupClassNotFound(Guid? classId) => Error.NotFound(
        "Program.DefaultMakeupClassNotFound",
        $"Default makeup class with Id = '{classId}' was not found");

    public static Error DefaultMakeupClassMismatch(Guid? classId, Guid? programId) => Error.Validation(
        "Program.DefaultMakeupClassMismatch",
        $"Class with Id = '{classId}' does not belong to program '{programId}'");
}

