using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Missions.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Audit;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kidzgo.Application.Attendance.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler(
    IDbContext context,
    IUserContext userContext,
    IGamificationService gamificationService,
    SessionParticipantService sessionParticipantService,
    ClassLifecycleService classLifecycleService,
    RegistrationSessionConsumptionService registrationSessionConsumptionService,
    ApprovedLeaveAttendanceService approvedLeaveAttendanceService)
    : ICommandHandler<UpdateAttendanceCommand, UpdateAttendanceResponse>
{
    public async Task<Result<UpdateAttendanceResponse>> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var attendance = await context.Attendances
            .Include(a => a.Session)
            .FirstOrDefaultAsync(a =>
                a.SessionId == request.SessionId &&
                a.StudentProfileId == request.StudentProfileId,
                cancellationToken);

        if (attendance is null)
        {
            if (!request.IsAdmin)
            {
                var session = await context.Sessions
                    .FirstOrDefaultAsync(s => s.Id == request.SessionId, cancellationToken);

                if (session is not null && await approvedLeaveAttendanceService
                        .HasApprovedLeaveAsync(request.StudentProfileId, session, cancellationToken))
                {
                    return Result.Failure<UpdateAttendanceResponse>(
                        AttendanceErrors.ApprovedLeaveLocked(request.SessionId, request.StudentProfileId));
                }
            }

            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.NotFoundForSessionStudent(request.SessionId, request.StudentProfileId));
        }

        var sessionDate = VietnamTime.ToVietnamDateOnly(attendance.Session.ActualDatetime ?? attendance.Session.PlannedDatetime);
        var today = VietnamTime.TodayDateOnly();
        if (!request.IsAdmin && sessionDate > today)
        {
            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.FutureSessionNotAllowed(attendance.SessionId));
        }

        if (!request.IsAdmin && sessionDate < today)
        {
            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.SessionDateClosed(attendance.SessionId));
        }

        if (!request.IsAdmin && await approvedLeaveAttendanceService
                .HasApprovedLeaveAsync(request.StudentProfileId, attendance.Session, cancellationToken))
        {
            return Result.Failure<UpdateAttendanceResponse>(
                AttendanceErrors.ApprovedLeaveLocked(request.SessionId, request.StudentProfileId));
        }

        // Store old values for audit log
        var oldStatus = attendance.AttendanceStatus;
        var oldAbsenceType = attendance.AbsenceType;
        var oldNote = attendance.Note;
        var shouldTrackClassAttendanceMission = oldStatus != request.AttendanceStatus;

        var participants = await sessionParticipantService.GetParticipantsAsync(request.SessionId, cancellationToken);
        var participant = participants.FirstOrDefault(p => p.StudentProfileId == request.StudentProfileId);
        var isMakeupParticipant = participant.StudentProfileId != Guid.Empty && participant.IsMakeup;
        var registrationId = participant.StudentProfileId != Guid.Empty
            ? participant.RegistrationId
            : null;
        var impactedClassIds = new HashSet<Guid>();
        AttendanceTransitionOutcome? transitionOutcome = null;

        var newAbsenceType = request.AttendanceStatus == AttendanceStatus.Absent
            ? await ResolveAbsenceType(request.StudentProfileId, attendance.Session, cancellationToken)
            : (AbsenceType?)null;

        // Update attendance
        attendance.AttendanceStatus = request.AttendanceStatus;
        attendance.AbsenceType = newAbsenceType;

        if (request.Note is not null)
        {
            attendance.Note = request.Note;
        }

        if (!isMakeupParticipant)
        {
            transitionOutcome = await registrationSessionConsumptionService.ApplyAttendanceTransitionAsync(
                attendance.SessionId,
                attendance.Id,
                registrationId,
                oldStatus,
                oldAbsenceType,
                attendance.AttendanceStatus,
                attendance.AbsenceType,
                attendance.Session.ParticipationType,
                attendance.Session.SectionType,
                attendance.Session.SlotTypeId,
                attendance.Session.ActualDatetime ?? attendance.Session.PlannedDatetime,
                cancellationToken);
            impactedClassIds.UnionWith(transitionOutcome.ImpactedClassIds);
        }

        // Create audit log
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            ActorUserId = userContext.UserId,
            Action = "UpdateAttendance",
            EntityType = "Attendance",
            EntityId = attendance.Id,
            DataBefore = JsonSerializer.Serialize(new { oldStatus = oldStatus.ToString(), oldNote }),
            DataAfter = JsonSerializer.Serialize(new { newStatus = request.AttendanceStatus.ToString(), newNote = request.Note }),
            IpAddress = userContext.IpAddress,
            CreatedAt = VietnamTime.UtcNow()
        };
        context.AuditLogs.Add(auditLog);

        await context.SaveChangesAsync(cancellationToken);

        foreach (var classId in impactedClassIds)
        {
            await classLifecycleService.RecalculateClassLifecycleAsync(classId, cancellationToken);
        }

        if (impactedClassIds.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        if (shouldTrackClassAttendanceMission)
        {
            await ClassAttendanceMissionProgressTracker.TrackAsync(
                context,
                gamificationService,
                attendance.StudentProfileId,
                attendance.Session,
                cancellationToken);
        }

        return new UpdateAttendanceResponse
        {
            Id = attendance.Id,
            SessionId = attendance.SessionId,
            StudentProfileId = attendance.StudentProfileId,
            AttendanceStatus = attendance.AttendanceStatus.ToString(),
            AbsenceType = attendance.AbsenceType.HasValue ? attendance.AbsenceType.Value.ToString() : null,
            Note = attendance.Note,
            TicketConsumed = transitionOutcome?.TicketDelta == -1,
            ConsumedQuantity = transitionOutcome?.TicketDelta == -1 ? 1 : 0,
            AdvanceLessonProgression = transitionOutcome?.AdvanceLessonProgression ?? false,
            TicketBalance = transitionOutcome?.TicketBalance,
            TicketCompatibilityPassed = transitionOutcome?.CompatibilityPassed,
            TicketCompatibilityReason = transitionOutcome?.CompatibilityReason,
            UpdatedAt = attendance.MarkedAt
        };
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

