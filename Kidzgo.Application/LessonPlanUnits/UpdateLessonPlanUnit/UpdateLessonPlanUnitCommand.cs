using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanUnits.UpdateLessonPlanUnit;

public sealed class UpdateLessonPlanUnitCommand : ICommand<UpdateLessonPlanUnitResponse>
{
    public Guid Id { get; init; }
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class UpdateLessonPlanUnitResponse
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string Name { get; init; } = null!;
    public int OrderIndex { get; init; }
    public bool IsActive { get; init; }
    public DateTime UpdatedAt { get; init; }
}
