using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.PromoteSyllabusVersion;

public sealed class PromoteSyllabusVersionCommand : ICommand<PromoteSyllabusVersionResponse>
{
    public Guid SourceSyllabusId { get; init; }
    public Guid TargetSyllabusId { get; init; }
}

public sealed class PromoteSyllabusVersionResponse
{
    public Guid SyllabusId { get; init; }
    public string Version { get; init; } = null!;
    public bool IsActive { get; init; }
}
