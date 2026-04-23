using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.ImportActiveRegistration;

public sealed class ImportActiveRegistrationCommandHandler(
    IDbContext context
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

        var tuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(
                tp => tp.Id == command.TuitionPlanId &&
                      tp.ProgramId == command.ProgramId &&
                      tp.IsActive &&
                      !tp.IsDeleted &&
                      (!tp.BranchId.HasValue || tp.BranchId == command.BranchId),
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

        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            BranchId = command.BranchId,
            ProgramId = command.ProgramId,
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

        context.Registrations.Add(registration);
        await context.SaveChangesAsync(cancellationToken);

        return new ImportActiveRegistrationResponse
        {
            Id = registration.Id,
            StudentProfileId = registration.StudentProfileId,
            BranchId = registration.BranchId,
            ProgramId = registration.ProgramId,
            ProgramName = program.Name,
            TuitionPlanId = registration.TuitionPlanId,
            TuitionPlanName = tuitionPlan.Name,
            RegistrationDate = registration.RegistrationDate,
            ExpectedStartDate = registration.ExpectedStartDate,
            ActualStartDate = registration.ActualStartDate!.Value,
            PreferredSchedule = registration.PreferredSchedule,
            Note = registration.Note,
            Status = registration.Status.ToString(),
            TotalSessions = registration.TotalSessions,
            UsedSessions = registration.UsedSessions,
            RemainingSessions = registration.RemainingSessions,
            CreatedAt = registration.CreatedAt
        };
    }
}
