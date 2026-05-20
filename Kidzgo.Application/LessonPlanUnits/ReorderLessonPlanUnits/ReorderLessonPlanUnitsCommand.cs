using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanUnits.ReorderLessonPlanUnits;

public sealed class ReorderLessonPlanUnitsCommand : ICommand
{
    public Guid ModuleId { get; init; }
    public IReadOnlyList<ReorderLessonPlanUnitItem> Items { get; init; } = [];
}

public sealed class ReorderLessonPlanUnitItem
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
}
