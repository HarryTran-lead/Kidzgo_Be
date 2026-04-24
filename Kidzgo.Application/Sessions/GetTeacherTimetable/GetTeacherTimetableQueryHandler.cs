using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetTeacherTimetable;

public sealed class GetTeacherTimetableQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetTeacherTimetableQuery, GetTeacherTimetableResponse>
{
    public async Task<Result<GetTeacherTimetableResponse>> Handle(GetTeacherTimetableQuery query, CancellationToken cancellationToken)
    {
        var currentUserRole = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .Select(u => (UserRole?)u.Role)
            .FirstOrDefaultAsync(cancellationToken);

        var userId = currentUserRole == UserRole.Teacher
            ? userContext.UserId
            : query.TeacherUserId ?? userContext.UserId;

        // Get sessions where teacher is PlannedTeacher or ActualTeacher
        // Note: When using Select projection, Include is not needed as EF Core will only load what's referenced
        var sessionsQuery = context.Sessions
            .Where(s => (s.PlannedTeacherId == userId || s.ActualTeacherId == userId) 
                     && s.Status != SessionStatus.Cancelled);

        // Filter by date range
        // Convert to UTC if DateTime is Unspecified (from query string)
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

        if (query.BranchId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.BranchId == query.BranchId.Value);
        }

        if (query.ClassId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == query.ClassId.Value);
        }

        var sessionRows = await sessionsQuery
            .OrderBy(s => s.PlannedDatetime)
            .Select(s => new TimetableItemDto
            {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                ClassCode = s.Class.Code,
                ClassTitle = s.Class.Title,
                PlannedDatetime = s.PlannedDatetime,
                ActualDatetime = s.ActualDatetime,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType,
                Status = s.Status,
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
                LessonPlanId = s.LessonPlan != null ? s.LessonPlan.Id : null,
                LessonPlanLink = s.LessonPlan != null ? $"/api/lesson-plans/{s.LessonPlan.Id}" : null,
                AttendanceStatus = null
            })
            .ToListAsync(cancellationToken);

        var sessions = sessionRows
            .Select(s => new TimetableItemDto
            {
                Id = s.Id,
                Color = s.Color,
                ClassId = s.ClassId,
                ClassCode = s.ClassCode,
                ClassTitle = s.ClassTitle,
                PlannedDatetime = VietnamTime.ToVietnamDateTime(s.PlannedDatetime),
                ActualDatetime = s.ActualDatetime.HasValue
                    ? VietnamTime.ToVietnamDateTime(s.ActualDatetime.Value)
                    : null,
                DurationMinutes = s.DurationMinutes,
                ParticipationType = s.ParticipationType,
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
                LessonPlanId = s.LessonPlanId,
                LessonPlanLink = s.LessonPlanLink,
                AttendanceStatus = s.AttendanceStatus
            })
            .ToList();

        return Result.Success(new GetTeacherTimetableResponse
        {
            Sessions = sessions
        });
    }
}

