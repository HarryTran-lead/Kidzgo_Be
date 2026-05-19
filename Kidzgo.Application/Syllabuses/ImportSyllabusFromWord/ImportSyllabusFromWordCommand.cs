using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;

public sealed class ImportSyllabusFromWordCommand : ICommand<ImportSyllabusFromWordResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string Code { get; init; } = null!;
    public string Version { get; init; } = null!;
    public bool OverwriteExisting { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportSyllabusFromWordResponse
{
    public Guid SyllabusId { get; init; }
    public int ImportedUnits { get; init; }
    public int ImportedLessons { get; init; }
    public int ImportedResources { get; init; }
    public int ImportedSessionTemplates { get; init; }
}
