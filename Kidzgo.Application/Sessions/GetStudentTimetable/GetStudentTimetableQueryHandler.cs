using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Services;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.GetStudentTimetable;

public sealed class GetStudentTimetableQueryHandler(
    IDbContext context,
    IUserContext userContext,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService,
    StudentSessionAssignmentService studentSessionAssignmentService
) : IQueryHandler<GetStudentTimetableQuery, GetStudentTimetableResponse>
{
    public async Task<Result<GetStudentTimetableResponse>> Handle(GetStudentTimetableQuery query, CancellationToken cancellationToken)
    {
        var targetStudentsResult = await ResolveTargetStudentsAsync(cancellationToken);
        if (!targetStudentsResult.IsSuccess)
        {
            return Result.Failure<GetStudentTimetableResponse>(targetStudentsResult.Error);
        }

        var response = await BuildTimetableResponseAsync(targetStudentsResult.Value, query, cancellationToken);

        return Result.Success(response);
    }

    private async Task<Result<List<StudentTimetableTarget>>> ResolveTargetStudentsAsync(CancellationToken cancellationToken)
    {
        if (userContext.ParentId.HasValue)
        {
            return await ResolveParentStudentsAsync(cancellationToken);
        }

        var studentId = userContext.StudentId;
        if (!studentId.HasValue)
        {
            return Result.Failure<List<StudentTimetableTarget>>(ProfileErrors.StudentNotFound);
        }

        var profile = await context.Profiles
            .AsNoTracking()
            .Where(p =>
                p.Id == studentId.Value &&
                p.ProfileType == Domain.Users.ProfileType.Student &&
                !p.IsDeleted &&
                p.IsActive &&
                p.UserId == userContext.UserId)
            .Select(p => new StudentTimetableTarget(
                p.Id,
                p.DisplayName,
                p.AvatarUrl))
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return Result.Failure<List<StudentTimetableTarget>>(ProfileErrors.NotFound(studentId.Value));
        }

        return Result.Success(new List<StudentTimetableTarget> { profile });
    }

    private async Task<Result<List<StudentTimetableTarget>>> ResolveParentStudentsAsync(CancellationToken cancellationToken)
    {
        var parentProfileId = userContext.ParentId!.Value;

        var parentExists = await context.Profiles
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id == parentProfileId &&
                p.UserId == userContext.UserId &&
                p.ProfileType == Domain.Users.ProfileType.Parent &&
                !p.IsDeleted &&
                p.IsActive,
                cancellationToken);

        if (!parentExists)
        {
            return Result.Failure<List<StudentTimetableTarget>>(
                Error.NotFound("ParentProfile", "Parent profile not found"));
        }

        var students = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(link =>
                link.ParentProfileId == parentProfileId &&
                link.StudentProfile.ProfileType == Domain.Users.ProfileType.Student &&
                !link.StudentProfile.IsDeleted &&
                link.StudentProfile.IsActive)
            .Select(link => new StudentTimetableTarget(
                link.StudentProfileId,
                link.StudentProfile.DisplayName,
                link.StudentProfile.AvatarUrl))
            .Distinct()
            .OrderBy(student => student.DisplayName)
            .ToListAsync(cancellationToken);

        return Result.Success(students);
    }

    private async Task<GetStudentTimetableResponse> BuildTimetableResponseAsync(
        List<StudentTimetableTarget> students,
        GetStudentTimetableQuery query,
        CancellationToken cancellationToken)
    {
        if (students.Count == 0)
        {
            return new GetStudentTimetableResponse();
        }

        var studentIds = students.Select(student => student.Id).ToList();
        var studentLookup = students.ToDictionary(student => student.Id);
        var (fromUtc, toUtc) = NormalizeDateRange(query);

        var regularAssignmentsQuery = context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => studentIds.Contains(a.StudentProfileId)
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.Status != SessionStatus.Cancelled);

        if (fromUtc.HasValue)
        {
            regularAssignmentsQuery = regularAssignmentsQuery
                .Where(a => a.Session.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            regularAssignmentsQuery = regularAssignmentsQuery
                .Where(a => a.Session.PlannedDatetime <= toUtc.Value);
        }

        var regularAssignments = await regularAssignmentsQuery
            .Select(a => new
            {
                a.StudentProfileId,
                a.SessionId,
                a.ClassEnrollmentId,
                a.RegistrationId,
                a.Track,
                a.Session.PlannedDatetime
            })
            .ToListAsync(cancellationToken);

        var regularPauseLookup = await BuildPauseLookupAsync(
            regularAssignments.Select(a => a.ClassEnrollmentId),
            cancellationToken);

        regularAssignments = regularAssignments
            .Where(a => !EnrollmentPauseWindowHelper.IsPausedOn(
                a.ClassEnrollmentId,
                VietnamTime.ToVietnamDateOnly(a.PlannedDatetime),
                regularPauseLookup))
            .ToList();

        var studentActiveEnrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => studentIds.Contains(ce.StudentProfileId)
                && ce.Status == EnrollmentStatus.Active)
            .Select(ce => new
            {
                ce.StudentProfileId,
                ce.ClassId
            })
            .ToListAsync(cancellationToken);

        var fallbackClassIds = studentActiveEnrollments
            .Select(ce => ce.ClassId)
            .Distinct()
            .ToList();

        var fallbackAssignments = new List<SessionMetadataAssignment>();
        if (fallbackClassIds.Count > 0)
        {
            var fallbackSessionsQuery = context.Sessions
                .AsNoTracking()
                .Where(s => s.Status != SessionStatus.Cancelled
                    && fallbackClassIds.Contains(s.ClassId));

            if (fromUtc.HasValue)
            {
                fallbackSessionsQuery = fallbackSessionsQuery.Where(s => s.PlannedDatetime >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                fallbackSessionsQuery = fallbackSessionsQuery.Where(s => s.PlannedDatetime <= toUtc.Value);
            }

            var fallbackSessions = await fallbackSessionsQuery.ToListAsync(cancellationToken);

            foreach (var session in fallbackSessions)
            {
                var participants = await studentSessionAssignmentService
                    .GetRegularParticipantsAsync(session.Id, cancellationToken);

                fallbackAssignments.AddRange(participants
                    .Where(participant => studentIds.Contains(participant.StudentProfileId))
                    .Select(participant => new SessionMetadataAssignment(
                        participant.StudentProfileId,
                        session.Id,
                        participant.RegistrationId,
                        participant.Track)));
            }
        }

        var makeupAssignmentsQuery = context.MakeupAllocations
            .AsNoTracking()
            .Where(a => studentIds.Contains(a.MakeupCredit.StudentProfileId)
                && a.Status != MakeupAllocationStatus.Cancelled
                && a.TargetSession.Status != SessionStatus.Cancelled);

        if (fromUtc.HasValue)
        {
            makeupAssignmentsQuery = makeupAssignmentsQuery
                .Where(a => a.TargetSession.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            makeupAssignmentsQuery = makeupAssignmentsQuery
                .Where(a => a.TargetSession.PlannedDatetime <= toUtc.Value);
        }

        var makeupAssignments = await makeupAssignmentsQuery
            .Select(a => new
            {
                StudentProfileId = a.MakeupCredit.StudentProfileId,
                SessionId = a.TargetSessionId
            })
            .ToListAsync(cancellationToken);

        var sessionMetadata = new Dictionary<(Guid StudentProfileId, Guid SessionId), SessionMetadata>();

        foreach (var assignment in regularAssignments)
        {
            sessionMetadata[(assignment.StudentProfileId, assignment.SessionId)] = new SessionMetadata(
                assignment.RegistrationId,
                RegistrationTrackHelper.ToTrackName(assignment.Track),
                false);
        }

        foreach (var fallbackAssignment in fallbackAssignments)
        {
            sessionMetadata.TryAdd(
                (fallbackAssignment.StudentProfileId, fallbackAssignment.SessionId),
                new SessionMetadata(
                    fallbackAssignment.RegistrationId,
                    RegistrationTrackHelper.ToTrackName(fallbackAssignment.Track),
                    false));
        }

        foreach (var makeupAssignment in makeupAssignments)
        {
            var key = (makeupAssignment.StudentProfileId, makeupAssignment.SessionId);

            if (sessionMetadata.TryGetValue(key, out var existingMetadata))
            {
                sessionMetadata[key] = existingMetadata with { IsMakeup = true };
                continue;
            }

            sessionMetadata[key] = new SessionMetadata(null, null, true);
        }

        var sessionIds = sessionMetadata.Keys
            .Select(key => key.SessionId)
            .Distinct()
            .ToList();

        if (sessionIds.Count == 0)
        {
            return new GetStudentTimetableResponse();
        }

        var sessionDetails = await context.Sessions
            .AsNoTracking()
            .Where(s => sessionIds.Contains(s.Id))
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
                ParticipationType = s.ParticipationType.ToString(),
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
                LessonPlanId = s.LessonPlan != null ? s.LessonPlan.Id : null,
                LessonPlanLink = s.LessonPlan != null ? $"/api/lesson-plans/{s.LessonPlan.Id}" : null
            })
            .ToDictionaryAsync(s => s.Id, cancellationToken);

        var attendances = await context.Attendances
            .AsNoTracking()
            .Where(a => sessionIds.Contains(a.SessionId) && studentIds.Contains(a.StudentProfileId))
            .Select(a => new
            {
                a.SessionId,
                a.StudentProfileId,
                AttendanceStatus = a.AttendanceStatus.ToString(),
                AbsenceType = a.AbsenceType.ToString(),
                AttendanceMarkedAt = a.MarkedAt
            })
            .ToListAsync(cancellationToken);

        var attendanceLookup = attendances.ToDictionary(
            attendance => (attendance.StudentProfileId, attendance.SessionId),
            attendance => attendance);
        var approvedLeavePairs = await approvedLeaveAttendanceService.GetApprovedLeavePairsAsync(
            sessionMetadata.Keys
                .Where(entry => sessionDetails.ContainsKey(entry.SessionId))
                .Select(entry =>
                {
                    var session = sessionDetails[entry.SessionId];
                    return new ApprovedLeaveSessionCandidate(
                        entry.StudentProfileId,
                        entry.SessionId,
                        session.ClassId,
                        VietnamTime.ToVietnamDateOnly(session.PlannedDatetime));
                }),
            cancellationToken);

        var sessions = sessionMetadata
            .Where(entry => sessionDetails.ContainsKey(entry.Key.SessionId))
            .Select(entry =>
            {
                var student = studentLookup[entry.Key.StudentProfileId];
                var session = sessionDetails[entry.Key.SessionId];
                attendanceLookup.TryGetValue((entry.Key.StudentProfileId, entry.Key.SessionId), out var attendance);
                var hasApprovedLeave = approvedLeavePairs.Contains((entry.Key.StudentProfileId, entry.Key.SessionId));

                return new TimetableItemDto
                {
                    Id = session.Id,
                    StudentProfileId = student.Id,
                    StudentDisplayName = student.DisplayName,
                    StudentAvatarUrl = student.AvatarUrl,
                    Color = session.Color,
                    ClassId = session.ClassId,
                    ClassCode = session.ClassCode,
                    ClassTitle = session.ClassTitle,
                    PlannedDatetime = VietnamTime.ToVietnamDateTime(session.PlannedDatetime),
                    ActualDatetime = session.ActualDatetime.HasValue
                        ? VietnamTime.ToVietnamDateTime(session.ActualDatetime.Value)
                        : null,
                    DurationMinutes = session.DurationMinutes,
                    ParticipationType = session.ParticipationType,
                    Status = session.Status,
                    PlannedRoomId = session.PlannedRoomId,
                    PlannedRoomName = session.PlannedRoomName,
                    ActualRoomId = session.ActualRoomId,
                    ActualRoomName = session.ActualRoomName,
                    PlannedTeacherId = session.PlannedTeacherId,
                    PlannedTeacherName = session.PlannedTeacherName,
                    ActualTeacherId = session.ActualTeacherId,
                    ActualTeacherName = session.ActualTeacherName,
                    PlannedAssistantId = session.PlannedAssistantId,
                    PlannedAssistantName = session.PlannedAssistantName,
                    LessonPlanId = session.LessonPlanId,
                    LessonPlanLink = session.LessonPlanLink,
                    RegistrationId = entry.Value.RegistrationId,
                    Track = entry.Value.Track,
                    IsMakeup = entry.Value.IsMakeup,
                    AttendanceStatus = hasApprovedLeave
                        ? AttendanceStatus.Makeup.ToString()
                        : attendance?.AttendanceStatus,
                    AbsenceType = hasApprovedLeave ? null : attendance?.AbsenceType,
                    AttendanceMarkedAt = hasApprovedLeave
                        ? null
                        : attendance?.AttendanceMarkedAt is { } markedAt
                            ? VietnamTime.ToVietnamDateTime(markedAt)
                            : null
                };
            })
            .OrderBy(s => s.PlannedDatetime)
            .ThenBy(s => s.StudentDisplayName)
            .ToList();

        return new GetStudentTimetableResponse
        {
            Sessions = sessions
        };
    }

    private async Task<Dictionary<Guid, List<EnrollmentPauseWindow>>> BuildPauseLookupAsync(
        IEnumerable<Guid> enrollmentIds,
        CancellationToken cancellationToken)
    {
        var enrollmentIdList = enrollmentIds
            .Distinct()
            .ToList();

        if (enrollmentIdList.Count == 0)
        {
            return new Dictionary<Guid, List<EnrollmentPauseWindow>>();
        }

        var pauseWindows = await context.PauseEnrollmentRequestHistories
            .AsNoTracking()
            .Where(history => history.EnrollmentId.HasValue
                && enrollmentIdList.Contains(history.EnrollmentId.Value)
                && history.NewStatus == EnrollmentStatus.Paused)
            .Select(history => new EnrollmentPauseWindow(
                history.EnrollmentId!.Value,
                history.PauseFrom,
                history.PauseTo))
            .ToListAsync(cancellationToken);

        return EnrollmentPauseWindowHelper.BuildLookup(pauseWindows);
    }

    private static (DateTime? FromUtc, DateTime? ToUtc) NormalizeDateRange(GetStudentTimetableQuery query)
    {
        DateTime? fromUtc = null;
        if (query.From.HasValue)
        {
            fromUtc = VietnamTime.NormalizeToUtc(query.From.Value);
        }

        DateTime? toUtc = null;
        if (query.To.HasValue)
        {
            var normalizedToUtc = VietnamTime.NormalizeToUtc(query.To.Value);
            toUtc = VietnamTime.EndOfVietnamDayUtc(normalizedToUtc);
        }

        return (fromUtc, toUtc);
    }

    private sealed record StudentTimetableTarget(
        Guid Id,
        string DisplayName,
        string? AvatarUrl);

    private sealed record SessionMetadata(
        Guid? RegistrationId,
        string? Track,
        bool IsMakeup);

    private sealed record SessionMetadataAssignment(
        Guid StudentProfileId,
        Guid SessionId,
        Guid? RegistrationId,
        Domain.Registrations.RegistrationTrackType Track);
}

