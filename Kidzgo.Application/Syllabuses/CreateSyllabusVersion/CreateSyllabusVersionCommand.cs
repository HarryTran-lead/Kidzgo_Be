using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.CreateSyllabusVersion;

public sealed class CreateSyllabusVersionCommand : ICommand<CreateSyllabusVersionResponse>
{
    public Guid SourceSyllabusId { get; init; }
    public int Version { get; init; }
    public string? Title { get; init; }
    public string? Edition { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool PromoteNow { get; init; }
}

public sealed class CreateSyllabusVersionResponse
{
    public Guid SyllabusId { get; init; }
    public Guid SourceSyllabusId { get; init; }
    public string Code { get; init; } = null!;
    public int Version { get; init; }
    public string Title { get; init; } = null!;
    public bool IsActive { get; init; }
}
