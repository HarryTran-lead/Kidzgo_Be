namespace Kidzgo.Application.LessonPlans.GetClassLessonPlanSyllabus;

public sealed class GetClassLessonPlanSyllabusResponse
{
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string? SyllabusMetadata { get; init; }
    public IReadOnlyList<ClassLessonPlanSyllabusSessionDto> Sessions { get; init; } =
        Array.Empty<ClassLessonPlanSyllabusSessionDto>();
}

public sealed class ClassLessonPlanSyllabusSessionDto
{
    public Guid SessionId { get; init; }
    public int SessionIndex { get; init; }
    public Guid? ModuleId { get; init; }
    public int? SessionIndexInModule { get; init; }
    public DateTime SessionDate { get; init; }
    public Guid? PlannedTeacherId { get; init; }
    public string? PlannedTeacherName { get; init; }
    public Guid? ActualTeacherId { get; init; }
    public string? ActualTeacherName { get; init; }
    public Guid? LessonPlanId { get; init; }
    public Guid? TemplateId { get; init; }
    public string? TemplateTitle { get; init; }
    public string? TemplateSyllabusContent { get; init; }
    public string? PlannedContent { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNotes { get; init; }
    public bool CanEdit { get; init; }
}
