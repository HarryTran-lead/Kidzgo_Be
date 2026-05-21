using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.PublishSyllabusDocument;

public sealed class PublishSyllabusDocumentCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public int ExpectedVersion { get; init; }
}
