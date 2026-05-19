using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Programs.Errors;

public static class TuitionPlanErrors
{
    public static Error NotFound(Guid? tuitionPlanId) => Error.NotFound(
        "TuitionPlan.NotFound",
        $"Tuition Plan with Id = '{tuitionPlanId}' was not found");

    public static readonly Error ProgramNotFound = Error.NotFound(
        "TuitionPlan.ProgramNotFound",
        "Program not found or deleted");

    public static readonly Error LevelNotFound = Error.NotFound(
        "TuitionPlan.LevelNotFound",
        "Level not found");

    public static readonly Error LevelProgramMismatch = Error.Validation(
        "TuitionPlan.LevelProgramMismatch",
        "Level does not belong to the selected program");

    public static readonly Error ModuleNotFound = Error.NotFound(
        "TuitionPlan.ModuleNotFound",
        "Module not found");

    public static readonly Error ModuleLevelMismatch = Error.Validation(
        "TuitionPlan.ModuleLevelMismatch",
        "Module does not belong to the selected level");

    public static readonly Error BranchNotFound = Error.NotFound(
        "TuitionPlan.BranchNotFound",
        "Branch not found or inactive");

    public static readonly Error ProgramNotAvailableInBranch = Error.Validation(
        "TuitionPlan.ProgramNotAvailableInBranch",
        "Program is not assigned to the selected branch");

    public static readonly Error HasActiveEnrollments = Error.Conflict(
        "TuitionPlan.HasActiveEnrollments",
        "Cannot delete tuition plan with active enrollments");
}

