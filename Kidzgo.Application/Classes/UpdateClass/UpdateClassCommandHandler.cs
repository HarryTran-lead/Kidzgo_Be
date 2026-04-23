using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Classes.UpdateClass;

public sealed class UpdateClassCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateClassCommand, UpdateClassResponse>
{
    public async Task<Result<UpdateClassResponse>> Handle(UpdateClassCommand command, CancellationToken cancellationToken)
    {
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.NotFound(command.Id));
        }

        // Check if branch exists
        bool branchExists = await context.Branches
            .AnyAsync(b => b.Id == command.BranchId && b.IsActive, cancellationToken);

        if (!branchExists)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.BranchNotFound);
        }

        // Check if program exists
        bool programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.ProgramNotFound);
        }

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            command.BranchId,
            command.ProgramId,
            cancellationToken);

        if (!programAssignedToBranch)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.ProgramNotAvailableInBranch);
        }

        // Check if code is unique (excluding current class)
        bool codeExists = await context.Classes
            .AnyAsync(c => c.Code == command.Code && c.Id != command.Id, cancellationToken);

        if (codeExists)
        {
            return Result.Failure<UpdateClassResponse>(
                ClassErrors.CodeExists);
        }

        // Check if teachers exist, are TEACHER role, and belong to the same branch
        if (command.MainTeacherId.HasValue)
        {
            var mainTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.MainTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (mainTeacher is null)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.MainTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (mainTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.MainTeacherBranchMismatch);
            }
        }

        if (command.AssistantTeacherId.HasValue)
        {
            var assistantTeacher = await context.Users
                .FirstOrDefaultAsync(u => u.Id == command.AssistantTeacherId.Value && u.Role == Domain.Users.UserRole.Teacher, cancellationToken);

            if (assistantTeacher is null)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.AssistantTeacherNotFound);
            }

            // Check if teacher belongs to the same branch as the class
            if (assistantTeacher.BranchId != command.BranchId)
            {
                return Result.Failure<UpdateClassResponse>(
                    ClassErrors.AssistantTeacherBranchMismatch);
            }
        }

        classEntity.BranchId = command.BranchId;
        classEntity.ProgramId = command.ProgramId;
        classEntity.Code = command.Code;
        classEntity.Title = command.Title;
        classEntity.RoomId = command.RoomId;
        classEntity.MainTeacherId = command.MainTeacherId;
        classEntity.AssistantTeacherId = command.AssistantTeacherId;
        classEntity.StartDate = command.StartDate;
        classEntity.EndDate = command.EndDate;
        classEntity.Capacity = command.Capacity;
        classEntity.SchedulePattern = command.SchedulePattern;
        classEntity.Description = command.Description;
        classEntity.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateClassResponse
        {
            Id = classEntity.Id,
            BranchId = classEntity.BranchId,
            ProgramId = classEntity.ProgramId,
            Code = classEntity.Code,
            Title = classEntity.Title,
            RoomId = classEntity.RoomId,
            MainTeacherId = classEntity.MainTeacherId,
            AssistantTeacherId = classEntity.AssistantTeacherId,
            StartDate = classEntity.StartDate,
            EndDate = classEntity.EndDate,
            Status = classEntity.Status.ToString(),
            Capacity = classEntity.Capacity,
            SchedulePattern = classEntity.SchedulePattern,
            Description = classEntity.Description
        };
    }
}

