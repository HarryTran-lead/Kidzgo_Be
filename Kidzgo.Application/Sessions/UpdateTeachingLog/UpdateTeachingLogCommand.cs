using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.UpdateTeachingLog;

public sealed class UpdateTeachingLogCommand : ICommand<UpdateTeachingLogResponse>
{
    public Guid SessionId { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public TeachingLogTeachingType ActualTeachingType { get; init; }
    public string ProgressStatus { get; init; } = null!;
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNote { get; init; }
}
