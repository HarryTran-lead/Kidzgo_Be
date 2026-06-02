using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Application.Students.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.ImportActiveRegistration;

public sealed class ImportActiveRegistrationCommandHandler(
    IDbContext context,
    TicketGrantService ticketGrantService
) : ICommandHandler<ImportActiveRegistrationCommand, ImportActiveRegistrationResponse>
{
    public async Task<Result<ImportActiveRegistrationResponse>> Handle(
        ImportActiveRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var student = await context.Profiles
            .FirstOrDefaultAsync(
                p => p.Id == command.StudentProfileId &&
                     p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student,
                cancellationToken);

        if (student == null)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                RegistrationErrors.StudentNotFound(command.StudentProfileId));
        }

        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                RegistrationErrors.BranchNotFound(command.BranchId));
        }

        var branchAccessResult = await StudentBranchAccessHelper.ValidateBranchAccessAsync(
            context,
            command.StudentProfileId,
            command.BranchId,
            allowCrossBranchEnrollment: false,
            cancellationToken);
        if (branchAccessResult.IsFailure)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(branchAccessResult.Error);
        }

        var program = await context.Programs
            .FirstOrDefaultAsync(
                p => p.Id == command.ProgramId && p.IsActive && !p.IsDeleted,
                cancellationToken);

        if (program == null)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                RegistrationErrors.ProgramNotFound(command.ProgramId));
        }

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            command.BranchId,
            command.ProgramId,
            cancellationToken);

        if (!programAssignedToBranch)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                RegistrationErrors.ProgramNotAvailableInBranch(command.ProgramId, command.BranchId));
        }

        var level = await context.Levels
            .FirstOrDefaultAsync(
                l => l.Id == command.LevelId &&
                     l.ProgramId == command.ProgramId &&
                     l.IsActive,
                cancellationToken);

        if (level is null)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                Error.Validation(
                    "Registration.LevelNotFoundInProgram",
                    $"Level '{command.LevelId}' was not found, inactive, or does not belong to the selected program."));
        }

        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(
                tp => tp.Id == command.TuitionPlanId &&
                      tp.ProgramId == command.ProgramId &&
                      tp.LevelId == command.LevelId &&
                      tp.IsActive &&
                      !tp.IsDeleted,
                cancellationToken);

        if (tuitionPlan == null)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                RegistrationErrors.TuitionPlanNotFound(command.TuitionPlanId));
        }

        var activeRegistrationExists = await context.Registrations
            .AnyAsync(
                r => r.StudentProfileId == command.StudentProfileId &&
                     r.Status != RegistrationStatus.Completed &&
                     r.Status != RegistrationStatus.Cancelled &&
                     (r.ProgramId == command.ProgramId || r.SecondaryProgramId == command.ProgramId),
                cancellationToken);

        if (activeRegistrationExists)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                RegistrationErrors.AlreadyExists(command.StudentProfileId, command.ProgramId));
        }

        var now = VietnamTime.UtcNow();
        var actualStartDate = VietnamTime.NormalizeToUtc(command.ActualStartDate);
        var expectedStartDate = VietnamTime.NormalizeToUtc(command.ExpectedStartDate);

        if (actualStartDate > now)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                Error.Validation(
                    "Registration.ActualStartDateInFuture",
                    "ActualStartDate cannot be later than the current time."));
        }

        var totalImportedSessions = command.UsedSessions + command.RemainingSessions;
        if (totalImportedSessions != tuitionPlan.TotalSessions)
        {
            return Result.Failure<ImportActiveRegistrationResponse>(
                Error.Validation(
                    "Registration.ImportSessionCountMismatch",
                    $"Imported used/remaining sessions must add up to the tuition plan total sessions ({tuitionPlan.TotalSessions})."));
        }

        var operationType = await RegistrationDiscountPricingHelper.ResolveInitialOrRenewalOperationTypeAsync(
            context,
            command.StudentProfileId,
            excludeRegistrationId: null,
            cancellationToken);
        var pricing = await RegistrationDiscountPricingHelper.ResolveAsync(
            context,
            command.BranchId,
            command.ProgramId,
            command.LevelId,
            tuitionPlan.Id,
            operationType,
            now,
            tuitionPlan.TuitionAmount,
            carryOverCreditAmount: 0m,
            cancellationToken);

        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            TuitionPlanId = command.TuitionPlanId,
            RegistrationDate = now,
            ExpectedStartDate = expectedStartDate,
            ActualStartDate = actualStartDate,
            PreferredSchedule = command.PreferredSchedule,
            Note = command.Note,
            Status = RegistrationStatus.New,
            TotalSessions = tuitionPlan.TotalSessions,
            UsedSessions = command.UsedSessions,
            RemainingSessions = command.RemainingSessions,
            CreatedAt = now,
            UpdatedAt = now
        };
        RegistrationDiscountPricingHelper.ApplyToRegistration(registration, pricing);

        context.Registrations.Add(registration);
        await ticketGrantService.GrantTicketsAsync(
            registration.StudentProfileId,
            registration.Id,
            tuitionPlan.TotalSessions,
            tuitionPlan.LearningTicketTypeId,
            $"Import {tuitionPlan.Name}",
            LearningTicketSource.Import,
            createdByUserId: null,
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        if (command.UsedSessions > 0)
        {
            var consumedAt = now;
            var importedItems = await context.LearningTicketItems
                .Where(x => x.RegistrationId == registration.Id && x.Status == LearningTicketItemStatus.Available)
                .OrderBy(x => x.CreatedAt)
                .Take(command.UsedSessions)
                .ToListAsync(cancellationToken);

            foreach (var item in importedItems)
            {
                item.Status = LearningTicketItemStatus.Consumed;
                item.ConsumedAt = consumedAt;
            }

            context.LearningTicketLedgers.Add(new LearningTicketLedger
            {
                Id = Guid.NewGuid(),
                StudentProfileId = registration.StudentProfileId,
                RegistrationId = registration.Id,
                TransactionType = LearningTicketTransactionType.Consume,
                Quantity = -command.UsedSessions,
                Reason = "Import used sessions",
                CreatedAt = consumedAt
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ImportActiveRegistrationResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            ProgramName = program.Name,
            LevelId = registration.LevelId,
            LevelName = level.Name,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = tuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            ActualStartDate = registration.ActualStartDate!.Value,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            StudentHomeBranchId = branchAccessResult.Value.State.HomeBranchId,
            StudentActiveBranchId = branchAccessResult.Value.State.ActiveBranchId,
            IsCrossBranchRegistration = branchAccessResult.Value.IsCrossBranch,
            OperationType = registration.OperationType?.ToString(),
            TotalSessions = registration.TotalSessions,
            UsedSessions = registration.UsedSessions,
            RemainingSessions = registration.RemainingSessions,
            DiscountCampaignId = registration.DiscountCampaignId,
            DiscountCampaignName = registration.DiscountCampaignName,
            DiscountType = registration.DiscountType?.ToString(),
            DiscountValue = registration.DiscountValue,
            OriginalTuitionAmount = registration.OriginalTuitionAmount ?? tuitionPlan.TuitionAmount,
            DiscountAmount = registration.DiscountAmount ?? 0m,
            CarryOverCreditAmount = registration.CarryOverCreditAmount ?? 0m,
            FinalTuitionAmount = registration.FinalTuitionAmount ?? tuitionPlan.TuitionAmount,
            CreatedAt = registration.CreatedAt
        };
    }
}
