using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessments;

public sealed class GetProgramProgressionAssessmentsQueryHandler(
    IDbContext context)
    : IQueryHandler<GetProgramProgressionAssessmentsQuery, GetProgramProgressionAssessmentsResponse>
{
    public async Task<Result<GetProgramProgressionAssessmentsResponse>> Handle(
        GetProgramProgressionAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var assessmentsQuery = ProgramProgressionAssessmentReadQuery.Build(context).AsQueryable();

        if (query.SourceRegistrationId.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.SourceRegistrationId == query.SourceRegistrationId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.SourceProgramId.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.SourceProgramId == query.SourceProgramId.Value);
        }

        if (query.SourceLevelId.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.SourceLevelId == query.SourceLevelId.Value);
        }

        if (query.TargetLevelId.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.TargetLevelId == query.TargetLevelId.Value);
        }

        if (query.Method.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.Method == query.Method.Value);
        }

        if (query.Status.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.Status == query.Status.Value);
        }

        if (query.IsEligible.HasValue)
        {
            assessmentsQuery = assessmentsQuery.Where(a => a.IsEligible == query.IsEligible.Value);
        }

        var totalCount = await assessmentsQuery.CountAsync(cancellationToken);

        var assessments = await assessmentsQuery
            .OrderByDescending(a => a.AssessmentDate)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new GetProgramProgressionAssessmentsResponse
        {
            Assessments = assessments.Select(a => a.ToDto()).ToList(),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        });
    }
}
