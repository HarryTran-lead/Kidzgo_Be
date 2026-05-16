using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.AcademicProgression.RemedialPlans.GetRemedialPlansByStudent;

public sealed class GetRemedialPlansByStudentQuery : IQuery<GetRemedialPlansByStudentResponse>
{
    public Guid StudentProfileId { get; init; }
}
