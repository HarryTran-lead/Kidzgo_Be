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

    public static readonly Error SyllabusNotFound = Error.NotFound(
        "TuitionPlan.SyllabusNotFound",
        "Syllabus not found or deleted");

    public static readonly Error SyllabusInactive = Error.Validation(
        "TuitionPlan.SyllabusInactive",
        "Syllabus is inactive and cannot be mapped to package");

    public static readonly Error SyllabusProgramMismatch = Error.Validation(
        "TuitionPlan.SyllabusProgramMismatch",
        "Syllabus must belong to the same program as the package");

    public static readonly Error SyllabusLevelMismatch = Error.Validation(
        "TuitionPlan.SyllabusLevelMismatch",
        "Syllabus must belong to the same level as the package");

    public static Error CurriculumAlreadyMapped(Guid tuitionPlanId, Guid syllabusId) => Error.Conflict(
        "TuitionPlan.CurriculumAlreadyMapped",
        $"Package '{tuitionPlanId}' is already mapped to syllabus '{syllabusId}'");

    public static readonly Error HasActiveEnrollments = Error.Conflict(
        "TuitionPlan.HasActiveEnrollments",
        "Cannot delete tuition plan with active enrollments");
}

