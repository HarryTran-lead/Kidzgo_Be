using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanUnits.GetLessonPlanUnits;

public sealed class GetLessonPlanUnitsQueryHandler(IDbContext context)
    : IQueryHandler<GetLessonPlanUnitsQuery, GetLessonPlanUnitsResponse>
{
    public async Task<Result<GetLessonPlanUnitsResponse>> Handle(
        GetLessonPlanUnitsQuery query,
        CancellationToken cancellationToken)
    {
        var moduleExists = await context.Modules
            .AnyAsync(x => x.Id == query.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            return Result.Failure<GetLessonPlanUnitsResponse>(
                LessonPlanUnitErrors.ModuleNotFound(query.ModuleId));
        }

        var items = await context.LessonPlanUnits
            .AsNoTracking()
            .Where(x => x.ModuleId == query.ModuleId)
            .OrderBy(x => x.OrderIndex)
            .ThenBy(x => x.Name)
            .Select(x => new LessonPlanUnitDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                Name = x.Name,
                OrderIndex = x.OrderIndex,
                LessonCount = x.LessonPlanTemplates.Count(t => !t.IsDeleted),
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return new GetLessonPlanUnitsResponse { Items = items };
    }
}
