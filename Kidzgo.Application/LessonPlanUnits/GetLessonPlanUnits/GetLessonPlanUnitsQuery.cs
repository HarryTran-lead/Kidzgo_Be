using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanUnits.GetLessonPlanUnits;

public sealed class GetLessonPlanUnitsQuery : IQuery<GetLessonPlanUnitsResponse>
{
    public Guid ModuleId { get; init; }
}

public sealed class GetLessonPlanUnitsResponse
{
    public IReadOnlyList<LessonPlanUnitDto> Items { get; init; } = [];
}

public sealed class LessonPlanUnitDto
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string Name { get; init; } = null!;
    public int OrderIndex { get; init; }
    public int LessonCount { get; init; }
    public bool IsActive { get; init; }
}
