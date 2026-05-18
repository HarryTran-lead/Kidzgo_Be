using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.Levels.GetLevels;

public sealed class GetLevelsResponse
{
    public List<LevelDto> Items { get; init; } = [];
}
