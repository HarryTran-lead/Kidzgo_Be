using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.StudentProgress.UpdateStudentProgress;

public sealed class UpdateStudentProgressCommand : ICommand<StudentProgressDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public Guid? CurrentLessonPlanTemplateId { get; init; }
    public decimal? CompletionPercent { get; init; }
}
