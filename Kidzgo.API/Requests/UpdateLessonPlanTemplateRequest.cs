namespace Kidzgo.API.Requests;

public sealed class UpdateLessonPlanTemplateRequest
{
    public Guid? SyllabusId { get; init; }
    public Guid? ModuleId { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public int? OrderIndexInUnit { get; init; }
    public string? Title { get; init; }
    public int? SessionIndex { get; init; }
    public int? SessionOrder { get; init; }
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
    public bool? IsActive { get; init; }
}
