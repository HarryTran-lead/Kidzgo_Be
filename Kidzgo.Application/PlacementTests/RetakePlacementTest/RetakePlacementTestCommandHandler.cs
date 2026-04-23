using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PlacementTests;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.RetakePlacementTest;

public sealed class RetakePlacementTestCommandHandler(
    IDbContext context
) : ICommandHandler<RetakePlacementTestCommand, RetakePlacementTestResponse>
{
    public async Task<Result<RetakePlacementTestResponse>> Handle(
        RetakePlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        var originalPlacementTest = await context.PlacementTests
            .Include(pt => pt.Lead)
            .FirstOrDefaultAsync(pt => pt.Id == command.OriginalPlacementTestId, cancellationToken);

        if (originalPlacementTest is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.NotFound(command.OriginalPlacementTestId));
        }

        var student = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId
                && p.ProfileType == Kidzgo.Domain.Users.ProfileType.Student, cancellationToken);

        if (student is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
        }

        if (originalPlacementTest.StudentProfileId != command.StudentProfileId)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
        }

        var existingRetake = await context.PlacementTests
            .AnyAsync(pt =>
                pt.StudentProfileId == command.StudentProfileId &&
                pt.Id != command.OriginalPlacementTestId &&
                (pt.Status == PlacementTestStatus.Scheduled ||
                 pt.Status == PlacementTestStatus.Completed),
                cancellationToken);

        if (existingRetake)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                PlacementTestErrors.RetakeAlreadyScheduled(command.StudentProfileId));
        }

        var activeRegistration = await context.Registrations
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r =>
                r.StudentProfileId == command.StudentProfileId &&
                r.Status != RegistrationStatus.Completed &&
                r.Status != RegistrationStatus.Cancelled,
                cancellationToken);

        var originalProgramName = activeRegistration?.Program?.Name;
        var originalTuitionPlanName = activeRegistration?.TuitionPlan?.Name;
        var originalRemainingSessions = activeRegistration?.RemainingSessions ?? 0;

        var branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.BranchNotFound(command.BranchId));
        }

        var duration = PlacementTestScheduleAvailability.NormalizeDuration(command.DurationMinutes);
        if (duration <= 0)
        {
            return Result.Failure<RetakePlacementTestResponse>(PlacementTestErrors.InvalidDuration);
        }

        var scheduledAtUtc = command.ScheduledAt.HasValue
            ? VietnamTime.NormalizeToUtc(command.ScheduledAt.Value)
            : (DateTime?)null;

        if (scheduledAtUtc.HasValue)
        {
            if (!command.InvigilatorUserId.HasValue)
            {
                return Result.Failure<RetakePlacementTestResponse>(PlacementTestErrors.InvigilatorRequired);
            }

            if (!command.RoomId.HasValue)
            {
                return Result.Failure<RetakePlacementTestResponse>(PlacementTestErrors.RoomRequired);
            }

            var availability = await PlacementTestScheduleAvailability.EnsureScheduleAvailableAsync(
                context,
                command.InvigilatorUserId.Value,
                command.RoomId.Value,
                scheduledAtUtc.Value,
                duration,
                excludePlacementTestId: null,
                branchId: command.BranchId,
                cancellationToken);

            if (availability.IsFailure)
            {
                return Result.Failure<RetakePlacementTestResponse>(availability.Error);
            }
        }
        else
        {
            if (command.InvigilatorUserId.HasValue)
            {
                var assignable = await PlacementTestScheduleAvailability.EnsureInvigilatorAssignableAsync(
                    context,
                    command.InvigilatorUserId.Value,
                    cancellationToken);

                if (assignable.IsFailure)
                {
                    return Result.Failure<RetakePlacementTestResponse>(assignable.Error);
                }
            }

            if (command.RoomId.HasValue)
            {
                var assignable = await PlacementTestScheduleAvailability.EnsureRoomAssignableAsync(
                    context,
                    command.RoomId.Value,
                    command.BranchId,
                    cancellationToken);

                if (assignable.IsFailure)
                {
                    return Result.Failure<RetakePlacementTestResponse>(assignable.Error);
                }
            }
        }

        var roomName = command.RoomId.HasValue
            ? await context.Classrooms
                .AsNoTracking()
                .Where(r => r.Id == command.RoomId.Value)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(cancellationToken)
            : null;

        var newProgram = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.NewProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (newProgram is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.ProgramNotFound(command.NewProgramId));
        }

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            command.BranchId,
            command.NewProgramId,
            cancellationToken);

        if (!programAssignedToBranch)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.ProgramNotAvailableInBranch(command.NewProgramId, command.BranchId));
        }

        var newTuitionPlan = await context.TuitionPlans
            .FirstOrDefaultAsync(tp =>
                tp.Id == command.NewTuitionPlanId &&
                tp.ProgramId == command.NewProgramId &&
                tp.IsActive &&
                !tp.IsDeleted &&
                (!tp.BranchId.HasValue || tp.BranchId == command.BranchId), cancellationToken);

        if (newTuitionPlan is null)
        {
            return Result.Failure<RetakePlacementTestResponse>(
                RegistrationErrors.TuitionPlanNotFound(command.NewTuitionPlanId));
        }

        var newPlacementTest = new PlacementTest
        {
            Id = Guid.NewGuid(),
            LeadId = originalPlacementTest.LeadId,
            LeadChildId = originalPlacementTest.LeadChildId,
            StudentProfileId = command.StudentProfileId,
            ClassId = null,
            ScheduledAt = scheduledAtUtc,
            DurationMinutes = duration,
            Status = PlacementTestStatus.Scheduled,
            RoomId = command.RoomId,
            Room = roomName,
            InvigilatorUserId = command.InvigilatorUserId,
            OriginalPlacementTestId = command.OriginalPlacementTestId,
            ResultScore = null,
            ListeningScore = null,
            SpeakingScore = null,
            ReadingScore = null,
            WritingScore = null,
            LevelRecommendation = null,
            ProgramRecommendationId = newProgram.Id,
            SecondaryProgramRecommendationId = null,
            SecondaryProgramSkillFocus = null,
            Notes = command.Note ?? $"Retake from PlacementTest {originalPlacementTest.Id}. Target program: {newProgram.Name}.",
            AttachmentUrl = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.PlacementTests.Add(newPlacementTest);

        if (originalPlacementTest.LeadId.HasValue)
        {
            var scheduledAtStr = command.ScheduledAt?.ToString("yyyy-MM-dd HH:mm") ?? "TBD";
            context.LeadActivities.Add(new LeadActivity
            {
                Id = Guid.NewGuid(),
                LeadId = originalPlacementTest.LeadId.Value,
                ActivityType = ActivityType.Note,
                Content = $"Student retake placement test scheduled. Original program: '{(originalProgramName ?? "N/A")}'. Target program after retake: '{newProgram.Name}'. Scheduled at: {scheduledAtStr}.",
                CreatedAt = now
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        return new RetakePlacementTestResponse
        {
            NewPlacementTestId = newPlacementTest.Id,
            OriginalPlacementTestId = originalPlacementTest.Id,
            StudentProfileId = command.StudentProfileId,
            OriginalProgramName = originalProgramName,
            NewProgramName = newProgram.Name,
            OriginalTuitionPlanName = originalTuitionPlanName,
            NewTuitionPlanName = newTuitionPlan.Name,
            OriginalRemainingSessions = originalRemainingSessions,
            PlacementTestStatus = newPlacementTest.Status.ToString(),
            ScheduledAt = newPlacementTest.ScheduledAt,
            DurationMinutes = newPlacementTest.DurationMinutes,
            RoomId = newPlacementTest.RoomId,
            RoomName = roomName,
            Room = newPlacementTest.Room,
            InvigilatorUserId = newPlacementTest.InvigilatorUserId,
            CreatedAt = now
        };
    }
}
