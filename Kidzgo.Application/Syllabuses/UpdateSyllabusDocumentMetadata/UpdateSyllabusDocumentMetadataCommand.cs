using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabusDocumentMetadata;

public sealed class UpdateSyllabusDocumentMetadataCommand : ICommand<SyllabusDocumentResponse>
{
    public Guid Id { get; init; }
    public int ExpectedVersion { get; init; }
    public string Code { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string? Edition { get; init; }
    public int? MinutesPerPeriod { get; init; }
}
