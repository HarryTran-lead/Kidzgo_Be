using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.GetSessionAttendance;

public sealed class GetSessionAttendanceQueryHandler(
    IDbContext context,
    SessionParticipantService sessionParticipantService,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : IQueryHandler<GetSessionAttendanceQuery, GetSessionAttendanceListResponse>
{
    public async Task<Result<GetSessionAttendanceListResponse>> Handle(GetSessionAttendanceQuery request, CancellationToken cancellationToken)
    {
        // Get session with class info
        var session = await context.Sessions
            .AsNoTracking()
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<GetSessionAttendanceListResponse>(SessionErrors.NotFound(request.SessionId));
        }

        var participants = await sessionParticipantService
            .GetParticipantsAsync(request.SessionId, cancellationToken);

        var studentIds = participants
            .Select(p => p.StudentProfileId)
            .Distinct()
            .ToList();

        var students = await context.Profiles
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => studentIds.Contains(p.Id))
            .Select(p => new
            {
                p.Id,
                p.DisplayName,
                AvatarUrl = p.AvatarUrl ?? p.User.AvatarUrl
            })
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        var attendances = await context.Attendances
            .AsNoTracking()
            .Where(a => a.SessionId == request.SessionId && studentIds.Contains(a.StudentProfileId))
            .ToDictionaryAsync(a => a.StudentProfileId, cancellationToken);

        var studentsWithCredit = await context.MakeupCredits
            .AsNoTracking()
            .Where(c => studentIds.Contains(c.StudentProfileId) && c.Status == MakeupCreditStatus.Available)
            .Select(c => c.StudentProfileId)
            .Distinct()
            .ToHashSetAsync(cancellationToken);

        var studentsWithApprovedLeave = await approvedLeaveAttendanceService
            .GetApprovedLeaveStudentIdsForSessionAsync(session, studentIds, cancellationToken);

        var data = participants
            .Where(p => students.ContainsKey(p.StudentProfileId))
            .Select(p =>
            {
                attendances.TryGetValue(p.StudentProfileId, out var attendance);
                var hasApprovedLeave = studentsWithApprovedLeave.Contains(p.StudentProfileId);
                return new GetSessionAttendanceResponse
                {
                    Id = attendance?.Id ?? Guid.Empty,
                    StudentProfileId = p.StudentProfileId,
                    StudentName = students[p.StudentProfileId].DisplayName,
                    StudentAvatarUrl = students[p.StudentProfileId].AvatarUrl,
                    RegistrationId = p.RegistrationId,
                    Track = p.Track.HasValue ? RegistrationTrackHelper.ToTrackName(p.Track.Value) : null,
                    IsMakeup = p.IsMakeup,
                    AttendanceStatus = hasApprovedLeave
                        ? AttendanceStatus.Makeup.ToString()
                        : attendance?.AttendanceStatus.ToString() ?? AttendanceStatus.NotMarked.ToString(),
                    AbsenceType = hasApprovedLeave ? null : attendance?.AbsenceType?.ToString(),
                    HasMakeupCredit = studentsWithCredit.Contains(p.StudentProfileId),
                    HasApprovedLeave = hasApprovedLeave,
                    Note = attendance?.Note,
                    MarkedAt = hasApprovedLeave ? null : attendance?.MarkedAt
                };
            })
            .OrderBy(x => x.StudentName)
            .ToList();

        // Calculate summary
        var summary = new AttendanceSummary
        {
            TotalStudents = data.Count,
            PresentCount = data.Count(a => a.AttendanceStatus == "Present"),
            AbsentCount = data.Count(a => a.AttendanceStatus == "Absent"),
            MakeupCount = data.Count(a => a.AttendanceStatus == "Makeup"),
            NotMarkedCount = data.Count(a => a.AttendanceStatus == "NotMarked")
        };

        var response = new GetSessionAttendanceListResponse
        {
            SessionId = session.Id,
            SessionName = session.Class?.Title,
            Date = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime),
            StartTime = VietnamTime.ToVietnamTimeOnly(session.PlannedDatetime),
            EndTime = VietnamTime.ToVietnamTimeOnly(session.PlannedDatetime.AddMinutes(session.DurationMinutes)),
            Summary = summary,
            Attendances = data
        };

        return response;
    }
}

