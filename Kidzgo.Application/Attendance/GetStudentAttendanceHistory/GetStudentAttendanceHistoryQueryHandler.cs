using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.GetStudentAttendanceHistory;

public sealed class GetStudentAttendanceHistoryQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetStudentAttendanceHistoryQuery, Page<GetStudentAttendanceHistoryResponse>>
{
    public async Task<Result<Page<GetStudentAttendanceHistoryResponse>>> Handle(GetStudentAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        // Get StudentId from context (token)
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<Page<GetStudentAttendanceHistoryResponse>>(ProfileErrors.StudentNotFound);
        }

        // Verify the student belongs to the current user
        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value && p.UserId == userContext.UserId, cancellationToken);

        if (profile == null || profile.ProfileType != Domain.Users.ProfileType.Student)
        {
            return Result.Failure<Page<GetStudentAttendanceHistoryResponse>>(ProfileErrors.StudentNotFound);
        }

        var query = context.Attendances
            .Include(a => a.Session)
            .Where(a => a.StudentProfileId == studentId.Value);

        var total = await query.CountAsync(cancellationToken);

        var attendanceRows = await query
            .OrderByDescending(a => a.Session.PlannedDatetime)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .Select(a => new GetStudentAttendanceHistoryResponse
            {
                Id = a.Id,
                SessionId = a.SessionId,
                SessionDateTime = a.Session.PlannedDatetime,
                AttendanceStatus = a.AttendanceStatus.ToString(),
                AbsenceType = a.AbsenceType.HasValue ? a.AbsenceType.Value.ToString() : null,
                Note = a.Note
            })
            .ToListAsync(cancellationToken);

        var items = attendanceRows
            .Select(a => new GetStudentAttendanceHistoryResponse
            {
                Id = a.Id,
                SessionId = a.SessionId,
                SessionDateTime = VietnamTime.ToVietnamDateTime(a.SessionDateTime),
                AttendanceStatus = a.AttendanceStatus,
                AbsenceType = a.AbsenceType,
                Note = a.Note
            })
            .ToList();

        return new Page<GetStudentAttendanceHistoryResponse>(items, total, request.PageNumber, request.PageSize);
    }
}

