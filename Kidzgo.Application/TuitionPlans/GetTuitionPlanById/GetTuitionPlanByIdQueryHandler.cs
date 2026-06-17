using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.GetTuitionPlanById;

public sealed class GetTuitionPlanByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetTuitionPlanByIdQuery, GetTuitionPlanByIdResponse>
{
    public async Task<Result<GetTuitionPlanByIdResponse>> Handle(
        GetTuitionPlanByIdQuery query,
        CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Where(t => t.Id == query.Id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<GetTuitionPlanByIdResponse>(TuitionPlanErrors.NotFound(query.Id));
        }

        return new GetTuitionPlanByIdResponse
        {
            Id = tuitionPlan.Id,
            ProgramId = tuitionPlan.ProgramId,
            LevelId = tuitionPlan.LevelId,
            LevelName = tuitionPlan.Level.Name,
            ProgramName = tuitionPlan.Program.Name,
            Name = tuitionPlan.Name,
            TotalSessions = tuitionPlan.TotalSessions,
            TuitionAmount = tuitionPlan.TuitionAmount,
            UnitPriceSession = tuitionPlan.UnitPriceSession,
            Currency = tuitionPlan.Currency,
            IsActive = tuitionPlan.IsActive,
            CreatedAt = tuitionPlan.CreatedAt,
            UpdatedAt = tuitionPlan.UpdatedAt
        };
    }
}
