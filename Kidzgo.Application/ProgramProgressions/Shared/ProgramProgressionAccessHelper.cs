using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.Shared;

internal static class ProgramProgressionAccessHelper
{
    public static Task<UserRole?> GetCurrentUserRoleAsync(
        IDbContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive && !user.IsDeleted)
            .Select(user => (UserRole?)user.Role)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public static async Task<Result> EnsureTeacherCanManageClassAssessmentAsync(
        IDbContext context,
        Guid teacherUserId,
        Guid sourceClassId,
        CancellationToken cancellationToken)
    {
        var sourceClass = await context.Classes
            .AsNoTracking()
            .Where(c => c.Id == sourceClassId)
            .Select(c => new
            {
                c.Id,
                c.MainTeacherId,
                c.AssistantTeacherId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sourceClass is null)
        {
            return Result.Failure(ProgramProgressionErrors.SourceClassNotFound(sourceClassId));
        }

        return sourceClass.MainTeacherId == teacherUserId || sourceClass.AssistantTeacherId == teacherUserId
            ? Result.Success()
            : Result.Failure(ProgramProgressionErrors.TeacherCannotManageAssessment(teacherUserId, sourceClassId));
    }

    public static async Task<Result> EnsureTeacherAssignedToScheduleAsync(
        IDbContext context,
        Guid teacherUserId,
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        var assignedTeacherUserId = await context.ProgramProgressionSchedules
            .AsNoTracking()
            .Where(schedule => schedule.Id == scheduleId)
            .Select(schedule => (Guid?)schedule.AssignedTeacherUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!assignedTeacherUserId.HasValue)
        {
            return Result.Failure(ProgramProgressionErrors.ScheduleNotFound(scheduleId));
        }

        return assignedTeacherUserId.Value == teacherUserId
            ? Result.Success()
            : Result.Failure(ProgramProgressionErrors.TeacherNotAssignedToSchedule(teacherUserId, scheduleId));
    }
}
