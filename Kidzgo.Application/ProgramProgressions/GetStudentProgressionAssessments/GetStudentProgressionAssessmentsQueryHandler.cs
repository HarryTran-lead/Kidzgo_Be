using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetStudentProgressionAssessments;

public sealed class GetStudentProgressionAssessmentsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentProgressionAssessmentsQuery, Page<ProgramProgressionAssessmentDto>>
{
    public async Task<Result<Page<ProgramProgressionAssessmentDto>>> Handle(
        GetStudentProgressionAssessmentsQuery query,
        CancellationToken cancellationToken)
    {
        var studentProfile = await ResolveStudentProfileAsync(cancellationToken);
        if (studentProfile is null)
        {
            return Result.Failure<Page<ProgramProgressionAssessmentDto>>(
                Error.NotFound("Student.NotFound", "Student profile not found"));
        }

        var assessmentQuery = ProgramProgressionAssessmentReadQuery.Build(context)
            .Where(a => a.StudentProfileId == studentProfile.Id);

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

    private async Task<Profile?> ResolveStudentProfileAsync(CancellationToken cancellationToken)
    {
        IQueryable<Profile> query = context.Profiles
            .AsNoTracking()
            .Where(p => p.ProfileType == ProfileType.Student && !p.IsDeleted);

        if (userContext.StudentId.HasValue)
        {
            query = query.Where(p => p.Id == userContext.StudentId.Value);
        }
        else
        {
            query = query.Where(p => p.UserId == userContext.UserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
