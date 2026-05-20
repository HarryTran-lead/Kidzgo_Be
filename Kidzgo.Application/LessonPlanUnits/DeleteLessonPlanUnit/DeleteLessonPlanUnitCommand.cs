using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanUnits.DeleteLessonPlanUnit;

public sealed class DeleteLessonPlanUnitCommand : ICommand
{
    public Guid Id { get; init; }
}
