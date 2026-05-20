using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanUnits.ReorderLessonPlanUnitLessons;

public sealed class ReorderLessonPlanUnitLessonsCommand : ICommand
{
    public Guid UnitId { get; init; }
    public IReadOnlyList<ReorderLessonPlanUnitLessonItem> Items { get; init; } = [];
}

public sealed class ReorderLessonPlanUnitLessonItem
{
    public Guid Id { get; init; }
    public int OrderIndexInUnit { get; init; }
}
