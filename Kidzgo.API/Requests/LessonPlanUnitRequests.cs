namespace Kidzgo.API.Requests;

public sealed class CreateLessonPlanUnitRequest
{
    public string Name { get; init; } = null!;
}

public sealed class UpdateLessonPlanUnitRequest
{
    public string? Name { get; init; }
    public bool? IsActive { get; init; }
}

public sealed class ReorderLessonPlanUnitRequest
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class MoveLessonPlanTemplateUnitRequest
{
    public Guid? LessonPlanUnitId { get; init; }
    public int? OrderIndexInUnit { get; init; }
}

public sealed class ReorderLessonPlanUnitLessonRequest
{
    public Guid Id { get; init; }
    public int OrderIndexInUnit { get; init; }
}
