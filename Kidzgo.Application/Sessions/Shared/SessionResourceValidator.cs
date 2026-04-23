using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.Shared;

internal static class SessionResourceValidator
{
    public static async Task<Result> ValidateAsync(
        IDbContext context,
        Guid branchId,
        Guid? roomId,
        Guid? teacherId,
        Guid? assistantId,
        CancellationToken cancellationToken)
    {
        if (teacherId.HasValue && assistantId.HasValue && teacherId.Value == assistantId.Value)
        {
            return Result.Failure(SessionErrors.TeacherAndAssistantMustDiffer);
        }

        if (roomId.HasValue)
        {
            bool roomExists = await context.Classrooms.AnyAsync(
                r => r.Id == roomId.Value && r.BranchId == branchId && r.IsActive,
                cancellationToken);

            if (!roomExists)
            {
                return Result.Failure(SessionErrors.InvalidRoom(roomId.Value));
            }
        }

        if (teacherId.HasValue)
        {
            bool teacherExists = await context.Users.AnyAsync(
                u => u.Id == teacherId.Value &&
                     u.Role == UserRole.Teacher &&
                     u.BranchId == branchId &&
                     u.IsActive &&
                     !u.IsDeleted,
                cancellationToken);

            if (!teacherExists)
            {
                return Result.Failure(SessionErrors.InvalidTeacher(teacherId.Value));
            }
        }

        if (assistantId.HasValue)
        {
            bool assistantExists = await context.Users.AnyAsync(
                u => u.Id == assistantId.Value &&
                     u.Role == UserRole.Teacher &&
                     u.BranchId == branchId &&
                     u.IsActive &&
                     !u.IsDeleted,
                cancellationToken);

            if (!assistantExists)
            {
                return Result.Failure(SessionErrors.InvalidAssistant(assistantId.Value));
            }
        }

        return Result.Success();
    }
}
