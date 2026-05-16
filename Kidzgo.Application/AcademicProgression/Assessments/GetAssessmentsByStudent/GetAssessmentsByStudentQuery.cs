using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.AcademicProgression.Assessments.GetAssessmentsByStudent;

public sealed class GetAssessmentsByStudentQuery : IQuery<GetAssessmentsByStudentResponse>
{
    public Guid StudentProfileId { get; init; }
}
