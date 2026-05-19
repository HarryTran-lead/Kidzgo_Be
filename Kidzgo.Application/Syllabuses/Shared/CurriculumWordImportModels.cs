namespace Kidzgo.Application.Syllabuses.Shared;

internal sealed record ParsedSyllabusDocument(
    string Title,
    string? Edition,
    string? Overview,
    List<ParsedSyllabusUnit> Units,
    List<ParsedSyllabusLesson> Lessons,
    List<ParsedSyllabusResource> Resources,
    string RawText);

internal sealed record ParsedSyllabusUnit(
    string Name,
    int OrderIndex,
    int? AllocatedPeriods,
    int? LessonCount,
    string? Notes,
    string? ModuleHint);

internal sealed record ParsedSyllabusLesson(
    int OrderIndex,
    int? PeriodFrom,
    int? PeriodTo,
    string? Topic,
    int? LessonNumber,
    string? ContentSummary,
    string? StructureSummary,
    string? Components,
    string? StudentBookPages,
    string? TeacherBookPages,
    string? ModuleHint);

internal sealed record ParsedSyllabusResource(
    int OrderIndex,
    string? DocumentName,
    string? Abbreviation,
    string? IntendedUsers,
    string? Notes);

internal sealed record ParsedLessonPlanDocument(
    string? UnitTitle,
    string? ModuleHint,
    int? LessonNumber,
    string Title,
    string? Objectives,
    string? LanguageContent,
    string? Vocabulary,
    string? Grammar,
    string? TeachingMethodology,
    string? TeacherMaterials,
    string? StudentMaterials,
    string? Procedure,
    string? Evaluation,
    string? Homework,
    string RawText);
