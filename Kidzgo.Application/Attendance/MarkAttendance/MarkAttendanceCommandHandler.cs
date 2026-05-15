using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Missions.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Attendance.MarkAttendance;

public sealed class MarkAttendanceCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService,
    SessionParticipantService sessionParticipantService,
    ClassLifecycleService classLifecycleService,
    RegistrationSessionConsumptionService registrationSessionConsumptionService,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : ICommandHandler<MarkAttendanceCommand, MarkAttendanceResponse>
{
    public async Task<Result<MarkAttendanceResponse>> Handle(MarkAttendanceCommand command, CancellationToken cancellationToken)
    {
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Failure<MarkAttendanceResponse>(AttendanceErrors.NotFound(command.SessionId));
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(session.ActualDatetime ?? session.PlannedDatetime);
        var today = VietnamTime.TodayDateOnly();
        if (!command.IsAdmin && sessionDate > today)
        {
            return Result.Failure<MarkAttendanceResponse>(
                AttendanceErrors.FutureSessionNotAllowed(command.SessionId));
        }

        var results = new List<AttendanceResultItem>();
        var studentsWithClassAttendanceMissionChanges = new HashSet<Guid>();
        var impactedClassIds = new HashSet<Guid>();
        var participants = await sessionParticipantService.GetParticipantsAsync(command.SessionId, cancellationToken);
        var participantsByStudent = participants.ToDictionary(p => p.StudentProfileId);
        var requestedStudentIds = command.Attendances
            .Select(x => x.StudentProfileId)
            .Distinct()
            .ToList();
        var existingAttendances = await context.Attendances
            .Where(a => a.SessionId == command.SessionId && requestedStudentIds.Contains(a.StudentProfileId))
            .ToDictionaryAsync(a => a.StudentProfileId, cancellationToken);
        var studentsWithApprovedLeave = command.IsAdmin
            ? new HashSet<Guid>()
            : await approvedLeaveAttendanceService.GetApprovedLeaveStudentIdsForSessionAsync(
                session,
                requestedStudentIds,
                cancellationToken);

        foreach (var item in command.Attendances)
        {
            if (!participantsByStudent.TryGetValue(item.StudentProfileId, out var participant))
            {
                return Result.Failure<MarkAttendanceResponse>(Error.Validation(
                    "Attendance.StudentNotAssigned",
                    $"Student '{item.StudentProfileId}' is not assigned to session '{command.SessionId}'."));
            }

            existingAttendances.TryGetValue(item.StudentProfileId, out var attendance);

            if (studentsWithApprovedLeave.Contains(item.StudentProfileId))
            {
                results.Add(new AttendanceResultItem
                {
                    Id = attendance?.Id ?? Guid.Empty,
                    SessionId = command.SessionId,
                    StudentProfileId = item.StudentProfileId,
                    AttendanceStatus = AttendanceStatus.Makeup.ToString(),
                    AbsenceType = null,
                    MarkedAt = null,
                    Note = attendance?.Note,
                    TicketConsumed = false,
                    ConsumedQuantity = 0,
                    AdvanceLessonProgression = false
                });

                continue;
            }

            var previousStatus = attendance?.AttendanceStatus;
            var previousAbsenceType = attendance?.AbsenceType;
            var shouldTrackClassAttendanceMission = previousStatus != item.AttendanceStatus;

            if (attendance is null)
            {
                attendance = new Domain.Sessions.Attendance
                {
                    Id = Guid.NewGuid(),
                    SessionId = command.SessionId,
                    StudentProfileId = item.StudentProfileId,
                };
                context.Attendances.Add(attendance);
                existingAttendances[item.StudentProfileId] = attendance;
            }

            if (item.Note is not null)
            {
                attendance.Note = item.Note;
            }

            var newAbsenceType = default(AbsenceType?);
            AttendanceTransitionOutcome? transitionOutcome = null;

            if (item.AttendanceStatus == AttendanceStatus.Absent)
            {
                var absenceType = await ResolveAbsenceType(item.StudentProfileId, session, cancellationToken);
                newAbsenceType = absenceType;

                if (absenceType == AbsenceType.WithNotice24H)
                {
                    bool creditExists = await context.MakeupCredits
                        .AnyAsync(c => c.StudentProfileId == item.StudentProfileId &&
                                       c.SourceSessionId == session.Id &&
                                       c.CreatedReason == CreatedReason.ApprovedLeave24H,
                            cancellationToken);

                    if (!creditExists)
                    {
                        var credit = new MakeupCredit
                        {
                            Id = Guid.NewGuid(),
                            StudentProfileId = item.StudentProfileId,
                            SourceSessionId = session.Id,
                            Status = MakeupCreditStatus.Available,
                            CreatedReason = CreatedReason.ApprovedLeave24H,
                            ExpiresAt = null,
                            CreatedAt = VietnamTime.UtcNow()
                        };
                        context.MakeupCredits.Add(credit);
                    }
                }
            }

            attendance.AttendanceStatus = item.AttendanceStatus;
            attendance.AbsenceType = newAbsenceType;

            if (!participant.IsMakeup)
            {
                transitionOutcome = await registrationSessionConsumptionService.ApplyAttendanceTransitionAsync(
                    command.SessionId,
                    attendance.Id,
                    participant.RegistrationId,
                    previousStatus,
                    previousAbsenceType,
                    attendance.AttendanceStatus,
                    attendance.AbsenceType,
                    session.SectionType,
                    session.SlotTypeId,
                    session.ActualDatetime ?? session.PlannedDatetime,
                    cancellationToken);
                impactedClassIds.UnionWith(transitionOutcome.ImpactedClassIds);
            }

            attendance.MarkedBy = userContext.UserId;
            attendance.MarkedAt = VietnamTime.UtcNow();

            if (shouldTrackClassAttendanceMission)
            {
                studentsWithClassAttendanceMissionChanges.Add(item.StudentProfileId);
            }

            results.Add(new AttendanceResultItem
            {
                Id = attendance.Id,
                SessionId = attendance.SessionId,
                StudentProfileId = attendance.StudentProfileId,
                AttendanceStatus = attendance.AttendanceStatus.ToString(),
                AbsenceType = attendance.AbsenceType.HasValue ? attendance.AbsenceType.Value.ToString() : null,
                MarkedAt = attendance.MarkedAt,
                Note = attendance.Note,
                TicketConsumed = transitionOutcome?.TicketDelta == -1,
                ConsumedQuantity = transitionOutcome?.TicketDelta == -1 ? 1 : 0,
                AdvanceLessonProgression = transitionOutcome?.AdvanceLessonProgression ?? false,
                TicketBalance = transitionOutcome?.TicketBalance,
                TicketCompatibilityPassed = transitionOutcome?.CompatibilityPassed,
                TicketCompatibilityReason = transitionOutcome?.CompatibilityReason
            });
        }

        await context.SaveChangesAsync(cancellationToken);

        foreach (var classId in impactedClassIds)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(classId, cancellationToken);
        }

        if (impactedClassIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        foreach (var studentProfileId in studentsWithClassAttendanceMissionChanges)
        {
            await ClassAttendanceMissionProgressTracker.TrackAsync(
                context,
                gamificationService,
                studentProfileId,
                session,
                cancellationToken);
        }

        return Result.Success(new MarkAttendanceResponse()
        {
            Results = results
        });
    }
    private async Task<AbsenceType> ResolveAbsenceType(Guid studentProfileId, Session session, CancellationToken cancellationToken)
    {
        var sessionDate = VietnamTime.ToVietnamDateOnly(session.PlannedDatetime);
        var leave = await context.LeaveRequests
            .Where(l => l.StudentProfileId == studentProfileId
                        && (l.SessionId == session.Id ||
                            (l.SessionDate <= sessionDate
                             && (l.EndDate == null || l.EndDate >= sessionDate)))
                        && l.Status == LeaveRequestStatus.Approved)
            .OrderByDescending(l => l.RequestedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (leave is not null)
        {
            if (leave.NoticeHours.GetValueOrDefault() >= 24)
            {
                return AbsenceType.WithNotice24H;
            }

            if (leave.NoticeHours.GetValueOrDefault() >= 0)
            {
                return AbsenceType.Under24H;
            }
        }

        return AbsenceType.NoNotice;
    }
}

