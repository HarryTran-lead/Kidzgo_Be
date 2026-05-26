using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;

public sealed class ImportLessonPlanTemplateFromWordCommand : ICommand<ImportLessonPlanTemplateFromWordResponse>
{
    public Guid? SyllabusId { get; init; }
    public Guid? ModuleId { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public string? LessonPlanUnitNameOverride { get; init; }
    public int? LessonPlanUnitOrderIndexOverride { get; init; }
    public int? LessonNumberOverride { get; init; }
    public int? SessionIndexOverride { get; init; }
    public bool OverwriteExisting { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportLessonPlanTemplateFromWordResponse
{
    public Guid LessonPlanTemplateId { get; init; }
    public Guid? LessonPlanUnitId { get; init; }
    public Guid? SessionTemplateId { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public int OrderIndexInUnit { get; init; }
    public bool Created { get; init; }
    public string Title { get; init; } = null!;
}
