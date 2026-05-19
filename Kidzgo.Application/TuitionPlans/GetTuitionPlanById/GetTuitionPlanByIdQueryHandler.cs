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
    public async Task<Result<GetTuitionPlanByIdResponse>> Handle(GetTuitionPlanByIdQuery query, CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Include(t => t.Module)
            .Where(t => t.Id == query.Id && !t.IsDeleted)
            .Select(t => new GetTuitionPlanByIdResponse
            {
                Id = t.Id,
                ProgramId = t.ProgramId,
                LevelId = t.LevelId,
                LevelName = t.Level.Name,
                ModuleId = t.ModuleId,
                ModuleName = t.Module != null ? t.Module.Name : null,
                LearningTicketTypeId = t.LearningTicketTypeId,
                LearningTicketTypeCode = t.LearningTicketType != null ? t.LearningTicketType.Code : null,
                ProgramName = t.Program.Name,
                Name = t.Name,
                TotalSessions = t.TotalSessions,
                TuitionAmount = t.TuitionAmount,
                UnitPriceSession = t.UnitPriceSession,
                Currency = t.Currency,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<GetTuitionPlanByIdResponse>(TuitionPlanErrors.NotFound(query.Id));
        }

        return tuitionPlan;
    }
}

