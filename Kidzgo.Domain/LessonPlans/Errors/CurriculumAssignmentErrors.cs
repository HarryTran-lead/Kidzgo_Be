using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class CurriculumAssignmentErrors
{
    public static Error InvalidEffectiveRange(DateTime effectiveFrom, DateTime effectiveTo) => Error.Validation(
        "CurriculumAssignment.InvalidEffectiveRange",
        $"EffectiveFrom '{effectiveFrom:O}' must be earlier than or equal to EffectiveTo '{effectiveTo:O}'.");

    public static Error SyllabusInactive(Guid syllabusId) => Error.Validation(
        "CurriculumAssignment.SyllabusInactive",
        $"Syllabus '{syllabusId}' is inactive and cannot be assigned to a branch.");
}
