using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.ImportSyllabusCommit;

public sealed class ImportSyllabusCommitCommand : ICommand<SyllabusImportCommitResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string Code { get; init; } = null!;
    public string? Title { get; init; }
    public string? Edition { get; init; }
    public bool AsDraft { get; init; } = true;
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}
