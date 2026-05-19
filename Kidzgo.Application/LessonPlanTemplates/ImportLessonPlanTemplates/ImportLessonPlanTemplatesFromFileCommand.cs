using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplates;

public sealed class ImportLessonPlanTemplatesFromFileCommand : ICommand<ImportLessonPlanTemplatesFromFileResponse>
{
    public Guid? ModuleId { get; init; }
    public bool OverwriteExisting { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportLessonPlanTemplatesFromFileResponse
{
    public int ImportedCount { get; init; }
    public IReadOnlyList<ImportedLessonPlanTemplateModuleDto> Modules { get; init; } =
        Array.Empty<ImportedLessonPlanTemplateModuleDto>();
}

public sealed record ImportedLessonPlanTemplateModuleDto
{
    public Guid ModuleId { get; init; }
    public string ModuleName { get; init; } = null!;
    public int ImportedSessions { get; init; }
}
