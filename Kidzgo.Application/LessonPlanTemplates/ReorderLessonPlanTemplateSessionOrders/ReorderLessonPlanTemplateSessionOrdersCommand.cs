using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.ReorderLessonPlanTemplateSessionOrders;

public sealed class ReorderLessonPlanTemplateSessionOrdersCommand
    : ICommand<ReorderLessonPlanTemplateSessionOrdersResponse>
{
    public Guid LevelId { get; init; }
    public IReadOnlyList<ReorderLessonPlanTemplateSessionOrderItem> Items { get; init; } =
        Array.Empty<ReorderLessonPlanTemplateSessionOrderItem>();
}

public sealed class ReorderLessonPlanTemplateSessionOrderItem
{
    public Guid Id { get; init; }
    public int SessionOrder { get; init; }
}

public sealed class ReorderLessonPlanTemplateSessionOrdersResponse
{
    public Guid LevelId { get; init; }
    public IReadOnlyList<ReorderedLessonPlanTemplateSessionOrderDto> Items { get; init; } =
        Array.Empty<ReorderedLessonPlanTemplateSessionOrderDto>();
}

public sealed class ReorderedLessonPlanTemplateSessionOrderDto
{
    public Guid Id { get; init; }
    public Guid ModuleId { get; init; }
    public int SessionIndex { get; init; }
    public int SessionOrder { get; init; }
    public DateTime UpdatedAt { get; init; }
}
