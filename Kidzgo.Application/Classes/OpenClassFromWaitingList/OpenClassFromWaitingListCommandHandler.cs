using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Classes.ChangeClassStatus;
using Kidzgo.Application.Classes.CreateClass;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.AssignClass;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.OpenClassFromWaitingList;

public sealed class OpenClassFromWaitingListCommandHandler(
    IDbContext context,
    ISender sender
) : ICommandHandler<OpenClassFromWaitingListCommand, OpenClassFromWaitingListResponse>
{
    public async Task<Result<OpenClassFromWaitingListResponse>> Handle(
        OpenClassFromWaitingListCommand command,
        CancellationToken cancellationToken)
    {
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        var createClassResult = await sender.Send(command.CreateClass, cancellationToken);
        if (createClassResult.IsFailure)
        {
            return Result.Failure<OpenClassFromWaitingListResponse>(createClassResult.Error);
        }

        var createdClass = createClassResult.Value;

        if (string.Equals(createdClass.Status, ClassStatus.Planned.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            var statusResult = await sender.Send(new ChangeClassStatusCommand
            {
                Id = createdClass.Id,
                Status = ClassStatus.Recruiting
            }, cancellationToken);

            if (statusResult.IsFailure)
            {
                return Result.Failure<OpenClassFromWaitingListResponse>(statusResult.Error);
            }
        }

        var waitingRegistrationIds = await GetWaitingRegistrationIdsAsync(
            createdClass.BranchId,
            createdClass.ProgramId,
            createdClass.LevelId,
            track,
            cancellationToken);

        var assignedRegistrationIds = new List<Guid>();
        var skippedCount = 0;

        foreach (var registrationId in waitingRegistrationIds)
        {
            if (assignedRegistrationIds.Count >= createdClass.Capacity)
            {
                break;
            }

            var assignResult = await sender.Send(new AssignClassCommand
            {
                RegistrationId = registrationId,
                ClassId = createdClass.Id,
                EntryType = nameof(EntryType.Immediate),
                Track = track
            }, cancellationToken);

            if (assignResult.IsSuccess)
            {
                assignedRegistrationIds.Add(registrationId);
                continue;
            }

            if (CanSkipAssignmentFailure(assignResult.Error))
            {
                skippedCount++;
                continue;
            }

            return Result.Failure<OpenClassFromWaitingListResponse>(assignResult.Error);
        }

        var classEntity = await context.Classes
            .Include(c => c.Syllabus)
            .FirstAsync(c => c.Id == createdClass.Id, cancellationToken);

        var finalClassResponse = MapClassToResponse(classEntity);

        await transaction.CommitAsync(cancellationToken);

        return new OpenClassFromWaitingListResponse
        {
            CreatedClass = finalClassResponse,
            Track = track,
            WaitingCount = waitingRegistrationIds.Count,
            AssignedCount = assignedRegistrationIds.Count,
            SkippedCount = skippedCount,
            AssignedRegistrationIds = assignedRegistrationIds
        };
    }

    private async Task<List<Guid>> GetWaitingRegistrationIdsAsync(
        Guid branchId,
        Guid programId,
        Guid levelId,
        string track,
        CancellationToken cancellationToken)
    {
        var query = context.Registrations
            .AsNoTracking()
            .Where(registration =>
                registration.BranchId == branchId &&
                registration.ProgramId == programId &&
                registration.Status != RegistrationStatus.Completed &&
                registration.Status != RegistrationStatus.Cancelled);

        query = string.Equals(track, RegistrationTrackHelper.SecondaryTrack, StringComparison.OrdinalIgnoreCase)
            ? query.Where(registration =>
                registration.SecondaryLevelId == levelId &&
                registration.SecondaryClassId == null)
            : query.Where(registration =>
                registration.LevelId == levelId &&
                registration.ClassId == null);

        return await query
            .OrderBy(registration => registration.ExpectedStartDate ?? registration.RegistrationDate)
            .Select(registration => registration.Id)
            .ToListAsync(cancellationToken);
    }

    private static bool CanSkipAssignmentFailure(Error error)
    {
        return error.Type is ErrorType.Validation or ErrorType.Conflict or ErrorType.NotFound;
    }

    private static CreateClassResponse MapClassToResponse(Domain.Classes.Class classEntity)
    {
        var slotResult = SchedulePatternSupport.ParseScheduleSlots(classEntity.WeeklyScheduleJson ?? string.Empty);

        return new CreateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            LevelId = classEntity.LevelId,
            SyllabusId = classEntity.SyllabusId,
            SyllabusCode = classEntity.Syllabus?.Code,
            SyllabusVersion = classEntity.Syllabus?.Version,
            SyllabusTitle = classEntity.Syllabus?.Title,
            StartModuleId = classEntity.StartModuleId,
            StartSessionIndex = classEntity.StartSessionIndex,
            CurrentModuleId = classEntity.CurrentModuleId,
            CurrentSessionIndex = classEntity.CurrentSessionIndex,
            CurrentLessonPlanTemplateId = classEntity.CurrentLessonPlanTemplateId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            RoomId = classEntity.RoomId,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            StartDate = classEntity.StartDate,
            ExpectedEndDate = classEntity.ExpectedEndDate,
            ActualEndDate = classEntity.ActualEndDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status.ToString(),
            Capacity = classEntity.Capacity,
            WeeklyScheduleSlots = slotResult.IsSuccess ? slotResult.Value : [],
            Description = classEntity.Description
        };
    }
}
