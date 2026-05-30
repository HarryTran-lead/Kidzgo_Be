namespace Kidzgo.Application.LessonPlans.GetClassLessonPlanSyllabus;

public sealed class GetClassLessonPlanSyllabusResponse
{
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid? SyllabusId { get; init; }
    public string? SyllabusCode { get; init; }
    public string? SyllabusVersion { get; init; }
    public string? SyllabusTitle { get; init; }
    public string? SourceFileName { get; init; }
    public string? AttachmentUrl { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string? SyllabusMetadata { get; init; }
    public IReadOnlyList<ClassLessonPlanSyllabusSessionDto> Sessions { get; init; } =
        Array.Empty<ClassLessonPlanSyllabusSessionDto>();
}

public sealed class ClassLessonPlanSyllabusSessionDto
{
    public Guid SessionId { get; init; }
    public int SessionIndex { get; init; }
    public Guid? SyllabusId { get; init; }
    public Guid? ModuleId { get; init; }
    public int? SessionIndexInModule { get; init; }
    public DateTime SessionDate { get; init; }
    public string? RowRef { get; init; }
    public string? UnitName { get; init; }
    public string? LessonTitle { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public string? PlannedTeacherName { get; init; }
    public Guid? ActualTeacherId { get; init; }
    public string? ActualTeacherName { get; init; }
    public Guid? LessonPlanId { get; init; }
    public Guid? TemplateId { get; init; }
    public Guid? PlannedLessonPlanTemplateId { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public string? TemplateTitle { get; init; }
    public string? PlannedLessonTitle { get; init; }
    public string? ActualLessonTitle { get; init; }
    public string? TemplateSyllabusContent { get; init; }
    public string? PlannedContent { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public bool CanEdit { get; init; }
}
