using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanUnits.CreateLessonPlanUnit;

public sealed class CreateLessonPlanUnitCommand : ICommand<CreateLessonPlanUnitResponse>
{
    public Guid ModuleId { get; init; }
    public string Name { get; init; } = null!;
}

public sealed class CreateLessonPlanUnitResponse
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public string Name { get; init; } = null!;
    public int OrderIndex { get; init; }
    public bool IsActive { get; init; }
}
