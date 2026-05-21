using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.DeleteSyllabusTableRow;

public sealed class DeleteSyllabusTableRowCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public Guid SectionId { get; init; }
    public Guid RowId { get; init; }
    public int ExpectedVersion { get; init; }
}
