using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.GetPackageSyllabuses;

public sealed class GetPackageSyllabusesQueryHandler(IDbContext context)
    : IQueryHandler<GetPackageSyllabusesQuery, GetPackageSyllabusesResponse>
{
    public async Task<Result<GetPackageSyllabusesResponse>> Handle(
        GetPackageSyllabusesQuery query,
        CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.TuitionPlanId && !x.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<GetPackageSyllabusesResponse>(TuitionPlanErrors.NotFound(query.TuitionPlanId));
        }

        var syllabuses = await context.PackageCurriculumMappings
            .AsNoTracking()
            .Where(x => x.TuitionPlanId == query.TuitionPlanId && x.IsActive && !x.Syllabus.IsDeleted)
            .OrderByDescending(x => x.Syllabus.IsActive)
            .ThenBy(x => x.Syllabus.Code)
            .ThenByDescending(x => x.Syllabus.Version)
            .Select(x => new PackageSyllabusDto
            {
                MappingId = x.Id,
                SyllabusId = x.SyllabusId,
                ProgramId = x.Syllabus.ProgramId,
                ProgramName = x.Syllabus.Program.Name,
                LevelId = x.Syllabus.LevelId,
                LevelName = x.Syllabus.Level.Name,
                Code = x.Syllabus.Code,
                Version = x.Syllabus.Version,
                Title = x.Syllabus.Title,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new GetPackageSyllabusesResponse
        {
            TuitionPlanId = tuitionPlan.Id,
            TuitionPlanName = tuitionPlan.Name,
            Syllabuses = syllabuses
        };
    }
}
