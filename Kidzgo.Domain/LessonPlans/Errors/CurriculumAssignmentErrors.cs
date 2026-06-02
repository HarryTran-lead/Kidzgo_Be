using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class CurriculumAssignmentErrors
{
    public static Error NotFound(Guid assignmentId, Guid branchId) => Error.NotFound(
        "CurriculumAssignment.NotFound",
        $"Curriculum assignment '{assignmentId}' was not found in branch '{branchId}'.");

    public static Error InvalidEffectiveRange(DateTime effectiveFrom, DateTime effectiveTo) => Error.Validation(
        "CurriculumAssignment.InvalidEffectiveRange",
        $"EffectiveFrom '{effectiveFrom:O}' must be earlier than or equal to EffectiveTo '{effectiveTo:O}'.");

    public static Error SyllabusInactive(Guid syllabusId) => Error.Validation(
        "CurriculumAssignment.SyllabusInactive",
        $"Syllabus '{syllabusId}' is inactive and cannot be assigned to a branch.");

    public static Error HasOperationalClasses(Guid assignmentId, Guid syllabusId) => Error.Conflict(
        "CurriculumAssignment.HasOperationalClasses",
        $"Curriculum assignment '{assignmentId}' cannot be deleted because syllabus '{syllabusId}' is still used by operational classes.");
}
