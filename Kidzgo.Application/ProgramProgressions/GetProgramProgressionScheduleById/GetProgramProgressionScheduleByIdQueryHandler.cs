using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleById;

public sealed class GetProgramProgressionScheduleByIdQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetProgramProgressionScheduleByIdQuery, ProgramProgressionScheduleDto>
{
    public async Task<Result<ProgramProgressionScheduleDto>> Handle(
        GetProgramProgressionScheduleByIdQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserRole = await ProgramProgressionAccessHelper.GetCurrentUserRoleAsync(
            context,
            userContext.UserId,
            cancellationToken);

        var schedule = await ProgramProgressionScheduleReadQuery.Build(context)
            .FirstOrDefaultAsync(s => s.Id == query.Id, cancellationToken);

        if (schedule is null)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.ScheduleNotFound(query.Id));
        }

        if (currentUserRole == UserRole.Teacher && schedule.AssignedTeacherUserId != userContext.UserId)
        {
            return Result.Failure<ProgramProgressionScheduleDto>(
                ProgramProgressionErrors.TeacherNotAssignedToSchedule(userContext.UserId, schedule.Id));
        }

        return Result.Success(schedule.ToDto());
    }
}
