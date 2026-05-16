using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.TeacherEvaluations.GetTeacherEvaluationsByStudent;

public sealed class GetTeacherEvaluationsByStudentResponse
{
    public List<TeacherEvaluationDto> Items { get; init; } = [];
}
