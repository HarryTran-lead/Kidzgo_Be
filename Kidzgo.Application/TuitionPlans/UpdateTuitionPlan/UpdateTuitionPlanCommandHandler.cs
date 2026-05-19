using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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

        Domain.Programs.Module? module = null;
        if (command.ModuleId.HasValue)
        {
            module = await context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == command.ModuleId.Value, cancellationToken);
            if (module is null)
            {
                return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.ModuleNotFound);
            }

            if (module.LevelId != command.LevelId)
            {
                return Result.Failure<UpdateTuitionPlanResponse>(TuitionPlanErrors.ModuleLevelMismatch);
            }
        }

        if (command.LearningTicketTypeId.HasValue)
        {
            var ticketTypeExists = await context.LearningTicketTypes
                .AnyAsync(
                    x => x.Id == command.LearningTicketTypeId.Value && x.IsActive,
                    cancellationToken);

            if (!ticketTypeExists)
            {
                return Result.Failure<UpdateTuitionPlanResponse>(
                    Error.Validation(
                        "TuitionPlan.LearningTicketTypeNotFound",
                        $"Learning ticket type '{command.LearningTicketTypeId.Value}' was not found or inactive."));
            }
        }

        // Calculate UnitPriceSession automatically from TuitionAmount / TotalSessions
        decimal unitPriceSession = command.TotalSessions > 0
            ? Math.Round(command.TuitionAmount / command.TotalSessions, 2)
            : 0;

        tuitionPlan.ProgramId = command.ProgramId;
        tuitionPlan.LevelId = command.LevelId;
        tuitionPlan.ModuleId = command.ModuleId;
        tuitionPlan.Name = command.Name;
        tuitionPlan.TotalSessions = command.TotalSessions;
        tuitionPlan.TuitionAmount = command.TuitionAmount;
        tuitionPlan.UnitPriceSession = unitPriceSession;
        tuitionPlan.Currency = command.Currency;
        tuitionPlan.LearningTicketTypeId = command.LearningTicketTypeId;
        tuitionPlan.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        // Query again with includes to get related data for response
        var updatedTuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Include(t => t.Module)
            .Include(t => t.LearningTicketType)
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        return new UpdateTuitionPlanResponse
        {
            Id = updatedTuitionPlan!.Id,
            ProgramId = updatedTuitionPlan.ProgramId,
            LevelId = updatedTuitionPlan.LevelId,
            LevelName = updatedTuitionPlan.Level.Name,
            ModuleId = updatedTuitionPlan.ModuleId,
            ModuleName = updatedTuitionPlan.Module?.Name,
            LearningTicketTypeId = updatedTuitionPlan.LearningTicketTypeId,
            LearningTicketTypeCode = updatedTuitionPlan.LearningTicketType?.Code,
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
