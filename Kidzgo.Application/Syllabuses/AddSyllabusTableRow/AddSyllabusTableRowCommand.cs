using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.AddSyllabusTableRow;

public sealed class AddSyllabusTableRowCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public Guid SectionId { get; init; }
    public int ExpectedVersion { get; init; }
    public int OrderIndex { get; init; }
    public IReadOnlyList<SyllabusDocumentTableCellDto> Cells { get; init; } = [];
}
