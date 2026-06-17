using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.UpdateTuitionPlan;

public sealed class UpdateTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateTuitionPlanCommand, UpdateTuitionPlanResponse>
{
    public async Task<Result<UpdateTuitionPlanResponse>> Handle(
        UpdateTuitionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.NotFound(command.Id));
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);
        if (!programExists)
        {
            return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.ProgramNotFound);
        }

        var level = await context.Levels
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.LevelId, cancellationToken);
        if (level is null)
        {
            return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.LevelNotFound);
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.LevelProgramMismatch);
        }

        var now = VietnamTime.UtcNow();
        var unitPriceSession = Math.Round(command.TuitionAmount / command.TotalSessions, 2);

        tuitionPlan.ProgramId = command.ProgramId;
        tuitionPlan.LevelId = command.LevelId;
        tuitionPlan.Name = command.Name;
        tuitionPlan.TotalSessions = command.TotalSessions;
        tuitionPlan.TuitionAmount = command.TuitionAmount;
        tuitionPlan.UnitPriceSession = unitPriceSession;
        tuitionPlan.Currency = command.Currency;
        tuitionPlan.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        var updatedTuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .FirstAsync(t => t.Id == command.Id, cancellationToken);

        return new UpdateTuitionPlanResponse
        {
            Id = updatedTuitionPlan.Id,
            ProgramId = updatedTuitionPlan.ProgramId,
            LevelId = updatedTuitionPlan.LevelId,
            LevelName = updatedTuitionPlan.Level.Name,
            ProgramName = updatedTuitionPlan.Program.Name,
            Name = updatedTuitionPlan.Name,
            TotalSessions = updatedTuitionPlan.TotalSessions,
            TuitionAmount = updatedTuitionPlan.TuitionAmount,
            UnitPriceSession = updatedTuitionPlan.UnitPriceSession,
            Currency = updatedTuitionPlan.Currency,
            IsActive = updatedTuitionPlan.IsActive,
            CreatedAt = updatedTuitionPlan.CreatedAt,
            UpdatedAt = updatedTuitionPlan.UpdatedAt
        };
    }
}
