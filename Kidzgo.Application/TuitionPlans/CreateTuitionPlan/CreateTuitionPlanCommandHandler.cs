using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.CreateTuitionPlan;

public sealed class CreateTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<CreateTuitionPlanCommand, CreateTuitionPlanResponse>
{
    public async Task<Result<CreateTuitionPlanResponse>> Handle(CreateTuitionPlanCommand command, CancellationToken cancellationToken)
    {
        // Check if program exists
        bool programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.ProgramNotFound);
        }

        // Check if branch exists (if provided)
        if (command.BranchId.HasValue)
        {
            bool branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchId.Value && b.IsActive, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.BranchNotFound);
            }

            var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
                context,
                command.BranchId.Value,
                command.ProgramId,
                cancellationToken);

            if (!programAssignedToBranch)
            {
                return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.ProgramNotAvailableInBranch);
            }
        }

        // Calculate UnitPriceSession automatically from TuitionAmount / TotalSessions
        decimal unitPriceSession = command.TotalSessions > 0
            ? Math.Round(command.TuitionAmount / command.TotalSessions, 2)
            : 0;

        var tuitionPlan = new TuitionPlan
        {
            Id = Guid.NewGuid(),
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            Name = command.Name,
            TotalSessions = command.TotalSessions,
            TuitionAmount = command.TuitionAmount,
            UnitPriceSession = unitPriceSession,
            Currency = command.Currency,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = VietnamTime.UtcNow(),
            UpdatedAt = VietnamTime.UtcNow()
        };

        context.TuitionPlans.Add(tuitionPlan);
        await context.SaveChangesAsync(cancellationToken);

        // Query again with includes to get related data for response
        var createdTuitionPlan = await context.TuitionPlans
            .Include(t => t.Branch)
            .Include(t => t.Program)
            .FirstOrDefaultAsync(t => t.Id == tuitionPlan.Id, cancellationToken);

        return new CreateTuitionPlanResponse
        {
            Id = createdTuitionPlan!.Id,
            BranchId = createdTuitionPlan.BranchId,
            BranchName = createdTuitionPlan.Branch?.Name,
            ProgramId = createdTuitionPlan.ProgramId,
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
