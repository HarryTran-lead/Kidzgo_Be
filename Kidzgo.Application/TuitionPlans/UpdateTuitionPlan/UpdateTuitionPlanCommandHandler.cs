using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.TuitionPlans.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
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
            .Include(t => t.SelectedModules)
            .Include(t => t.CurriculumMappings)
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

        var selectedModuleIds = TuitionPlanSelectionSupport.NormalizeRequestedModuleIds(
            command.ModuleIds);
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
            return Result.Failure<UpdateTuitionPlanResponse>(selectionResult.Error);
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

        var now = VietnamTime.UtcNow();
        var resolvedTotalSessions = selectionResult.Value.ResolvedTotalSessions;

        // Calculate UnitPriceSession automatically from TuitionAmount / TotalSessions
        decimal unitPriceSession = resolvedTotalSessions > 0
            ? Math.Round(command.TuitionAmount / resolvedTotalSessions, 2)
            : 0;

        tuitionPlan.ProgramId = command.ProgramId;
        tuitionPlan.LevelId = command.LevelId;
        tuitionPlan.Name = command.Name;
        tuitionPlan.TotalSessions = resolvedTotalSessions;
        tuitionPlan.TuitionAmount = command.TuitionAmount;
        tuitionPlan.UnitPriceSession = unitPriceSession;
        tuitionPlan.Currency = command.Currency;
        tuitionPlan.LearningTicketTypeId = command.LearningTicketTypeId;
        tuitionPlan.UpdatedAt = now;

        TuitionPlanSelectionSupport.ReplaceSelectedModules(
            tuitionPlan,
            selectionResult.Value.OrderedSelectedModules,
            now);
        SyncCurriculumMappings(tuitionPlan, selectionResult.Value.Syllabus, now);

        await context.SaveChangesAsync(cancellationToken);

        // Query again with includes to get related data for response
        var updatedTuitionPlan = await context.TuitionPlans
            .Include(t => t.Program)
            .Include(t => t.Level)
            .Include(t => t.Module)
            .Include(t => t.SelectedModules)
                .ThenInclude(x => x.Module)
            .Include(t => t.LearningTicketType)
            .Include(t => t.CurriculumMappings)
                .ThenInclude(x => x.Syllabus)
            .FirstOrDefaultAsync(t => t.Id == command.Id, cancellationToken);

        var syllabus = TuitionPlanSelectionSupport.ResolveActiveSyllabus(updatedTuitionPlan!);
        var modules = TuitionPlanSelectionSupport.ResolveModules(updatedTuitionPlan!);

        return new UpdateTuitionPlanResponse
        {
            Id = updatedTuitionPlan!.Id,
            ProgramId = updatedTuitionPlan.ProgramId,
            LevelId = updatedTuitionPlan.LevelId,
            LevelName = updatedTuitionPlan.Level.Name,
            SyllabusId = syllabus?.SyllabusId,
            SyllabusCode = syllabus?.SyllabusCode,
            SyllabusVersion = syllabus?.SyllabusVersion,
            SyllabusTitle = syllabus?.SyllabusTitle,
            ModuleIds = TuitionPlanSelectionSupport.ResolveModuleIds(updatedTuitionPlan),
            Modules = modules,
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

    private static void SyncCurriculumMappings(
        TuitionPlan tuitionPlan,
        Domain.LessonPlans.Syllabus? syllabus,
        DateTime now)
    {
        foreach (var mapping in tuitionPlan.CurriculumMappings)
        {
            mapping.IsActive = false;
            mapping.UpdatedAt = now;
        }

        if (syllabus is null)
        {
            return;
        }

        var existingMapping = tuitionPlan.CurriculumMappings
            .FirstOrDefault(x => x.SyllabusId == syllabus.Id);

        if (existingMapping is null)
        {
            tuitionPlan.CurriculumMappings.Add(new PackageCurriculumMapping
            {
                Id = Guid.NewGuid(),
                TuitionPlanId = tuitionPlan.Id,
                SyllabusId = syllabus.Id,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });

            return;
        }

        existingMapping.IsActive = true;
        existingMapping.UpdatedAt = now;
    }
}
