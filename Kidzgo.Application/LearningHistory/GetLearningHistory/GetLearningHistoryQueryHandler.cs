using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LearningHistory.GetLearningHistory;

public sealed class GetLearningHistoryQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetLearningHistoryQuery, GetLearningHistoryResponse>
{
    public async Task<Result<GetLearningHistoryResponse>> Handle(
        GetLearningHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var studentResult = await ResolveStudentProfileAsync(query.StudentProfileId, cancellationToken);
        if (!studentResult.IsSuccess)
        {
            return Result.Failure<GetLearningHistoryResponse>(studentResult.Error);
        }

        var student = studentResult.Value;
        var studentId = student.Id;
        var (fromUtc, toUtc) = NormalizeDateRange(query);

        var registrations = await context.Registrations
            .AsNoTracking()
            .Where(registration => registration.StudentProfileId == studentId)
            .OrderByDescending(registration => registration.RegistrationDate)
            .Select(registration => new LearningHistoryRegistrationDto
            {
                Id = registration.Id,
                RegistrationDate = VietnamTime.ToVietnamDateTime(registration.RegistrationDate),
                Status = registration.Status.ToString(),
                OperationType = registration.OperationType.HasValue ? registration.OperationType.Value.ToString() : null,
                BranchId = registration.BranchId,
                BranchName = registration.Branch.Name,
                ProgramId = registration.ProgramId,
                ProgramName = registration.Program.Name,
                SecondaryProgramId = registration.SecondaryProgramId,
                SecondaryProgramName = registration.SecondaryProgram != null ? registration.SecondaryProgram.Name : null,
                TuitionPlanId = registration.TuitionPlanId,
                TuitionPlanName = registration.TuitionPlan.Name,
                ClassId = registration.ClassId,
                ClassName = registration.Class != null ? registration.Class.Title : null,
                SecondaryClassId = registration.SecondaryClassId,
                SecondaryClassName = registration.SecondaryClass != null ? registration.SecondaryClass.Title : null,
                ExpectedStartDate = registration.ExpectedStartDate.HasValue
                    ? VietnamTime.ToVietnamDateTime(registration.ExpectedStartDate.Value)
                    : null,
                ActualStartDate = registration.ActualStartDate.HasValue
                    ? VietnamTime.ToVietnamDateTime(registration.ActualStartDate.Value)
                    : null,
                TotalSessions = registration.TotalSessions,
                UsedSessions = registration.UsedSessions,
                RemainingSessions = registration.RemainingSessions,
                OriginalRegistrationId = registration.OriginalRegistrationId,
                UpdatedAt = VietnamTime.ToVietnamDateTime(registration.UpdatedAt)
            })
            .ToListAsync(cancellationToken);

        var enrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Where(enrollment => enrollment.StudentProfileId == studentId)
            .OrderByDescending(enrollment => enrollment.EnrollDate)
            .Select(enrollment => new LearningHistoryEnrollmentDto
            {
                Id = enrollment.Id,
                RegistrationId = enrollment.RegistrationId,
                ClassId = enrollment.ClassId,
                ClassCode = enrollment.Class.Code,
                ClassTitle = enrollment.Class.Title,
                ProgramId = enrollment.Class.ProgramId,
                ProgramName = enrollment.Class.Program.Name,
                BranchId = enrollment.Class.BranchId,
                BranchName = enrollment.Class.Branch.Name,
                Track = RegistrationTrackHelper.ToTrackName(enrollment.Track),
                EnrollDate = enrollment.EnrollDate,
                Status = enrollment.Status,
                CreatedAt = VietnamTime.ToVietnamDateTime(enrollment.CreatedAt),
                UpdatedAt = VietnamTime.ToVietnamDateTime(enrollment.UpdatedAt)
            })
            .ToListAsync(cancellationToken);

        var sessionItems = await BuildSessionTimelineAsync(studentId, fromUtc, toUtc, cancellationToken);
        var pagedSessions = sessionItems
            .OrderByDescending(session => session.PlannedDatetime)
            .ThenByDescending(session => session.SessionId)
            .ToList();

        var sessionPageItems = pagedSessions
            .Skip((query.SessionPageNumber - 1) * query.SessionPageSize)
            .Take(query.SessionPageSize)
            .ToList();

        var missionQuery = context.MissionProgresses
            .AsNoTracking()
            .Include(progress => progress.Mission)
            .Where(progress => progress.StudentProfileId == studentId);

        if (fromUtc.HasValue)
        {
            missionQuery = missionQuery.Where(progress => progress.Mission.CreatedAt >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            missionQuery = missionQuery.Where(progress => progress.Mission.CreatedAt <= toUtc.Value);
        }

        var totalMissions = await missionQuery.CountAsync(cancellationToken);
        var missionItems = await missionQuery
            .OrderByDescending(progress => progress.Mission.CreatedAt)
            .ThenByDescending(progress => progress.Id)
            .Skip((query.MissionPageNumber - 1) * query.MissionPageSize)
            .Take(query.MissionPageSize)
            .Select(progress => new LearningHistoryMissionDto
            {
                Id = progress.Id,
                MissionId = progress.MissionId,
                Title = progress.Mission.Title,
                Description = progress.Mission.Description,
                MissionType = progress.Mission.MissionType.ToString(),
                ProgressMode = progress.Mission.ProgressMode.ToString(),
                Status = progress.Status.ToString(),
                ProgressValue = progress.ProgressValue,
                TotalRequired = progress.Mission.TotalRequired,
                ProgressPercentage = progress.Mission.TotalRequired.HasValue && progress.Mission.TotalRequired.Value > 0
                    ? (progress.ProgressValue ?? 0) * 100m / progress.Mission.TotalRequired.Value
                    : (progress.ProgressValue.HasValue ? 100m : 0m),
                RewardStars = progress.Mission.RewardStars,
                RewardExp = progress.Mission.RewardExp,
                StartAt = progress.Mission.StartAt.HasValue
                    ? VietnamTime.ToVietnamDateTime(progress.Mission.StartAt.Value)
                    : null,
                EndAt = progress.Mission.EndAt.HasValue
                    ? VietnamTime.ToVietnamDateTime(progress.Mission.EndAt.Value)
                    : null,
                CreatedAt = VietnamTime.ToVietnamDateTime(progress.Mission.CreatedAt),
                CompletedAt = progress.CompletedAt.HasValue
                    ? VietnamTime.ToVietnamDateTime(progress.CompletedAt.Value)
                    : null
            })
            .ToListAsync(cancellationToken);

        var completedMissionCount = await context.MissionProgresses
            .AsNoTracking()
            .CountAsync(
                progress => progress.StudentProfileId == studentId &&
                            progress.Status == MissionProgressStatus.Completed,
                cancellationToken);

        return Result.Success(new GetLearningHistoryResponse
        {
            StudentProfileId = student.Id,
            StudentName = student.DisplayName,
            Summary = new LearningHistorySummaryDto
            {
                TotalRegistrations = registrations.Count,
                CompletedRegistrations = registrations.Count(registration => registration.Status == Domain.Registrations.RegistrationStatus.Completed.ToString()),
                TotalPurchasedSessions = registrations.Sum(registration => registration.TotalSessions),
                TotalUsedSessions = registrations.Sum(registration => registration.UsedSessions),
                TotalRemainingSessions = registrations.Sum(registration => registration.RemainingSessions),
                TotalEnrollments = enrollments.Count,
                CompletedEnrollments = enrollments.Count(enrollment => enrollment.Status == Domain.Classes.EnrollmentStatus.Completed),
                TotalSessionRecords = sessionItems.Count,
                PresentSessions = sessionItems.Count(session => session.AttendanceStatus == AttendanceStatus.Present.ToString()),
                AbsentSessions = sessionItems.Count(session => session.AttendanceStatus == AttendanceStatus.Absent.ToString()),
                MakeupSessions = sessionItems.Count(session => session.IsMakeup || session.AttendanceStatus == AttendanceStatus.Makeup.ToString()),
                TotalMissions = await context.MissionProgresses
                    .AsNoTracking()
                    .CountAsync(progress => progress.StudentProfileId == studentId, cancellationToken),
                CompletedMissions = completedMissionCount
            },
            Registrations = registrations,
            Enrollments = enrollments,
            Sessions = new Page<LearningHistorySessionDto>(
                sessionPageItems,
                pagedSessions.Count,
                query.SessionPageNumber,
                query.SessionPageSize),
            Missions = new Page<LearningHistoryMissionDto>(
                missionItems,
                totalMissions,
                query.MissionPageNumber,
                query.MissionPageSize)
        });
    }

    private async Task<Result<Profile>> ResolveStudentProfileAsync(Guid? requestedStudentProfileId, CancellationToken cancellationToken)
    {
        if (userContext.ParentId.HasValue)
        {
            var parentProfileResult = await ParentRegistrationAccessHelper.ResolveParentProfileIdAsync(
                context,
                userContext,
                cancellationToken);
            if (!parentProfileResult.IsSuccess)
            {
                return Result.Failure<Profile>(parentProfileResult.Error);
            }

            var studentIdResult = await ParentRegistrationAccessHelper.ResolveTargetStudentIdAsync(
                context,
                userContext,
                parentProfileResult.Value,
                requestedStudentProfileId,
                cancellationToken);
            if (!studentIdResult.IsSuccess)
            {
                return Result.Failure<Profile>(studentIdResult.Error);
            }

            var profile = await context.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(profile =>
                    profile.Id == studentIdResult.Value &&
                    profile.ProfileType == ProfileType.Student &&
                    profile.IsActive &&
                    !profile.IsDeleted,
                    cancellationToken);

            return profile is null
                ? Result.Failure<Profile>(ProfileErrors.StudentNotFound)
                : Result.Success(profile);
        }

        if (!userContext.StudentId.HasValue)
        {
            return Result.Failure<Profile>(ProfileErrors.StudentNotFound);
        }

        if (requestedStudentProfileId.HasValue && requestedStudentProfileId.Value != userContext.StudentId.Value)
        {
            return Result.Failure<Profile>(ProfileErrors.StudentNotFound);
        }

        var studentProfile = await context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile =>
                profile.Id == userContext.StudentId.Value &&
                profile.UserId == userContext.UserId &&
                profile.ProfileType == ProfileType.Student &&
                profile.IsActive &&
                !profile.IsDeleted,
                cancellationToken);

        return studentProfile is null
            ? Result.Failure<Profile>(ProfileErrors.StudentNotFound)
            : Result.Success(studentProfile);
    }

