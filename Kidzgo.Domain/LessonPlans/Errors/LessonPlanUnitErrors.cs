using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class LessonPlanUnitErrors
{
    public static Error NotFound(Guid unitId) => Error.NotFound(
        "LessonPlanUnit.NotFound",
        $"Lesson plan unit with Id = '{unitId}' was not found");

    public static Error ModuleNotFound(Guid moduleId) => Error.NotFound(
        "LessonPlanUnit.ModuleNotFound",
        $"Module with Id = '{moduleId}' was not found");

    public static readonly Error NameRequired = Error.Validation(
        "LessonPlanUnit.NameRequired",
        "Unit name is required");

    public static Error DuplicateName(Guid moduleId, string name) => Error.Conflict(
        "LessonPlanUnit.DuplicateName",
        $"Unit '{name}' already exists in module '{moduleId}'");

    public static Error HasLessonPlanTemplates(int lessonCount) => Error.Conflict(
        "LessonPlanUnit.HasLessonPlanTemplates",
        $"Cannot delete unit because it still has {lessonCount} lesson plan template(s)");

    public static readonly Error InvalidOrderIndex = Error.Validation(
        "LessonPlanUnit.InvalidOrderIndex",
        "Order index must be greater than or equal to 0");

    public static readonly Error LessonMustStayInSameModule = Error.Validation(
        "LessonPlanUnit.LessonMustStayInSameModule",
        "Lesson plan template and target unit must belong to the same module");
}
