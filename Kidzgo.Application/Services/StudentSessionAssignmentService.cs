using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Services;

public readonly record struct RegularSessionParticipant(
    Guid StudentProfileId,
    Guid? ClassEnrollmentId,
    Guid? RegistrationId,
    RegistrationTrackType Track);

public sealed class StudentSessionAssignmentService(
    IDbContext context,
    ISchedulePatternParser patternParser)
{
    public Result ValidateSelectionPattern(Class classEntity, string? sessionSelectionPattern)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return Result.Success();
        }

        var validationEndDate = classEntity.EndDate ?? classEntity.StartDate.AddMonths(3);
        var selectionResult = patternParser.ParseAndGenerateOccurrences(
            sessionSelectionPattern,
            classEntity.StartDate,
            validationEndDate);

        if (selectionResult.IsFailure)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternInvalid",
                selectionResult.Error.Description));
        }

        if (selectionResult.Value.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternEmpty",
                "Session selection pattern does not match any session slot in the validation range."));
        }

        if (string.IsNullOrWhiteSpace(classEntity.SchedulePattern))
        {
            return Result.Success();
        }

        var classResult = patternParser.ParseAndGenerateOccurrences(
            classEntity.SchedulePattern,
            classEntity.StartDate,
            validationEndDate);

        if (classResult.IsFailure)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.ClassSchedulePatternInvalid",
                classResult.Error.Description));
        }

        var classOccurrences = classResult.Value
            .Select(ToMinuteKey)
            .ToHashSet();

        var isSubset = selectionResult.Value
            .Select(ToMinuteKey)
            .All(classOccurrences.Contains);

        if (!isSubset)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternMismatch",
                "Session selection pattern must be a subset of the class schedule pattern."));
        }

        return Result.Success();
    }

    public async Task<Result> ValidateSelectionPatternForPeriodAsync(
        Class classEntity,
        string? sessionSelectionPattern,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionSelectionPattern))
        {
            return Result.Success();
        }

        var validationStartDate = effectiveFrom > classEntity.StartDate
            ? effectiveFrom
            : classEntity.StartDate;
        var validationEndDate = effectiveTo
            ?? classEntity.EndDate
            ?? validationStartDate.AddMonths(3);

        if (validationEndDate < validationStartDate)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.ScheduleSegmentInvalidEffectiveDate",
                "Schedule segment effective range is invalid."));
        }

        var selectionResult = patternParser.ParseAndGenerateOccurrences(
            sessionSelectionPattern,
            validationStartDate,
            validationEndDate);

        if (selectionResult.IsFailure)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternInvalid",
                selectionResult.Error.Description));
        }

        if (selectionResult.Value.Count == 0)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternEmpty",
                "Session selection pattern does not match any session slot in the validation range."));
        }

        var classOccurrencesResult = await GetClassScheduleOccurrencesForPeriodAsync(
            classEntity,
            validationStartDate,
            validationEndDate,
            cancellationToken);

        if (classOccurrencesResult.IsFailure)
        {
            return Result.Failure(classOccurrencesResult.Error);
        }

        if (classOccurrencesResult.Value.Count == 0)
        {
            return Result.Success();
        }

        var classOccurrences = classOccurrencesResult.Value
            .Select(ToMinuteKey)
            .ToHashSet();

        var isSubset = selectionResult.Value
            .Select(ToMinuteKey)
            .All(classOccurrences.Contains);

        if (!isSubset)
        {
            return Result.Failure(Error.Validation(
                "Enrollment.SessionSelectionPatternMismatch",
                "Session selection pattern must be a subset of the class schedule pattern."));
        }

        return Result.Success();
    }

    public async Task SyncAssignmentsForEnrollmentAsync(
        ClassEnrollment enrollment,
        CancellationToken cancellationToken)
    {
        var sessionDateFrom = enrollment.EnrollDate;
        var sessions = await context.Sessions
            .Where(s => s.ClassId == enrollment.ClassId)
            .ToListAsync(cancellationToken);

        var existingAssignments = await context.StudentSessionAssignments
            .Where(a => a.ClassEnrollmentId == enrollment.Id)
            .ToListAsync(cancellationToken);
        var pauseLookup = await GetPauseLookupAsync(new[] { enrollment.Id }, cancellationToken);
        var scheduleSegmentLookup = await GetEnrollmentScheduleSegmentLookupAsync(new[] { enrollment.Id }, cancellationToken);

        var assignmentsBySessionId = existingAssignments.ToDictionary(a => a.SessionId);
        var now = VietnamTime.UtcNow();

        foreach (var session in sessions)
        {
            var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);
            var sessionSelectionPattern = ResolveSessionSelectionPattern(
                enrollment,
                sessionDate,
                scheduleSegmentLookup);
            var shouldBeAssigned =
                session.Status != SessionStatus.Cancelled &&
                sessionDate >= sessionDateFrom &&
                !EnrollmentPauseWindowHelper.IsPausedOn(enrollment.Id, sessionDate, pauseLookup) &&
                MatchesSelectionPattern(session, sessionSelectionPattern);

            assignmentsBySessionId.TryGetValue(session.Id, out var assignment);

            if (shouldBeAssigned)
            {
                if (assignment == null)
                {
                    context.StudentSessionAssignments.Add(new StudentSessionAssignment
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        StudentProfileId = enrollment.StudentProfileId,
                        ClassEnrollmentId = enrollment.Id,
                        RegistrationId = enrollment.RegistrationId,
                        Track = enrollment.Track,
                        Status = StudentSessionAssignmentStatus.Assigned,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    assignment.StudentProfileId = enrollment.StudentProfileId;
                    assignment.RegistrationId = enrollment.RegistrationId;
                    assignment.Track = enrollment.Track;
                    assignment.Status = StudentSessionAssignmentStatus.Assigned;
                    assignment.UpdatedAt = now;
                }
            }
            else if (assignment is not null && assignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                assignment.Status = StudentSessionAssignmentStatus.Cancelled;
                assignment.UpdatedAt = now;
            }
        }
    }

    public async Task RestoreAssignmentsForEnrollmentAsync(
        ClassEnrollment enrollment,
        DateOnly effectiveFrom,
        CancellationToken cancellationToken)
    {
        var sessionDateFrom = effectiveFrom > enrollment.EnrollDate
            ? effectiveFrom
            : enrollment.EnrollDate;
        var sessionDateFromUtc = VietnamTime.TreatAsVietnamLocal(sessionDateFrom.ToDateTime(TimeOnly.MinValue));

        var sessions = await context.Sessions
            .Where(s => s.ClassId == enrollment.ClassId &&
                        s.PlannedDatetime >= sessionDateFromUtc)
            .ToListAsync(cancellationToken);

        if (sessions.Count == 0)
        {
            return;
        }

        var sessionIds = sessions.Select(s => s.Id).ToList();
        var existingAssignments = await context.StudentSessionAssignments
            .Where(a => a.ClassEnrollmentId == enrollment.Id && sessionIds.Contains(a.SessionId))
            .ToListAsync(cancellationToken);

        var assignmentsBySessionId = existingAssignments.ToDictionary(a => a.SessionId);
        var scheduleSegmentLookup = await GetEnrollmentScheduleSegmentLookupAsync(new[] { enrollment.Id }, cancellationToken);
        var now = VietnamTime.UtcNow();

        foreach (var session in sessions)
        {
            var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);
            var sessionSelectionPattern = ResolveSessionSelectionPattern(
                enrollment,
                sessionDate,
                scheduleSegmentLookup);
            var shouldBeAssigned =
                session.Status != SessionStatus.Cancelled &&
                MatchesSelectionPattern(session, sessionSelectionPattern);

            assignmentsBySessionId.TryGetValue(session.Id, out var assignment);

            if (shouldBeAssigned)
            {
                if (assignment == null)
                {
                    context.StudentSessionAssignments.Add(new StudentSessionAssignment
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        StudentProfileId = enrollment.StudentProfileId,
                        ClassEnrollmentId = enrollment.Id,
                        RegistrationId = enrollment.RegistrationId,
                        Track = enrollment.Track,
                        Status = StudentSessionAssignmentStatus.Assigned,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    assignment.StudentProfileId = enrollment.StudentProfileId;
                    assignment.RegistrationId = enrollment.RegistrationId;
                    assignment.Track = enrollment.Track;
                    assignment.Status = StudentSessionAssignmentStatus.Assigned;
                    assignment.UpdatedAt = now;
                }
            }
            else if (assignment is not null && assignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                assignment.Status = StudentSessionAssignmentStatus.Cancelled;
                assignment.UpdatedAt = now;
            }
        }
    }

    public async Task SyncAssignmentsForSessionAsync(
        Session session,
        CancellationToken cancellationToken)
    {
        var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);
        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.ClassId == session.ClassId && e.Status == EnrollmentStatus.Active)
            .ToListAsync(cancellationToken);
        var pauseLookup = await GetPauseLookupAsync(
            activeEnrollments.Select(e => e.Id),
            cancellationToken);
        var scheduleSegmentLookup = await GetEnrollmentScheduleSegmentLookupAsync(
            activeEnrollments.Select(e => e.Id),
            cancellationToken);

        var existingAssignments = await context.StudentSessionAssignments
            .Where(a => a.SessionId == session.Id)
            .ToListAsync(cancellationToken);

        var assignmentsByEnrollmentId = existingAssignments.ToDictionary(a => a.ClassEnrollmentId);
        var activeEnrollmentIds = activeEnrollments.Select(e => e.Id).ToHashSet();
        var now = VietnamTime.UtcNow();

        foreach (var enrollment in activeEnrollments)
        {
            var shouldBeAssigned =
                session.Status != SessionStatus.Cancelled &&
                sessionDate >= enrollment.EnrollDate &&
                !EnrollmentPauseWindowHelper.IsPausedOn(enrollment.Id, sessionDate, pauseLookup) &&
                MatchesSelectionPattern(
                    session,
                    ResolveSessionSelectionPattern(enrollment, sessionDate, scheduleSegmentLookup));

            assignmentsByEnrollmentId.TryGetValue(enrollment.Id, out var assignment);

            if (shouldBeAssigned)
            {
                if (assignment == null)
                {
                    context.StudentSessionAssignments.Add(new StudentSessionAssignment
                    {
                        Id = Guid.NewGuid(),
                        SessionId = session.Id,
                        StudentProfileId = enrollment.StudentProfileId,
                        ClassEnrollmentId = enrollment.Id,
                        RegistrationId = enrollment.RegistrationId,
                        Track = enrollment.Track,
                        Status = StudentSessionAssignmentStatus.Assigned,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    assignment.StudentProfileId = enrollment.StudentProfileId;
                    assignment.RegistrationId = enrollment.RegistrationId;
                    assignment.Track = enrollment.Track;
                    assignment.Status = StudentSessionAssignmentStatus.Assigned;
                    assignment.UpdatedAt = now;
                }
            }
            else if (assignment is not null && assignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                assignment.Status = StudentSessionAssignmentStatus.Cancelled;
                assignment.UpdatedAt = now;
            }
        }

        foreach (var orphanedAssignment in existingAssignments.Where(a => !activeEnrollmentIds.Contains(a.ClassEnrollmentId)))
        {
            if (orphanedAssignment.Status != StudentSessionAssignmentStatus.Cancelled)
            {
                orphanedAssignment.Status = StudentSessionAssignmentStatus.Cancelled;
                orphanedAssignment.UpdatedAt = now;
            }
        }
    }

    public async Task CancelFutureAssignmentsForEnrollmentAsync(
        Guid enrollmentId,
        DateTime effectiveFromUtc,
        CancellationToken cancellationToken)
    {
        var assignments = await context.StudentSessionAssignments
            .Include(a => a.Session)
            .Where(a => a.ClassEnrollmentId == enrollmentId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.PlannedDatetime >= effectiveFromUtc)
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        foreach (var assignment in assignments)
        {
            assignment.Status = StudentSessionAssignmentStatus.Cancelled;
            assignment.UpdatedAt = now;
        }
    }

    public async Task<int> CancelAssignmentsForEnrollmentInRangeAsync(
        Guid enrollmentId,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken)
    {
        var assignments = await context.StudentSessionAssignments
            .Include(a => a.Session)
            .Where(a => a.ClassEnrollmentId == enrollmentId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.PlannedDatetime >= fromUtc
                && a.Session.PlannedDatetime <= toUtc)
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        foreach (var assignment in assignments)
        {
            assignment.Status = StudentSessionAssignmentStatus.Cancelled;
            assignment.UpdatedAt = now;
        }

        return assignments.Count;
    }

    public async Task ReassignFutureAssignmentsToRegistrationAsync(
        Guid oldRegistrationId,
        Guid newRegistrationId,
        DateTime effectiveFromUtc,
        CancellationToken cancellationToken)
    {
        var assignments = await context.StudentSessionAssignments
            .Include(a => a.Session)
            .Where(a => a.RegistrationId == oldRegistrationId
                && a.Status == StudentSessionAssignmentStatus.Assigned
                && a.Session.PlannedDatetime >= effectiveFromUtc)
            .ToListAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        foreach (var assignment in assignments)
        {
            assignment.RegistrationId = newRegistrationId;
            assignment.UpdatedAt = now;
        }
    }

    public async Task<bool> HasRegularAssignmentsAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        return await context.StudentSessionAssignments
            .AnyAsync(a => a.SessionId == sessionId, cancellationToken);
    }

    public async Task<bool> IsStudentRegularlyAssignedToSessionAsync(
        Guid sessionId,
        Guid studentProfileId,
        CancellationToken cancellationToken)
    {
        var participants = await GetRegularParticipantsAsync(sessionId, cancellationToken);
        return participants.Any(p => p.StudentProfileId == studentProfileId);
    }

    public async Task<int> GetRegularAssignedStudentCountAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var participants = await GetRegularParticipantsAsync(sessionId, cancellationToken);
        return participants.Count;
    }

    public async Task<List<RegularSessionParticipant>> GetRegularParticipantsAsync(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var hasAssignments = await HasRegularAssignmentsAsync(sessionId, cancellationToken);

        if (hasAssignments)
        {
            return await context.StudentSessionAssignments
                .AsNoTracking()
                .Where(a => a.SessionId == sessionId && a.Status == StudentSessionAssignmentStatus.Assigned)
                .Select(a => new RegularSessionParticipant(
                    a.StudentProfileId,
                    a.ClassEnrollmentId,
                    a.RegistrationId,
                    a.Track))
                .ToListAsync(cancellationToken);
        }

        var sessionInfo = await context.Sessions
            .AsNoTracking()
            .Where(s => s.Id == sessionId)
            .Select(s => new
            {
                s.ClassId,
                s.PlannedDatetime
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (sessionInfo == null)
        {
            return new List<RegularSessionParticipant>();
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(sessionInfo.PlannedDatetime);
        var pauseLookup = await GetPauseLookupAsync(
            await context.ClassEnrollments
                .AsNoTracking()
                .Where(e => e.ClassId == sessionInfo.ClassId
                    && e.Status == EnrollmentStatus.Active
                    && e.EnrollDate <= sessionDate)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken),
            cancellationToken);

        var activeEnrollments = await context.ClassEnrollments
            .AsNoTracking()
            .Where(e => e.ClassId == sessionInfo.ClassId
                && e.Status == EnrollmentStatus.Active
                && e.EnrollDate <= sessionDate)
            .ToListAsync(cancellationToken);
        var scheduleSegmentLookup = await GetEnrollmentScheduleSegmentLookupAsync(
            activeEnrollments.Select(e => e.Id),
            cancellationToken);

        return activeEnrollments
            .Where(e => !EnrollmentPauseWindowHelper.IsPausedOn(e.Id, sessionDate, pauseLookup) &&
                MatchesSelectionPattern(
                    new Session { PlannedDatetime = sessionInfo.PlannedDatetime },
                    ResolveSessionSelectionPattern(e, sessionDate, scheduleSegmentLookup)))
            .Select(e => new RegularSessionParticipant(
                e.StudentProfileId,
                e.Id,
                e.RegistrationId,
                e.Track))
            .ToList();
    }

    private async Task<Dictionary<Guid, List<EnrollmentPauseWindow>>> GetPauseLookupAsync(
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

    private async Task<Result<List<DateTime>>> GetClassScheduleOccurrencesForPeriodAsync(
        Class classEntity,
        DateOnly validationStartDate,
        DateOnly validationEndDate,
        CancellationToken cancellationToken)
    {
        var scheduleSegments = await context.ClassScheduleSegments
            .AsNoTracking()
            .Where(segment => segment.ClassId == classEntity.Id)
            .OrderBy(segment => segment.EffectiveFrom)
            .ToListAsync(cancellationToken);

        if (scheduleSegments.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(classEntity.SchedulePattern))
            {
                return Result.Success(new List<DateTime>());
            }

            var classResult = patternParser.ParseAndGenerateOccurrences(
                classEntity.SchedulePattern,
                validationStartDate,
                validationEndDate);

            if (classResult.IsFailure)
            {
                return Result.Failure<List<DateTime>>(Error.Validation(
                    "Enrollment.ClassSchedulePatternInvalid",
                    classResult.Error.Description));
            }

            return Result.Success(classResult.Value);
        }

        var occurrences = new List<DateTime>();
        foreach (var segment in scheduleSegments)
        {
            if (segment.EffectiveFrom > validationEndDate ||
                (segment.EffectiveTo.HasValue && segment.EffectiveTo.Value < validationStartDate))
            {
                continue;
            }

            var segmentStart = segment.EffectiveFrom > validationStartDate
                ? segment.EffectiveFrom
                : validationStartDate;
            var segmentEnd = segment.EffectiveTo.HasValue && segment.EffectiveTo.Value < validationEndDate
                ? segment.EffectiveTo.Value
                : validationEndDate;

            var classResult = patternParser.ParseAndGenerateOccurrences(
                segment.SchedulePattern,
                segmentStart,
                segmentEnd);

            if (classResult.IsFailure)
            {
                return Result.Failure<List<DateTime>>(Error.Validation(
                    "Enrollment.ClassSchedulePatternInvalid",
                    classResult.Error.Description));
            }

            occurrences.AddRange(classResult.Value);
        }

        return Result.Success(occurrences);
    }

    private async Task<Dictionary<Guid, List<ClassEnrollmentScheduleSegment>>> GetEnrollmentScheduleSegmentLookupAsync(
        IEnumerable<Guid> enrollmentIds,
        CancellationToken cancellationToken)
    {
        var enrollmentIdList = enrollmentIds
            .Distinct()
            .ToList();

        if (enrollmentIdList.Count == 0)
        {
            return new Dictionary<Guid, List<ClassEnrollmentScheduleSegment>>();
        }

        var segments = await context.ClassEnrollmentScheduleSegments
            .AsNoTracking()
            .Where(segment => enrollmentIdList.Contains(segment.ClassEnrollmentId))
            .OrderBy(segment => segment.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return segments
            .GroupBy(segment => segment.ClassEnrollmentId)
            .ToDictionary(
                group => group.Key,
                group => group.ToList());
    }

    private static string? ResolveSessionSelectionPattern(
        ClassEnrollment enrollment,
        DateOnly sessionDate,
        IReadOnlyDictionary<Guid, List<ClassEnrollmentScheduleSegment>> scheduleSegmentLookup)
    {
        if (!scheduleSegmentLookup.TryGetValue(enrollment.Id, out var segments) ||
            segments.Count == 0)
        {
            return enrollment.SessionSelectionPattern;
        }

        var matchingSegment = segments
            .Where(segment => segment.EffectiveFrom <= sessionDate &&
                (!segment.EffectiveTo.HasValue || sessionDate <= segment.EffectiveTo.Value))
            .OrderByDescending(segment => segment.EffectiveFrom)
            .FirstOrDefault();

        return matchingSegment?.SessionSelectionPattern ?? enrollment.SessionSelectionPattern;
    }

    private bool MatchesSelectionPattern(Session session, string? sessionSelectionPattern)
    {
        return ScheduleSelectionPatternMatcher.Matches(
            session.PlannedDatetime,
            sessionSelectionPattern,
            patternParser);
    }

    private static long ToMinuteKey(DateTime value)
    {
        return value.ToUniversalTime().Ticks / TimeSpan.TicksPerMinute;
    }
}
