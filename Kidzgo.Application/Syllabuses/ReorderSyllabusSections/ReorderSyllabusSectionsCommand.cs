using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.ReorderSyllabusSections;

public sealed class ReorderSyllabusSectionsCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public int ExpectedVersion { get; init; }
    public IReadOnlyList<ReorderSyllabusSectionItem> Orders { get; init; } = [];
}

public sealed class ReorderSyllabusSectionItem
{
    public Guid SectionId { get; init; }
    public int OrderIndex { get; init; }
}
