using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.RemedialPlans.GetRemedialPlansByStudent;

public sealed class GetRemedialPlansByStudentQueryHandler(IDbContext context)
    : IQueryHandler<GetRemedialPlansByStudentQuery, GetRemedialPlansByStudentResponse>
{
    public async Task<Result<GetRemedialPlansByStudentResponse>> Handle(GetRemedialPlansByStudentQuery query, CancellationToken cancellationToken)
    {
        var items = await context.RemedialPlans
            .AsNoTracking()
            .Include(x => x.Module)
            .Where(x => x.StudentProfileId == query.StudentProfileId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new RemedialPlanDto
            {
                Id = x.Id,
                StudentProfileId = x.StudentProfileId,
                ModuleId = x.ModuleId,
                ModuleCode = x.Module.Code,
                WeakSkills = x.WeakSkills,
                RecommendedSessionCount = x.RecommendedSessionCount,
                Notes = x.Notes,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetRemedialPlansByStudentResponse { Items = items });
    }
}
