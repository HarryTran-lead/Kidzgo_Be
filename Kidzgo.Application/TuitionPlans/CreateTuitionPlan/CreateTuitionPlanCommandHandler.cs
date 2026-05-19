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
    public async Task<Result<CreateTuitionPlanResponse>> Handle(CreateTuitionPlanCommand command, CancellationToken cancellationToken)
    {
        // Check if program exists
        bool programExists = await context.Programs
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

        Domain.Programs.Module? module = null;
        if (command.ModuleId.HasValue)
        {
            module = await context.Modules
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == command.ModuleId.Value, cancellationToken);
            if (module is null)
            {
                return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.ModuleNotFound);
            }

            if (module.LevelId != command.LevelId)
            {
                return Result.Failure<CreateTuitionPlanResponse>(TuitionPlanErrors.ModuleLevelMismatch);
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
                return Result.Failure<CreateTuitionPlanResponse>(
                    Error.Validation(
                        "TuitionPlan.LearningTicketTypeNotFound",
                        $"Learning ticket type '{command.LearningTicketTypeId.Value}' was not found or inactive."));
            }
        }

        // Calculate UnitPriceSession automatically from TuitionAmount / TotalSessions
        decimal unitPriceSession = command.TotalSessions > 0
            ? Math.Round(command.TuitionAmount / command.TotalSessions, 2)
            : 0;

        var tuitionPlan = new TuitionPlan
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            ModuleId = command.ModuleId,
            Name = command.Name,
            TotalSessions = command.TotalSessions,
            TuitionAmount = command.TuitionAmount,
            UnitPriceSession = unitPriceSession,
            Currency = command.Currency,
            LearningTicketTypeId = command.LearningTicketTypeId,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = VietnamTime.UtcNow(),
            UpdatedAt = VietnamTime.UtcNow()
        };

        context.TuitionPlans.Add(tuitionPlan);
        await context.SaveChangesAsync(cancellationToken);

        // Query again with includes to get related data for response
        var createdTuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Include(t => t.Module)
            .Include(t => t.LearningTicketType)
            .FirstOrDefaultAsync(t => t.Id == tuitionPlan.Id, cancellationToken);

        return new CreateTuitionPlanResponse
        {
            Id = createdTuitionPlan!.Id,
            ProgramId = createdTuitionPlan.ProgramId,
            LevelId = createdTuitionPlan.LevelId,
            LevelName = createdTuitionPlan.Level.Name,
            ModuleId = createdTuitionPlan.ModuleId,
            ModuleName = createdTuitionPlan.Module?.Name,
            LearningTicketTypeId = createdTuitionPlan.LearningTicketTypeId,
            LearningTicketTypeCode = createdTuitionPlan.LearningTicketType?.Code,
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
