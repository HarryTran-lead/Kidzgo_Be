using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.GetSyllabusDocument;

public sealed class GetSyllabusDocumentQuery : IQuery<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
}
