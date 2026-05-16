using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.AcademicProgression.TeacherEvaluations.GetTeacherEvaluationsByStudent;

public sealed class GetTeacherEvaluationsByStudentQuery : IQuery<GetTeacherEvaluationsByStudentResponse>
{
    public Guid StudentProfileId { get; init; }
}
