using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.Assessments.GetAssessmentsByStudent;

public sealed class GetAssessmentsByStudentResponse
{
    public List<AssessmentDto> Items { get; init; } = [];
}
