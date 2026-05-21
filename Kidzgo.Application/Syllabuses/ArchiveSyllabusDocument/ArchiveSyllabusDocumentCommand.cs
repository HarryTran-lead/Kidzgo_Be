using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.ArchiveSyllabusDocument;

public sealed class ArchiveSyllabusDocumentCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public int ExpectedVersion { get; init; }
    public string? Reason { get; init; }
}
