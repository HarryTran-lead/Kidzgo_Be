using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetMyProgramProgressionSchedules;

public sealed class GetMyProgramProgressionSchedulesQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetMyProgramProgressionSchedulesQuery, GetMyProgramProgressionSchedulesResponse>
{
    public async Task<Result<GetMyProgramProgressionSchedulesResponse>> Handle(
        GetMyProgramProgressionSchedulesQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserRole = await ProgramProgressionAccessHelper.GetCurrentUserRoleAsync(
            context,
            userContext.UserId,
            cancellationToken);

        var schedulesQuery = ProgramProgressionScheduleReadQuery.Build(context).AsQueryable();
        Func<Domain.ProgramProgressions.ProgramProgressionScheduleParticipant, bool>? participantFilter = null;
        List<Guid>? visibleStudentIds = null;

        switch (currentUserRole)
        {
            case UserRole.Teacher:
                schedulesQuery = schedulesQuery.Where(schedule => schedule.AssignedTeacherUserId == userContext.UserId);
                break;

            case UserRole.Student:
                if (!userContext.StudentId.HasValue)
                {
                    return Result.Failure<GetMyProgramProgressionSchedulesResponse>(
                        Error.NotFound("StudentProfile", "Student profile not found in token."));
                }

                schedulesQuery = schedulesQuery.Where(schedule =>
                    schedule.Participants.Any(participant => participant.StudentProfileId == userContext.StudentId.Value));
                participantFilter = participant => participant.StudentProfileId == userContext.StudentId.Value;
                visibleStudentIds = [userContext.StudentId.Value];
                break;

            case UserRole.Parent:
                if (!userContext.ParentId.HasValue)
                {
                    return Result.Failure<GetMyProgramProgressionSchedulesResponse>(
                        Error.NotFound("ParentProfile", "Parent profile not found in token."));
                }

                var linkedStudentIds = await context.ParentStudentLinks
                    .AsNoTracking()
                    .Where(link => link.ParentProfileId == userContext.ParentId.Value)
                    .Select(link => link.StudentProfileId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                if (query.StudentProfileId.HasValue)
                {
                    if (!linkedStudentIds.Contains(query.StudentProfileId.Value))
                    {
                        return Result.Failure<GetMyProgramProgressionSchedulesResponse>(
                            Error.Validation("StudentProfile", "Student is not linked to the current parent."));
                    }

                    linkedStudentIds = [query.StudentProfileId.Value];
                }

                schedulesQuery = schedulesQuery.Where(schedule =>
                    schedule.Participants.Any(participant => linkedStudentIds.Contains(participant.StudentProfileId)));
                participantFilter = participant => linkedStudentIds.Contains(participant.StudentProfileId);
                visibleStudentIds = linkedStudentIds;
                break;

            default:
                return Result.Failure<GetMyProgramProgressionSchedulesResponse>(
                    Error.Unauthorized("ProgramProgression.MySchedulesUnauthorized", "Current user cannot access assessment schedules."));
        }

        if (query.Status.HasValue)
        {
            schedulesQuery = schedulesQuery.Where(schedule => schedule.Status == query.Status.Value);
        }

        if (query.ParticipantStatus.HasValue)
        {
            if (visibleStudentIds is { Count: > 0 })
            {
                schedulesQuery = schedulesQuery.Where(schedule =>
                    schedule.Participants.Any(participant =>
                        visibleStudentIds.Contains(participant.StudentProfileId) &&
                        participant.Status == query.ParticipantStatus.Value));
            }
            else
            {
                schedulesQuery = schedulesQuery.Where(schedule =>
                    schedule.Participants.Any(participant => participant.Status == query.ParticipantStatus.Value));
            }
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
            .OrderBy(schedule => schedule.ScheduledAt)
            .ThenBy(schedule => schedule.SourceClass.Title)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var page = new Page<ProgramProgressionScheduleDto>(
            schedules.Select(schedule => schedule.ToDto(participantFilter)).ToList(),
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetMyProgramProgressionSchedulesResponse
        {
            Schedules = page
        });
    }
}
