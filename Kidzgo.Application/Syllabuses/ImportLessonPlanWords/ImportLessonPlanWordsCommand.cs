using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.ImportLessonPlanWords;

public sealed class ImportLessonPlanWordsCommand : ICommand<ImportLessonPlanWordsResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public Guid? ModuleId { get; init; }
    public bool OverwriteExisting { get; init; } = true;
    public IReadOnlyList<ImportLessonPlanWordFile> Files { get; init; } = [];
}

public sealed class ImportLessonPlanWordFile
{
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportLessonPlanWordsResponse
{
    public int ImportedLessonPlans { get; init; }
    public int SkippedFiles { get; init; }
    public IReadOnlyList<ImportedLessonPlanWordDto> ImportedEntries { get; init; } = [];
    public IReadOnlyList<string> SkippedEntries { get; init; } = [];
}

public sealed class ImportedLessonPlanWordDto
{
    public string FileName { get; init; } = null!;
    public Guid ModuleId { get; init; }
    public Guid LessonPlanTemplateId { get; init; }
    public Guid? SessionTemplateId { get; init; }
    public int SessionIndex { get; init; }
    public bool Created { get; init; }
    public string Title { get; init; } = null!;
}
