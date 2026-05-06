using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetParentProgressionAssessments;

public sealed class GetParentProgressionAssessmentsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetParentProgressionAssessmentsQuery, Page<ProgramProgressionAssessmentDto>>
{
    public async Task<Result<Page<ProgramProgressionAssessmentDto>>> Handle(
        GetParentProgressionAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var parentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.UserId == userContext.UserId &&
                p.ProfileType == ProfileType.Parent &&
                !p.IsDeleted &&
                p.IsActive,
                cancellationToken);

        if (parentProfile is null)
        {
            return Result.Failure<Page<ProgramProgressionAssessmentDto>>(
                Error.NotFound("Parent.NotFound", "Parent profile not found"));
        }

        var linkedStudentIds = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link => link.ParentProfileId == parentProfile.Id)
            .Select(link => link.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (linkedStudentIds.Count == 0)
        {
            return Result.Success(new Page<ProgramProgressionAssessmentDto>(
                new List<ProgramProgressionAssessmentDto>(),
                0,
                query.PageNumber,
                query.PageSize));
        }

        Guid? requestedStudentId = null;
        if (query.StudentProfileId.HasValue)
        {
            if (!linkedStudentIds.Contains(query.StudentProfileId.Value))
            {
                return Result.Failure<Page<ProgramProgressionAssessmentDto>>(
                    Error.Validation("Parent.NotLinked", "Student profile not linked to current parent"));
            }
            requestedStudentId = query.StudentProfileId.Value;
        }

        var assessmentQuery = ProgramProgressionAssessmentReadQuery.Build(context)
            .Where(a => requestedStudentId.HasValue
                ? a.StudentProfileId == requestedStudentId.Value
                : linkedStudentIds.Contains(a.StudentProfileId));

        if (query.Status.HasValue)
        {
            assessmentQuery = assessmentQuery.Where(a => a.Status == query.Status.Value);
        }

        var totalCount = await assessmentQuery.CountAsync(cancellationToken);

        var items = await assessmentQuery
            .OrderByDescending(a => a.AssessmentDate)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(a => a.ToDto()).ToList();

        return Result.Success(new Page<ProgramProgressionAssessmentDto>(
            dtos,
            totalCount,
            query.PageNumber,
            query.PageSize));
    }
}
