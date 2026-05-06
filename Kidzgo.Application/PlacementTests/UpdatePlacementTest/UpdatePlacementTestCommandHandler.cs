using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PlacementTests;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTest;

public sealed class UpdatePlacementTestCommandHandler(
    IDbContext context
) : ICommandHandler<UpdatePlacementTestCommand, UpdatePlacementTestResponse>
{
    public async Task<Result<UpdatePlacementTestResponse>> Handle(
        UpdatePlacementTestCommand command,
        CancellationToken cancellationToken)
    {
        // UC-029: Update Placement Test
        var placementTest = await context.PlacementTests
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<UpdatePlacementTestResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        // Cannot update completed test
        if (placementTest.Status == PlacementTestStatus.Completed)
        {
            return Result.Failure<UpdatePlacementTestResponse>(
                PlacementTestErrors.CannotUpdateCompletedTest);
        }

        // Validate StudentProfile if provided
        if (command.StudentProfileId.HasValue)
        {
            var profile = await context.Profiles
                .FirstOrDefaultAsync(p => p.Id == command.StudentProfileId.Value, cancellationToken);

            if (profile is null || profile.ProfileType != Kidzgo.Domain.Users.ProfileType.Student)
            {
                return Result.Failure<UpdatePlacementTestResponse>(
                    PlacementTestErrors.StudentProfileNotFound(command.StudentProfileId));
            }

            var duplicateCheck = await PlacementTestDuplicateGuard.EnsureStudentProfileCanBeAssignedAsync(
                context,
                command.StudentProfileId.Value,
                command.PlacementTestId,
                cancellationToken);

            if (duplicateCheck.IsFailure)
            {
                return Result.Failure<UpdatePlacementTestResponse>(duplicateCheck.Error);
            }

            placementTest.StudentProfileId = command.StudentProfileId.Value;
        }

        // Validate Class if provided
        if (command.ClassId.HasValue)
        {
            var classExists = await context.Classes
                .AnyAsync(c => c.Id == command.ClassId.Value, cancellationToken);

            if (!classExists)
            {
                return Result.Failure<UpdatePlacementTestResponse>(
                    PlacementTestErrors.ClassNotFound(command.ClassId));
            }

            placementTest.ClassId = command.ClassId.Value;
        }

        var effectiveInvigilatorUserId = command.InvigilatorUserId ?? placementTest.InvigilatorUserId;
        var effectiveRoomId = command.RoomId ?? placementTest.RoomId;
        var effectiveDuration = command.DurationMinutes ?? placementTest.DurationMinutes;
        var effectiveScheduledAt = command.ScheduledAt.HasValue
            ? VietnamTime.NormalizeToUtc(command.ScheduledAt.Value)
            : placementTest.ScheduledAt;

        if (effectiveDuration <= 0)
        {
            return Result.Failure<UpdatePlacementTestResponse>(PlacementTestErrors.InvalidDuration);
        }

        var scheduleFieldsChanged = command.ScheduledAt.HasValue ||
                                    command.DurationMinutes.HasValue ||
                                    command.RoomId.HasValue ||
                                    command.InvigilatorUserId.HasValue;

        if (scheduleFieldsChanged && effectiveScheduledAt.HasValue)
        {
            if (!effectiveInvigilatorUserId.HasValue)
            {
                return Result.Failure<UpdatePlacementTestResponse>(PlacementTestErrors.InvigilatorRequired);
            }

            if (!effectiveRoomId.HasValue)
            {
                return Result.Failure<UpdatePlacementTestResponse>(PlacementTestErrors.RoomRequired);
            }

            var branchId = placementTest.LeadId.HasValue
                ? await context.Leads
                    .AsNoTracking()
                    .Where(l => l.Id == placementTest.LeadId.Value)
                    .Select(l => l.BranchPreference)
                    .FirstOrDefaultAsync(cancellationToken)
                : null;

            var availability = await PlacementTestScheduleAvailability.EnsureScheduleAvailableAsync(
                context,
                effectiveInvigilatorUserId.Value,
                effectiveRoomId.Value,
                effectiveScheduledAt.Value,
                effectiveDuration,
                placementTest.Id,
                branchId,
                cancellationToken);

            if (availability.IsFailure)
            {
                return Result.Failure<UpdatePlacementTestResponse>(availability.Error);
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
                    return Result.Failure<UpdatePlacementTestResponse>(assignable.Error);
                }
            }

            if (command.RoomId.HasValue)
            {
                var assignable = await PlacementTestScheduleAvailability.EnsureRoomAssignableAsync(
                    context,
                    command.RoomId.Value,
                    branchId: null,
                    cancellationToken);

                if (assignable.IsFailure)
                {
                    return Result.Failure<UpdatePlacementTestResponse>(assignable.Error);
                }
            }
        }

        if (command.InvigilatorUserId.HasValue)
        {
            placementTest.InvigilatorUserId = command.InvigilatorUserId.Value;
        }

        if (command.DurationMinutes.HasValue)
        {
            placementTest.DurationMinutes = effectiveDuration;
        }

        string? roomName = null;
        if (effectiveRoomId.HasValue)
        {
            roomName = await context.Classrooms
                .AsNoTracking()
                .Where(r => r.Id == effectiveRoomId.Value)
                .Select(r => r.Name)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (command.RoomId.HasValue)
        {
            placementTest.RoomId = command.RoomId.Value;
        }

        if (command.ScheduledAt.HasValue)
        {
            placementTest.ScheduledAt = effectiveScheduledAt;
        }

        if (command.RoomId.HasValue)
        {
            placementTest.Room = roomName;
        }

        placementTest.UpdatedAt = VietnamTime.UtcNow();
        await context.SaveChangesAsync(cancellationToken);

        return new UpdatePlacementTestResponse
        {
            Id = placementTest.Id,
            LeadId = placementTest.LeadId,
            StudentProfileId = placementTest.StudentProfileId,
            ClassId = placementTest.ClassId,
            ScheduledAt = placementTest.ScheduledAt,
            DurationMinutes = placementTest.DurationMinutes,
            Status = placementTest.Status.ToString(),
            RoomId = placementTest.RoomId,
            RoomName = roomName,
            Room = placementTest.Room,
            InvigilatorUserId = placementTest.InvigilatorUserId,
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

