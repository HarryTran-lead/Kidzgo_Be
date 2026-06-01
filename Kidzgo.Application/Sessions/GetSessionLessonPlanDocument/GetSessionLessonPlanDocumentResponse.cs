using Kidzgo.Application.Sessions.Shared;

namespace Kidzgo.Application.Sessions.GetSessionLessonPlanDocument;

public sealed class GetSessionLessonPlanDocumentResponse
{
    public Guid SessionId { get; init; }
    public Guid ClassId { get; init; }
    public Guid? SyllabusId { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public int? SessionIndexInModule { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public Guid? PlannedLessonPlanTemplateId { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public string? PlannedLessonTitle { get; init; }
    public string? ActualLessonTitle { get; init; }
    public Guid? TeachingLogId { get; init; }
    public string? TeachingLogStatus { get; init; }
    public string? TeachingProgressStatus { get; init; }
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNote { get; init; }
    public string? TeacherNotes => TeacherNote;
    public TeachingLogSnapshotDto? TeachingLog { get; init; }
    public SessionLessonPlanDocumentDto Document { get; init; } = null!;
}

public sealed class SessionLessonPlanDocumentDto
{
    public Guid Id { get; init; }
    public Guid SyllabusId { get; init; }
    public string SyllabusCode { get; init; } = null!;
    public string SyllabusVersion { get; init; } = null!;
    public string SyllabusTitle { get; init; } = null!;
    public Guid ModuleId { get; init; }
    public string ModuleCode { get; init; } = null!;
    public string ModuleName { get; init; } = null!;
    public Guid? LessonPlanUnitId { get; init; }
    public string? LessonPlanUnitName { get; init; }
    public int OrderIndexInUnit { get; init; }
    public Guid LevelId { get; init; }
    public string LevelName { get; init; } = null!;
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public string? Title { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public string? SyllabusMetadata { get; init; }
    public string? SyllabusContent { get; init; }
    public string? Objectives { get; init; }
    public string? LanguageContent { get; init; }
    public string? Vocabulary { get; init; }
    public string? Grammar { get; init; }
    public string? TeachingMethodology { get; init; }
    public string? TeacherMaterials { get; init; }
    public string? StudentMaterials { get; init; }
    public string? Procedure { get; init; }
    public string? Evaluation { get; init; }
    public string? SourceFileName { get; init; }
    public string? Attachment { get; init; }
    public bool IsActive { get; init; }
    public Guid? CreatedBy { get; init; }
    public string? CreatedByName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
