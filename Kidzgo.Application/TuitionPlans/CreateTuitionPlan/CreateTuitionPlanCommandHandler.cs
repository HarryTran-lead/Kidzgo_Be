using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TuitionPlans.Shared;
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

        var selectedModuleIds = TuitionPlanSelectionSupport.NormalizeRequestedModuleIds(
            command.ModuleIds,
            command.ModuleId);
        var selectionResult = await TuitionPlanSelectionSupport.ValidateSelectionAsync(
            context,
            command.ProgramId,
            command.LevelId,
            command.SyllabusId,
            selectedModuleIds,
            command.TotalSessions,
            cancellationToken);
        if (selectionResult.IsFailure)
        {
            return Result.Failure<CreateTuitionPlanResponse>(selectionResult.Error);
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

        var now = VietnamTime.UtcNow();
        var resolvedTotalSessions = selectionResult.Value.ResolvedTotalSessions;

        // Calculate UnitPriceSession automatically from TuitionAmount / TotalSessions
        decimal unitPriceSession = resolvedTotalSessions > 0
            ? Math.Round(command.TuitionAmount / resolvedTotalSessions, 2)
            : 0;

        var tuitionPlan = new TuitionPlan
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            ModuleId = selectionResult.Value.StartModuleId,
            Name = command.Name,
            TotalSessions = resolvedTotalSessions,
            TuitionAmount = command.TuitionAmount,
            UnitPriceSession = unitPriceSession,
            Currency = command.Currency,
            LearningTicketTypeId = command.LearningTicketTypeId,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        TuitionPlanSelectionSupport.ReplaceSelectedModules(
            tuitionPlan,
            selectionResult.Value.OrderedSelectedModules,
            now);

        if (selectionResult.Value.Syllabus is not null)
        {
            tuitionPlan.CurriculumMappings.Add(new PackageCurriculumMapping
            {
                Id = Guid.NewGuid(),
                TuitionPlanId = tuitionPlan.Id,
                SyllabusId = selectionResult.Value.Syllabus.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        context.TuitionPlans.Add(tuitionPlan);
        await context.SaveChangesAsync(cancellationToken);

        // Query again with includes to get related data for response
        var createdTuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Include(t => t.Module)
            .Include(t => t.SelectedModules)
                .ThenInclude(x => x.Module)
            .Include(t => t.LearningTicketType)
            .Include(t => t.CurriculumMappings)
                .ThenInclude(x => x.Syllabus)
            .FirstOrDefaultAsync(t => t.Id == tuitionPlan.Id, cancellationToken);

        var syllabus = TuitionPlanSelectionSupport.ResolveActiveSyllabus(createdTuitionPlan!);
        var modules = TuitionPlanSelectionSupport.ResolveModules(createdTuitionPlan!);

        return new CreateTuitionPlanResponse
        {
            Id = createdTuitionPlan!.Id,
            ProgramId = createdTuitionPlan.ProgramId,
            LevelId = createdTuitionPlan.LevelId,
            LevelName = createdTuitionPlan.Level.Name,
            SyllabusId = syllabus?.SyllabusId,
            SyllabusCode = syllabus?.SyllabusCode,
            SyllabusVersion = syllabus?.SyllabusVersion,
            SyllabusTitle = syllabus?.SyllabusTitle,
            ModuleId = TuitionPlanSelectionSupport.ResolvePrimaryModuleId(createdTuitionPlan),
            ModuleName = TuitionPlanSelectionSupport.ResolvePrimaryModuleName(createdTuitionPlan),
            ModuleIds = TuitionPlanSelectionSupport.ResolveModuleIds(createdTuitionPlan),
            Modules = modules,
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
