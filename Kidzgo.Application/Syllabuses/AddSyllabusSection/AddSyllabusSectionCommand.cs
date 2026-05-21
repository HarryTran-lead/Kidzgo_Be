using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.AddSyllabusSection;

public sealed class AddSyllabusSectionCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public int ExpectedVersion { get; init; }
    public SyllabusDocumentSectionDto Section { get; init; } = new();
}
