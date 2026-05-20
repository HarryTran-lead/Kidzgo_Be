using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class LessonPlanTemplateErrors
{
    public static Error NotFound(Guid? templateId) => Error.NotFound(
        "LessonPlanTemplate.NotFound",
        $"Lesson plan template with Id = '{templateId}' was not found");

    public static Error ModuleNotFound(Guid? moduleId) => Error.NotFound(
        "LessonPlanTemplate.ModuleNotFound",
        $"Module with Id = '{moduleId}' was not found");

    public static Error LevelNotFound(Guid levelId) => Error.NotFound(
        "LessonPlanTemplate.LevelNotFound",
        $"Level with Id = '{levelId}' was not found");

    public static readonly Error SessionIndexRequired = Error.Validation(
        "LessonPlanTemplate.SessionIndexRequired",
        "SessionIndex is required and must be greater than 0");

    public static Error DuplicateSessionIndex(Guid moduleId, int sessionIndex) => Error.Conflict(
        "LessonPlanTemplate.DuplicateSessionIndex",
        $"Template with SessionIndex {sessionIndex} already exists for Module {moduleId}");

    public static Error DuplicateSessionOrder(int sessionOrder) => Error.Conflict(
        "LessonPlanTemplate.DuplicateSessionOrder",
        $"More than one template is assigned to SessionOrder {sessionOrder}");

    public static Error SessionIndexOutOfRange(int sessionIndex, int totalSessions) => Error.Validation(
        "LessonPlanTemplate.SessionIndexOutOfRange",
        $"SessionIndex {sessionIndex} must be between 1 and {totalSessions}");

    public static Error SessionOrderOutOfRange(int sessionOrder, int totalSessions) => Error.Validation(
        "LessonPlanTemplate.SessionOrderOutOfRange",
        $"SessionOrder {sessionOrder} must be between 1 and {totalSessions}");

    public static Error DoesNotBelongToLevel(Guid templateId, Guid levelId) => Error.Validation(
        "LessonPlanTemplate.DoesNotBelongToLevel",
        $"Lesson plan template '{templateId}' does not belong to level '{levelId}'");

    public static readonly Error HasActiveLessonPlans = Error.Conflict(
        "LessonPlanTemplate.HasActiveLessonPlans",
        "Cannot delete template that has active lesson plans");

    public static Error UnsupportedImportFileType(string extension) => Error.Validation(
        "LessonPlanTemplate.UnsupportedImportFileType",
        $"Unsupported syllabus import file type '{extension}'. Only .csv, .xlsx, and .xls are supported");

    public static readonly Error ImportFileRequiresModuleId = Error.Validation(
        "LessonPlanTemplate.ImportFileRequiresModuleId",
        "ModuleId is required when importing a syllabus file");

    public static Error InvalidImportFile(string message) => Error.Validation(
        "LessonPlanTemplate.InvalidImportFile",
        message);

    public static Error ModuleMappingNotFound(string sheetName) => Error.NotFound(
        "LessonPlanTemplate.ModuleMappingNotFound",
        $"Could not map syllabus sheet '{sheetName}' to an active module");

    public static readonly Error Unauthorized = Error.Validation(
        "LessonPlanTemplate.Unauthorized",
        "You do not have permission to modify this lesson plan template");
}
