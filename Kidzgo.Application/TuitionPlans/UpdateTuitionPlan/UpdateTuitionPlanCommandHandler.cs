using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.UpdateTuitionPlan;

public sealed class UpdateTuitionPlanCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateTuitionPlanCommand, UpdateTuitionPlanResponse>
{
    public async Task<Result<UpdateTuitionPlanResponse>> Handle(UpdateTuitionPlanCommand command, CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(t => t.Id == command.Id && !t.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.NotFound(command.Id));
        }

        // Check if program exists
        bool programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.ProgramNotFound);
        }

        // Check if branch exists (if provided)
        if (command.BranchId.HasValue)
        {
            bool branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchId.Value && b.IsActive, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.BranchNotFound);
            }

            var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
                context,
                command.BranchId.Value,
                command.ProgramId,
                cancellationToken);

            if (!programAssignedToBranch)
            {
                return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.ProgramNotAvailableInBranch);
            }
        }

        // Calculate UnitPriceSession automatically from TuitionAmount / TotalSessions
        decimal unitPriceSession = command.TotalSessions > 0
            ? Math.Round(command.TuitionAmount / command.TotalSessions, 2)
            : 0;

        tuitionPlan.BranchId = command.BranchId;
        tuitionPlan.ProgramId = command.ProgramId;
        tuitionPlan.Name = command.Name;
        tuitionPlan.TotalSessions = command.TotalSessions;
        tuitionPlan.TuitionAmount = command.TuitionAmount;
        tuitionPlan.UnitPriceSession = unitPriceSession;
        tuitionPlan.Currency = command.Currency;
        tuitionPlan.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        // Query again with includes to get related data for response
        var updatedTuitionPlan = await context.TuitionPlans
            .Include(t => t.Branch)
            .Include(t => t.Program)
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        return new UpdateTuitionPlanResponse
        {
            Id = updatedTuitionPlan!.Id,
            BranchId = updatedTuitionPlan.BranchId,
            BranchName = updatedTuitionPlan.Branch?.Name,
            ProgramId = updatedTuitionPlan.ProgramId,
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