    private async Task<List<LearningHistorySessionDto>> BuildSessionTimelineAsync(
        Guid studentProfileId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var regularAssignmentsQuery = context.StudentSessionAssignments
            .AsNoTracking()
            .Where(assignment =>
                assignment.StudentProfileId == studentProfileId &&
                assignment.Status == StudentSessionAssignmentStatus.Assigned &&
                assignment.Session.Status != SessionStatus.Cancelled);

        if (fromUtc.HasValue)
        {
            regularAssignmentsQuery = regularAssignmentsQuery
                .Where(assignment => assignment.Session.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            regularAssignmentsQuery = regularAssignmentsQuery
                .Where(assignment => assignment.Session.PlannedDatetime <= toUtc.Value);
        }

        var regularAssignments = await regularAssignmentsQuery
            .Select(assignment => new
            {
                assignment.SessionId,
                assignment.RegistrationId,
                assignment.Track
            })
            .ToListAsync(cancellationToken);

        var makeupAllocationsQuery = context.MakeupAllocations
            .AsNoTracking()
            .Where(allocation =>
                allocation.MakeupCredit.StudentProfileId == studentProfileId &&
                allocation.Status != MakeupAllocationStatus.Cancelled &&
                allocation.TargetSession.Status != SessionStatus.Cancelled);

        if (fromUtc.HasValue)
        {
            makeupAllocationsQuery = makeupAllocationsQuery
                .Where(allocation => allocation.TargetSession.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            makeupAllocationsQuery = makeupAllocationsQuery
                .Where(allocation => allocation.TargetSession.PlannedDatetime <= toUtc.Value);
        }

        var makeupAllocations = await makeupAllocationsQuery
            .Select(allocation => allocation.TargetSessionId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var attendancesQuery = context.Attendances
            .AsNoTracking()
            .Where(attendance => attendance.StudentProfileId == studentProfileId);

        if (fromUtc.HasValue)
        {
            attendancesQuery = attendancesQuery
                .Where(attendance => attendance.Session.PlannedDatetime >= fromUtc.Value);
        }

        if (toUtc.HasValue)
        {
            attendancesQuery = attendancesQuery
                .Where(attendance => attendance.Session.PlannedDatetime <= toUtc.Value);
        }

        var attendanceRows = await attendancesQuery
            .Select(attendance => new
            {
                attendance.SessionId,
                AttendanceStatus = attendance.AttendanceStatus.ToString(),
                AbsenceType = attendance.AbsenceType.HasValue ? attendance.AbsenceType.Value.ToString() : null,
                attendance.MarkedAt,
                attendance.Note
            })
            .ToListAsync(cancellationToken);

        var attendanceLookup = attendanceRows.ToDictionary(row => row.SessionId);
        var sessionMetadata = regularAssignments
            .GroupBy(assignment => assignment.SessionId)
            .ToDictionary(
                group => group.Key,
                group =>
                {
                    var selected = group
                        .OrderByDescending(assignment => assignment.RegistrationId.HasValue)
                        .First();

                    return new SessionMetadata(
                        selected.RegistrationId,
                        RegistrationTrackHelper.ToTrackName(selected.Track),
                        false);
                });

        foreach (var sessionId in makeupAllocations)
        {
            if (sessionMetadata.TryGetValue(sessionId, out var existingMetadata))
            {
                sessionMetadata[sessionId] = existingMetadata with { IsMakeup = true };
                continue;
            }

            sessionMetadata[sessionId] = new SessionMetadata(null, null, true);
        }

        foreach (var attendance in attendanceRows)
        {
            sessionMetadata.TryAdd(
                attendance.SessionId,
                new SessionMetadata(
                    null,
                    null,
                    attendance.AttendanceStatus == AttendanceStatus.Makeup.ToString()));
        }

        var sessionIds = sessionMetadata.Keys.ToList();
        if (sessionIds.Count == 0)
        {
            return new List<LearningHistorySessionDto>();
        }

        var sessionDetails = await context.Sessions
            .AsNoTracking()
            .Where(session => sessionIds.Contains(session.Id))
            .Select(session => new
            {
                session.Id,
                session.ClassId,
                ClassCode = session.Class.Code,
                ClassTitle = session.Class.Title,
                session.PlannedDatetime,
                session.ActualDatetime,
                session.DurationMinutes,
                SessionStatus = session.Status.ToString(),
                TeacherId = session.ActualTeacherId ?? session.PlannedTeacherId,
                TeacherName = session.ActualTeacher != null
                    ? session.ActualTeacher.Name
                    : session.PlannedTeacher != null
                        ? session.PlannedTeacher.Name
                        : null,
                RoomId = session.ActualRoomId ?? session.PlannedRoomId,
                RoomName = session.ActualRoom != null
                    ? session.ActualRoom.Name
                    : session.PlannedRoom != null
                        ? session.PlannedRoom.Name
                        : null
            })
            .ToDictionaryAsync(session => session.Id, cancellationToken);

        return sessionMetadata
            .Where(entry => sessionDetails.ContainsKey(entry.Key))
            .Select(entry =>
            {
                var session = sessionDetails[entry.Key];
                attendanceLookup.TryGetValue(entry.Key, out var attendance);

                return new LearningHistorySessionDto
                {
                    SessionId = session.Id,
                    ClassId = session.ClassId,
                    ClassCode = session.ClassCode,
                    ClassTitle = session.ClassTitle,
                    PlannedDatetime = VietnamTime.ToVietnamDateTime(session.PlannedDatetime),
                    ActualDatetime = session.ActualDatetime.HasValue
                        ? VietnamTime.ToVietnamDateTime(session.ActualDatetime.Value)
                        : null,
                    DurationMinutes = session.DurationMinutes,
                    SessionStatus = session.SessionStatus,
                    RegistrationId = entry.Value.RegistrationId,
                    Track = entry.Value.Track,
                    IsMakeup = entry.Value.IsMakeup,
                    AttendanceStatus = attendance?.AttendanceStatus,
                    AbsenceType = attendance?.AbsenceType,
                    AttendanceMarkedAt = attendance?.MarkedAt.HasValue == true
                        ? VietnamTime.ToVietnamDateTime(attendance.MarkedAt.Value)
                        : null,
                    AttendanceNote = attendance?.Note,
                    TeacherId = session.TeacherId,
                    TeacherName = session.TeacherName,
                    RoomId = session.RoomId,
                    RoomName = session.RoomName
                };
            })
            .ToList();
    }

    private static (DateTime? FromUtc, DateTime? ToUtc) NormalizeDateRange(GetLearningHistoryQuery query)
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

    private sealed record SessionMetadata(
        Guid? RegistrationId,
        string? Track,
        bool IsMakeup);
}
