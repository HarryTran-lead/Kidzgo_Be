using Kidzgo.Application.AcademicProgression.Shared;

namespace Kidzgo.Application.AcademicProgression.RemedialPlans.GetRemedialPlansByStudent;

public sealed class GetRemedialPlansByStudentResponse
{
    public List<RemedialPlanDto> Items { get; init; } = [];
}
