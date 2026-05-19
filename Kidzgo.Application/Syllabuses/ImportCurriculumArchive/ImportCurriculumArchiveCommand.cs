using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.ImportCurriculumArchive;

public sealed class ImportCurriculumArchiveCommand : ICommand<ImportCurriculumArchiveResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string Code { get; init; } = null!;
    public string Version { get; init; } = null!;
    public bool OverwriteExisting { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportCurriculumArchiveResponse
{
    public Guid? SyllabusId { get; init; }
    public int ImportedLessonPlans { get; init; }
    public int SkippedFiles { get; init; }
    public IReadOnlyList<string> SkippedEntries { get; init; } = [];
}
