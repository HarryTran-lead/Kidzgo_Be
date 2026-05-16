using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.AcademicProgression.Levels.GetLevels;

public sealed class GetLevelsQuery : IQuery<GetLevelsResponse>
{
    public Guid? ProgramId { get; init; }
    public bool? IsActive { get; init; }
    public string? SearchTerm { get; init; }
}
