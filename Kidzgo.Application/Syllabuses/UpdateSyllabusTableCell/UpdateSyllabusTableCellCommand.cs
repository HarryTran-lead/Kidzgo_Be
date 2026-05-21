using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabusTableCell;

public sealed class UpdateSyllabusTableCellCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public Guid SectionId { get; init; }
    public Guid RowId { get; init; }
    public string ColumnKey { get; init; } = null!;
    public int ExpectedVersion { get; init; }
    public string? Value { get; init; }
    public int? RowSpan { get; init; }
    public int? ColSpan { get; init; }
    public string? Align { get; init; }
    public bool? Bold { get; init; }
}
