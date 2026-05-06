using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionSchedules;

public sealed class GetProgramProgressionSchedulesQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetProgramProgressionSchedulesQuery, GetProgramProgressionSchedulesResponse>
{
    public async Task<Result<GetProgramProgressionSchedulesResponse>> Handle(
        GetProgramProgressionSchedulesQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserRole = await ProgramProgressionAccessHelper.GetCurrentUserRoleAsync(
            context,
            userContext.UserId,
            cancellationToken);

        var schedulesQuery = ProgramProgressionScheduleReadQuery.Build(context).AsQueryable();

        if (currentUserRole == UserRole.Teacher)
        {
            schedulesQuery = schedulesQuery.Where(schedule => schedule.AssignedTeacherUserId == userContext.UserId);
        }

        if (query.SourceClassId.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(schedule => schedule.SourceClassId == query.SourceClassId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(schedule =>
                schedule.Participants.Any(participant => participant.StudentProfileId == query.StudentProfileId.Value));
        }

        if (query.AssignedTeacherUserId.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(schedule => schedule.AssignedTeacherUserId == query.AssignedTeacherUserId.Value);
        }

        if (query.Status.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(schedule => schedule.Status == query.Status.Value);
        }

        if (query.ParticipantStatus.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(schedule =>
                schedule.Participants.Any(participant => participant.Status == query.ParticipantStatus.Value));
        }

        if (query.From.HasValue)
        {
            var fromUtc = VietnamTime.NormalizeToUtc(query.From.Value);
            schedulesQuery = schedulesQuery.Where(schedule => schedule.ScheduledAt >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.NormalizeToUtc(query.To.Value));
            schedulesQuery = schedulesQuery.Where(schedule => schedule.ScheduledAt <= toUtc);
        }

        var totalCount = await schedulesQuery.CountAsync(cancellationToken);

        var schedules = await schedulesQuery
            .OrderByDescending(schedule => schedule.ScheduledAt)
            .ThenByDescending(schedule => schedule.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var page = new Page<ProgramProgressionScheduleDto>(
            schedules.Select(schedule => schedule.ToDto()).ToList(),
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetProgramProgressionSchedulesResponse
        {
            Schedules = page
        });
    }
}
