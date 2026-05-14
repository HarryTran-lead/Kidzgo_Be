using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetSessions;

public sealed class GetSessionsQueryHandler(
    IDbContext context
) : IQueryHandler<GetSessionsQuery, GetSessionsResponse>
{
    public async Task<Result<GetSessionsResponse>> Handle(GetSessionsQuery query, CancellationToken cancellationToken)
    {
        var sessionsQuery = context.Sessions
            .Include(s => s.Class)
                .ThenInclude(c => c.Branch)
            .Include(s => s.PlannedRoom)
            .Include(s => s.ActualRoom)
            .Include(s => s.PlannedTeacher)
            .Include(s => s.ActualTeacher)
            .Include(s => s.PlannedAssistant)
            .Include(s => s.ActualAssistant)
            .AsQueryable();

        if (query.ClassId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == query.ClassId.Value);
        }

        if (query.BranchId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.BranchId == query.BranchId.Value);
        }

        if (query.Status.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            var fromUtc = VietnamTime.NormalizeToUtc(query.From.Value);
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime >= fromUtc);
        }

        if (query.To.HasValue)
        {
            var toUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.NormalizeToUtc(query.To.Value));
            sessionsQuery = sessionsQuery.Where(s => s.PlannedDatetime <= toUtc);
        }

        var totalCount = await sessionsQuery.CountAsync(cancellationToken);

        var sessionRows = await sessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(s => new SessionListItemDto
            {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                BranchId = s.BranchId,
                BranchName = s.Branch.Name,
                PlannedDatetime = s.PlannedDatetime,
                ActualDatetime = s.ActualDatetime,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType.ToString(),
                SectionType = s.SectionType.ToString(),
                Status = s.Status.ToString(),
                PlannedRoomId = s.PlannedRoomId,
                PlannedRoomName = s.PlannedRoom != null ? s.PlannedRoom.Name : null,
                ActualRoomId = s.ActualRoomId,
                ActualRoomName = s.ActualRoom != null ? s.ActualRoom.Name : null,
                PlannedTeacherId = s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                ActualTeacherId = s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacher != null ? s.ActualTeacher.Name : null,
                PlannedAssistantId = s.PlannedAssistantId,
                PlannedAssistantName = s.PlannedAssistant != null ? s.PlannedAssistant.Name : null,
                ActualAssistantId = s.ActualAssistantId,
                ActualAssistantName = s.ActualAssistant != null ? s.ActualAssistant.Name : null
            })
            .ToListAsync(cancellationToken);

        var items = sessionRows
            .Select(s => new SessionListItemDto
            {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                ClassCode = s.ClassCode,
                ClassTitle = s.ClassTitle,
                BranchId = s.BranchId,
                BranchName = s.BranchName,
                PlannedDatetime = VietnamTime.ToVietnamDateTime(s.PlannedDatetime),
                ActualDatetime = s.ActualDatetime.HasValue
                    ? VietnamTime.ToVietnamDateTime(s.ActualDatetime.Value)
                    : null,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType,
                SectionType = s.SectionType,
                Status = s.Status,
                PlannedRoomId = s.PlannedRoomId,
                PlannedRoomName = s.PlannedRoomName,
                ActualRoomId = s.ActualRoomId,
                ActualRoomName = s.ActualRoomName,
                PlannedTeacherId = s.PlannedTeacherId,
                PlannedTeacherName = s.PlannedTeacherName,
                ActualTeacherId = s.ActualTeacherId,
                ActualTeacherName = s.ActualTeacherName,
                PlannedAssistantId = s.PlannedAssistantId,
                PlannedAssistantName = s.PlannedAssistantName,
                ActualAssistantId = s.ActualAssistantId,
                ActualAssistantName = s.ActualAssistantName
            })
            .ToList();

        var page = new Page<SessionListItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetSessionsResponse
        {
            Sessions = page
        };
    }
}


