using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Sessions.SubmitTeachingLog;

public sealed class SubmitTeachingLogCommand : ICommand<SubmitTeachingLogResponse>
{
    public Guid SessionId { get; init; }
    public Guid? ActualLessonPlanTemplateId { get; init; }
    public TeachingLogTeachingType ActualTeachingType { get; init; }
    public string ProgressStatus { get; init; } = null!;
    public string? ActualContent { get; init; }
    public string? ActualHomework { get; init; }
    public string? TeacherNote { get; init; }
}
