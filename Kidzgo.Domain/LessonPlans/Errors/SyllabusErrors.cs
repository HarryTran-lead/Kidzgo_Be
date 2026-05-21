using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.LessonPlans.Errors;

public static class SyllabusErrors
{
    public static Error NotFound(Guid? syllabusId) => Error.NotFound(
        "Syllabus.NotFound",
        $"Syllabus with Id = '{syllabusId}' was not found");

    public static Error ProgramNotFound(Guid? programId) => Error.NotFound(
        "Syllabus.ProgramNotFound",
        $"Program with Id = '{programId}' was not found");

    public static Error LevelNotFound(Guid? levelId) => Error.NotFound(
        "Syllabus.LevelNotFound",
        $"Level with Id = '{levelId}' was not found");

    public static Error LevelDoesNotBelongToProgram(Guid levelId, Guid programId) => Error.Validation(
        "Syllabus.LevelDoesNotBelongToProgram",
        $"Level '{levelId}' does not belong to Program '{programId}'");

    public static Error DuplicateVersion(Guid programId, Guid levelId, string code, string version) => Error.Conflict(
        "Syllabus.DuplicateVersion",
        $"Syllabus '{code}' version '{version}' already exists for Program '{programId}' and Level '{levelId}'");

    public static Error UnsupportedImportFileType(string extension) => Error.Validation(
        "Syllabus.UnsupportedImportFileType",
        $"Unsupported syllabus import file type '{extension}'. Only .docx and .zip are supported");

    public static Error InvalidImportFile(string message) => Error.Validation(
        "Syllabus.InvalidImportFile",
        message);

    public static Error ModuleMappingNotFound(string identifier) => Error.NotFound(
        "Syllabus.ModuleMappingNotFound",
        $"Could not map imported content to a module using '{identifier}'");

    public static Error ImportConfigurationNotFound(Guid programId, Guid levelId) => Error.NotFound(
        "Syllabus.ImportConfigurationNotFound",
        $"Curriculum import configuration for Program '{programId}' and Level '{levelId}' was not found");

    public static Error InvalidImportConfiguration(string message) => Error.Validation(
        "Syllabus.InvalidImportConfiguration",
        message);

    public static Error DuplicateCode(Guid programId, Guid levelId, string code) => Error.Conflict(
        "Syllabus.DuplicateCode",
        $"Syllabus code '{code}' already exists for Program '{programId}' and Level '{levelId}' in an active document state.");

    public static Error VersionConflict(int expectedVersion, int currentVersion) => Error.Conflict(
        "Syllabus.VersionConflict",
        $"Version conflict. Please reload document. Expected version {expectedVersion}, current version {currentVersion}.");

    public static readonly Error PublishedReadOnly = Error.Conflict(
        "Syllabus.PublishedReadOnly",
        "Published syllabus cannot be edited directly. Create or clone a draft before editing.");

    public static Error InvalidTableLayout(string message) => Error.Validation(
        "Syllabus.InvalidTableLayout",
        message);

    public static Error PublishValidationFailed(string message) => Error.Validation(
        "Syllabus.PublishValidationFailed",
        message);

    public static Error ImportParseFailed(string message) => Error.Validation(
        "Syllabus.ImportParseFailed",
        message);

    public static Error SectionNotFound(Guid sectionId) => Error.NotFound(
        "Syllabus.SectionNotFound",
        $"Syllabus section '{sectionId}' was not found");

    public static Error RowNotFound(Guid rowId) => Error.NotFound(
        "Syllabus.RowNotFound",
        $"Syllabus table row '{rowId}' was not found");

    public static Error CellNotFound(string columnKey) => Error.NotFound(
        "Syllabus.CellNotFound",
        $"Syllabus table cell '{columnKey}' was not found");
}
