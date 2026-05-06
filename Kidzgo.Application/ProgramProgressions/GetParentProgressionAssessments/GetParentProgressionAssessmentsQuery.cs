using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;

namespace Kidzgo.Application.ProgramProgressions.GetParentProgressionAssessments;

public sealed record GetParentProgressionAssessmentsQuery : IQuery<Page<ProgramProgressionAssessmentDto>>
{
    public Guid? StudentProfileId { get; init; }
    public ProgramProgressionAssessmentStatus? Status { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
