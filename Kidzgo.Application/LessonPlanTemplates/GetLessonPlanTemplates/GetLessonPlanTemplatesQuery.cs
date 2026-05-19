using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;

public sealed class GetLessonPlanTemplatesQuery : IQuery<GetLessonPlanTemplatesResponse>
{
    public Guid? ModuleId { get; init; }
    public string? Title { get; init; }
    public bool? IsActive { get; init; }
    public bool IncludeDeleted { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
