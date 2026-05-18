using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.TeacherEvaluations.CreateTeacherEvaluation;

public sealed class CreateTeacherEvaluationCommand : ICommand<TeacherEvaluationDto>
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public int Speaking { get; init; }
    public int Listening { get; init; }
    public int Reading { get; init; }
    public int Writing { get; init; }
    public int Participation { get; init; }
    public int Confidence { get; init; }
    public int Behavior { get; init; }
    public string? Notes { get; init; }
    public DateTime? EvaluatedAt { get; init; }
}
