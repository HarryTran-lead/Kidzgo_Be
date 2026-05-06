using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessments;

public sealed class GetProgramProgressionAssessmentsResponse
{
    public List<ProgramProgressionAssessmentDto> Assessments { get; init; } = new();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}
