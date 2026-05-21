using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabusSection;

public sealed class UpdateSyllabusSectionCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public Guid SectionId { get; init; }
    public int ExpectedVersion { get; init; }
    public string? Title { get; init; }
    public string? Content { get; init; }
    public IReadOnlyList<string>? Items { get; init; }
}
