using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.ImportSyllabusPreview;

public sealed class ImportSyllabusPreviewCommand : ICommand<SyllabusImportPreviewResponse>
{
    public Guid ProgramId { get; init; }
    public Guid LevelId { get; init; }
    public string FileName { get; init; } = null!;
    public Stream FileStream { get; init; } = null!;
}
