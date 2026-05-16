using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.Levels.CreateLevel;

public sealed class CreateLevelCommand : ICommand<LevelDto>
{
    public Guid ProgramId { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public int Order { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
