using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessments;

public sealed class GetProgramProgressionAssessmentsQuery : IQuery<GetProgramProgressionAssessmentsResponse>
{
    public Guid? SourceRegistrationId { get; init; }
    public Guid? StudentProfileId { get; init; }
    public Guid? SourceProgramId { get; init; }
    public Guid? SourceLevelId { get; init; }
    public Guid? TargetLevelId { get; init; }
    public ProgramProgressionMethod? Method { get; init; }
    public ProgramProgressionAssessmentStatus? Status { get; init; }
    public bool? IsEligible { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
