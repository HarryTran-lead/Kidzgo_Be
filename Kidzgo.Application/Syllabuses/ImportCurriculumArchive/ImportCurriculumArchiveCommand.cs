using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.ImportCurriculumArchive;

public sealed class ImportCurriculumArchiveCommand : ICommand<ImportCurriculumArchiveResponse>
{
    public Guid? BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string Code { get; init; } = null!;
    public int Version { get; init; }
    public bool OverwriteExisting { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}

public sealed class ImportCurriculumArchiveResponse
{
    public string ArchiveFileName { get; init; } = null!;
    public string ArchiveParserVersion { get; init; } = null!;
    public Guid? SyllabusId { get; init; }
    public string? SelectedSyllabusEntryName { get; init; }
    public string? SelectedSyllabusNormalizedEntryName { get; init; }
    public string? SelectedSyllabusFileName { get; init; }
    public string? SelectedSyllabusSourceType { get; init; }
    public string? SelectedSyllabusParserVersion { get; init; }
    public int ImportedLessonPlans { get; init; }
    public int SkippedFiles { get; init; }
    public IReadOnlyList<ImportedCurriculumArchiveEntryDto> ImportedEntries { get; init; } = [];
    public IReadOnlyList<string> SkippedEntries { get; init; } = [];
    public IReadOnlyList<SkippedCurriculumArchiveEntryDto> SkippedItems { get; init; } = [];
}

public sealed class ImportedCurriculumArchiveEntryDto
{
    public string EntryName { get; init; } = null!;
    public string NormalizedEntryName { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public string? SourceFolder { get; init; }
    public string SourceType { get; init; } = null!;
    public string? ParserVersion { get; init; }
    public bool IsPrimarySyllabusSource { get; init; }
    public Guid? ModuleId { get; init; }
    public string? ModuleName { get; init; }
    public Guid? LessonPlanTemplateId { get; init; }
    public Guid? SessionTemplateId { get; init; }
    public int? SessionIndex { get; init; }
    public int? SessionOrder { get; init; }
    public bool Created { get; init; }
    public string Title { get; init; } = null!;
}

public sealed class SkippedCurriculumArchiveEntryDto
{
    public string EntryName { get; init; } = null!;
    public string NormalizedEntryName { get; init; } = null!;
    public string FileName { get; init; } = null!;
    public string? SourceFolder { get; init; }
    public string SourceType { get; init; } = null!;
    public string? ParserVersion { get; init; }
    public string Reason { get; init; } = null!;
}
