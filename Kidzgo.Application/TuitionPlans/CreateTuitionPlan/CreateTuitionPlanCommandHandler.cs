using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.CreateTuitionPlan;

public sealed class CreateTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<CreateTuitionPlanCommand, CreateTuitionPlanResponse>
{
    public async Task<Result<CreateTuitionPlanResponse>> Handle(
        CreateTuitionPlanCommand command,
        CancellationToken cancellationToken)
    {
        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);
        if (!programExists)
        {
            return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.ProgramNotFound);
        }

        var level = await context.Levels
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.LevelId, cancellationToken);
        if (level is null)
        {
            return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.LevelNotFound);
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.LevelProgramMismatch);
        }

        var now = VietnamTime.UtcNow();
        var unitPriceSession = Math.Round(command.TuitionAmount / command.TotalSessions, 2);

        var tuitionPlan = new TuitionPlan
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            Name = command.Name,
            TotalSessions = command.TotalSessions,
            TuitionAmount = command.TuitionAmount,
            UnitPriceSession = unitPriceSession,
            Currency = command.Currency,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.TuitionPlans.Add(tuitionPlan);
        await context.SaveChangesAsync(cancellationToken);

        var createdTuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .FirstAsync(t => t.Id == tuitionPlan.Id, cancellationToken);

        return new CreateTuitionPlanResponse
        {
            Id = createdTuitionPlan.Id,
            ProgramId = createdTuitionPlan.ProgramId,
            LevelId = createdTuitionPlan.LevelId,
            LevelName = createdTuitionPlan.Level.Name,
            ProgramName = createdTuitionPlan.Program.Name,
            Name = createdTuitionPlan.Name,
            TotalSessions = createdTuitionPlan.TotalSessions,
            TuitionAmount = createdTuitionPlan.TuitionAmount,
            UnitPriceSession = createdTuitionPlan.UnitPriceSession,
            Currency = createdTuitionPlan.Currency,
            IsActive = createdTuitionPlan.IsActive,
            CreatedAt = createdTuitionPlan.CreatedAt,
            UpdatedAt = createdTuitionPlan.UpdatedAt
        };
    }
}
