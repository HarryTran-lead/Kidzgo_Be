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

        tuitionPlan.ModuleId = selectionResult.Value.OrderedSelectedModules.Count == 0
            ? null
            : selectionResult.Value.OrderedSelectedModules[0].Id;

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await ReplaceSelectedModulesAsync(
                tuitionPlan.Id,
                selectionResult.Value.OrderedSelectedModules,
                now,
                cancellationToken);
            await SyncCurriculumMappingsAsync(
                tuitionPlan.Id,
                selectionResult.Value.Syllabus?.Id,
                now,
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<UpdateTuitionPlanResponse>(
                TuitionPlanErrors.UpdateConflict(DescribeConflictedEntries(ex)));
        }

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

    private async Task ReplaceSelectedModulesAsync(
        Guid tuitionPlanId,
        IReadOnlyList<Module> orderedSelectedModules,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await context.TuitionPlanModuleSelections
            .Where(x => x.TuitionPlanId == tuitionPlanId)
            .ExecuteDeleteAsync(cancellationToken);

        if (orderedSelectedModules.Count == 0)
        {
            return;
        }

        var newSelections = orderedSelectedModules
            .Select((module, index) => new TuitionPlanModuleSelection
            {
                Id = Guid.NewGuid(),
                TuitionPlanId = tuitionPlanId,
                ModuleId = module.Id,
                OrderIndex = index,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();

        context.TuitionPlanModuleSelections.AddRange(newSelections);
    }

    private async Task SyncCurriculumMappingsAsync(
        Guid tuitionPlanId,
        Guid? syllabusId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await context.PackageCurriculumMappings
            .Where(x =>
                x.TuitionPlanId == tuitionPlanId &&
                x.IsActive &&
                (!syllabusId.HasValue || x.SyllabusId != syllabusId.Value))
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsActive, false)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        if (!syllabusId.HasValue)
        {
            return;
        }

        var activatedCount = await context.PackageCurriculumMappings
            .Where(x => x.TuitionPlanId == tuitionPlanId && x.SyllabusId == syllabusId.Value)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(x => x.IsActive, true)
                    .SetProperty(x => x.UpdatedAt, now),
                cancellationToken);

        if (activatedCount == 0)
        {
            context.PackageCurriculumMappings.Add(new PackageCurriculumMapping
            {
                Id = Guid.NewGuid(),
                TuitionPlanId = tuitionPlanId,
                SyllabusId = syllabusId.Value,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }
    }

    private static string DescribeConflictedEntries(DbUpdateConcurrencyException exception)
    {
        return string.Join(
            ", ",
            exception.Entries
                .Select(entry => entry.Metadata.ClrType.Name)
                .Distinct()
                .OrderBy(name => name));
    }
}
