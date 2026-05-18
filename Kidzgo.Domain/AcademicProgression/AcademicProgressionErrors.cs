using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.AcademicProgression;

public static class AcademicProgressionErrors
{
    public static Error LevelNotFound(Guid id) =>
        Error.NotFound("AcademicProgression.LevelNotFound", $"Level '{id}' was not found.");

    public static Error ModuleNotFound(Guid id) =>
        Error.NotFound("AcademicProgression.ModuleNotFound", $"Module '{id}' was not found.");

    public static Error StudentProgressNotFound(Guid studentProfileId) =>
        Error.NotFound("AcademicProgression.StudentProgressNotFound", $"Student progress for '{studentProfileId}' was not found.");

    public static Error AssessmentNotFound(Guid id) =>
        Error.NotFound("AcademicProgression.AssessmentNotFound", $"Assessment '{id}' was not found.");

    public static Error TeacherEvaluationNotFound(Guid id) =>
        Error.NotFound("AcademicProgression.TeacherEvaluationNotFound", $"Teacher evaluation '{id}' was not found.");

    public static Error PromotionDecisionNotFound(Guid id) =>
        Error.NotFound("AcademicProgression.PromotionDecisionNotFound", $"Promotion decision '{id}' was not found.");

    public static Error RemedialPlanNotFound(Guid id) =>
        Error.NotFound("AcademicProgression.RemedialPlanNotFound", $"Remedial plan '{id}' was not found.");
}
