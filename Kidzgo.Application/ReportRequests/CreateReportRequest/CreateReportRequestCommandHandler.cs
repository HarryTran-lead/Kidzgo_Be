using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportRequests.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Notifications;
using Kidzgo.Domain.Notifications.Events;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Reports.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ReportRequests.CreateReportRequest;

public sealed class CreateReportRequestCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateReportRequestCommand, ReportRequestDto>
{
    public async Task<Result<ReportRequestDto>> Handle(
        CreateReportRequestCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.TargetStudentProfileId.HasValue && !command.TargetClassId.HasValue)
        {
            return Result.Failure<ReportRequestDto>(ReportRequestErrors.TargetRequired);
        }

        var assignedTeacher = await context.Users
            .FirstOrDefaultAsync(u => u.Id == command.AssignedTeacherUserId &&
                                      u.Role == UserRole.Teacher &&
                                      !u.IsDeleted,
                cancellationToken);

        if (assignedTeacher is null)
        {
            return Result.Failure<ReportRequestDto>(ReportRequestErrors.TeacherNotFound);
        }

        var requester = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (requester is null)
        {
            return Result.Failure<ReportRequestDto>(
                Error.NotFound("User.NotFound", "Requester was not found"));
        }

        var targetStudent = command.TargetStudentProfileId.HasValue
            ? await context.Profiles.FirstOrDefaultAsync(
                p => p.Id == command.TargetStudentProfileId.Value &&
                     p.ProfileType == ProfileType.Student,
                cancellationToken)
            : null;

        if (command.TargetStudentProfileId.HasValue && targetStudent is null)
        {
            return Result.Failure<ReportRequestDto>(ReportRequestErrors.StudentNotFound);
        }

        var targetClassId = command.TargetClassId;
        Guid? linkedMonthlyReportId = null;
        Guid? linkedSessionReportId = null;

        if (command.ReportType == ReportRequestType.Session)
        {
            var validation = await ValidateSessionRequestAsync(
                command,
                targetStudent is not null,
                cancellationToken);

            if (validation.IsFailure)
            {
                return Result.Failure<ReportRequestDto>(validation.Error);
            }

            targetClassId = validation.Value.TargetClassId;
            linkedSessionReportId = validation.Value.LinkedSessionReportId;
        }
        else
        {
            var validation = await ValidateMonthlyRequestAsync(
                command,
                targetStudent is not null,
                cancellationToken);

            if (validation.IsFailure)
            {
                return Result.Failure<ReportRequestDto>(validation.Error);
            }

            targetClassId = validation.Value.TargetClassId;
            linkedMonthlyReportId = validation.Value.LinkedMonthlyReportId;
        }

        var now = VietnamTime.UtcNow();
        var request = new ReportRequest
        {
            Id = Guid.NewGuid(),
            ReportType = command.ReportType,
            Status = ReportRequestStatus.Requested,
            Priority = command.Priority,
            AssignedTeacherUserId = command.AssignedTeacherUserId,
            RequestedByUserId = userContext.UserId,
            TargetStudentProfileId = command.TargetStudentProfileId,
            TargetClassId = targetClassId,
            TargetSessionId = command.TargetSessionId,
            Month = command.Month,
            Year = command.Year,
            Message = command.Message,
            DueAt = command.DueAt,
            LinkedSessionReportId = linkedSessionReportId,
            LinkedMonthlyReportId = linkedMonthlyReportId,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ReportRequests.Add(request);
        context.Notifications.Add(CreateTeacherNotification(
            request,
            command.NotificationChannel,
            requester,
            assignedTeacher,
            targetStudent?.DisplayName));

        await context.SaveChangesAsync(cancellationToken);

        var notification = await context.Notifications
            .Where(n => n.RecipientUserId == assignedTeacher.Id &&
                        n.Kind == "report_request" &&
                        n.Deeplink == $"/report-requests/{request.Id}")
            .OrderByDescending(n => n.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (notification is not null && notification.Channel != NotificationChannel.InApp)
        {
            notification.Raise(new NotificationCreatedDomainEvent(notification.Id, notification.Channel));
            await context.SaveChangesAsync(cancellationToken);
        }

        var created = await LoadRequestAsync(request.Id, cancellationToken);
        return ReportRequestMapper.ToDto(created!);
    }

    private async Task<Result<ValidatedRequestTarget>> ValidateSessionRequestAsync(
        CreateReportRequestCommand command,
        bool hasStudentTarget,
        CancellationToken cancellationToken)
    {
        if (!command.TargetSessionId.HasValue)
        {
            return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.SessionRequired);
        }

        var session = await context.Sessions
            .Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == command.TargetSessionId.Value, cancellationToken);

        if (session is null)
        {
            return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.SessionNotFound);
        }

        if (command.TargetClassId.HasValue && command.TargetClassId.Value != session.ClassId)
        {
            return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.ClassSessionMismatch);
        }

        var teacherMatchesSession =
            session.PlannedTeacherId == command.AssignedTeacherUserId ||
            session.ActualTeacherId == command.AssignedTeacherUserId ||
            session.Class.MainTeacherId == command.AssignedTeacherUserId ||
            session.Class.AssistantTeacherId == command.AssignedTeacherUserId;

        if (!teacherMatchesSession)
        {
            return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.TeacherNotAssigned);
        }

        Guid? linkedSessionReportId = null;

        if (hasStudentTarget && command.TargetStudentProfileId.HasValue)
        {
            var hasAssignment = await context.StudentSessionAssignments
                .AnyAsync(a => a.SessionId == session.Id &&
                               a.StudentProfileId == command.TargetStudentProfileId.Value,
                    cancellationToken);

            if (!hasAssignment)
            {
                return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.StudentNotInSession);
            }

            linkedSessionReportId = await context.SessionReports
                .Where(r => r.SessionId == session.Id &&
                            r.StudentProfileId == command.TargetStudentProfileId.Value)
                .Select(r => (Guid?)r.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new ValidatedRequestTarget(session.ClassId, linkedSessionReportId, null);
    }

    private async Task<Result<ValidatedRequestTarget>> ValidateMonthlyRequestAsync(
        CreateReportRequestCommand command,
        bool hasStudentTarget,
        CancellationToken cancellationToken)
    {
        if (!command.Month.HasValue || !command.Year.HasValue ||
            command.Month.Value < 1 || command.Month.Value > 12)
        {
            return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.MonthYearRequired);
        }

        var targetClassId = command.TargetClassId;

        if (hasStudentTarget && !targetClassId.HasValue)
        {
            var startDate = new DateOnly(command.Year.Value, command.Month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var classIds = await context.ClassEnrollments
                .Where(e => e.StudentProfileId == command.TargetStudentProfileId!.Value &&
                            (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused) &&
                            e.EnrollDate <= endDate)
                .Select(e => e.ClassId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (classIds.Count != 1)
            {
                return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.MonthlyClassRequiredForStudent);
            }

            targetClassId = classIds[0];
        }

        if (targetClassId.HasValue)
        {
            var targetClass = await context.Classes
                .FirstOrDefaultAsync(c => c.Id == targetClassId.Value, cancellationToken);

            if (targetClass is null)
            {
                return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.ClassNotFound);
            }

            if (targetClass.MainTeacherId != command.AssignedTeacherUserId &&
                targetClass.AssistantTeacherId != command.AssignedTeacherUserId)
            {
                return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.TeacherNotAssigned);
            }
        }

        Guid? linkedMonthlyReportId = null;

        if (hasStudentTarget && targetClassId.HasValue)
        {
            var isEnrolled = await context.ClassEnrollments
                .AnyAsync(e => e.StudentProfileId == command.TargetStudentProfileId!.Value &&
                               e.ClassId == targetClassId.Value &&
                               (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused),
                    cancellationToken);

            if (!isEnrolled)
            {
                return Result.Failure<ValidatedRequestTarget>(ReportRequestErrors.StudentNotInClass);
            }

            var report = await context.StudentMonthlyReports
                .FirstOrDefaultAsync(r => r.StudentProfileId == command.TargetStudentProfileId!.Value &&
                                          r.ClassId == targetClassId.Value &&
                                          r.Month == command.Month.Value &&
                                          r.Year == command.Year.Value,
                    cancellationToken);

            if (report is null)
            {
                var now = VietnamTime.UtcNow();
                report = new StudentMonthlyReport
                {
                    Id = Guid.NewGuid(),
                    StudentProfileId = command.TargetStudentProfileId!.Value,
                    ClassId = targetClassId.Value,
                    Month = command.Month.Value,
                    Year = command.Year.Value,
                    Status = ReportStatus.Draft,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                context.StudentMonthlyReports.Add(report);
            }

            linkedMonthlyReportId = report.Id;
        }
        else if (!hasStudentTarget && targetClassId.HasValue)
        {
            var startDate = new DateOnly(command.Year.Value, command.Month.Value, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var now = VietnamTime.UtcNow();

            var studentIds = await context.ClassEnrollments
                .Where(e => e.ClassId == targetClassId.Value &&
                            (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Paused) &&
                            e.EnrollDate <= endDate)
                .Select(e => e.StudentProfileId)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (studentIds.Count > 0)
            {
                var existingStudentIds = await context.StudentMonthlyReports
                    .Where(r => r.ClassId == targetClassId.Value &&
                                r.Month == command.Month.Value &&
                                r.Year == command.Year.Value &&
                                studentIds.Contains(r.StudentProfileId))
                    .Select(r => r.StudentProfileId)
                    .ToListAsync(cancellationToken);

                foreach (var studentId in studentIds.Except(existingStudentIds))
                {
                    context.StudentMonthlyReports.Add(new StudentMonthlyReport
                    {
                        Id = Guid.NewGuid(),
                        StudentProfileId = studentId,
                        ClassId = targetClassId.Value,
                        Month = command.Month.Value,
                        Year = command.Year.Value,
                        Status = ReportStatus.Draft,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
            }
        }

        return new ValidatedRequestTarget(targetClassId, null, linkedMonthlyReportId);
    }

    private Notification CreateTeacherNotification(
        ReportRequest request,
        NotificationChannel channel,
        User requester,
        User assignedTeacher,
        string? studentName)
    {
        var title = request.ReportType == ReportRequestType.Monthly
            ? "Monthly report request"
            : "Session report request";

        var target = studentName ?? "class";
        var content = string.IsNullOrWhiteSpace(request.Message)
            ? $"A priority {request.ReportType.ToString().ToLowerInvariant()} report request was assigned for {target}."
            : request.Message;

        return new Notification
        {
            Id = Guid.NewGuid(),
            RecipientUserId = assignedTeacher.Id,
            Channel = channel,
            Title = title,
            Content = content,
            Deeplink = $"/report-requests/{request.Id}",
            Status = NotificationStatus.Pending,
            CreatedAt = request.CreatedAt,
            TargetRole = "Teacher",
            Kind = "report_request",
            Priority = request.Priority.ToString().ToLowerInvariant(),
            SenderRole = requester.Role.ToString(),
            SenderName = requester.Name,
            ScopeClassId = request.TargetClassId,
            ScopeStudentProfileId = request.TargetStudentProfileId
        };
    }

    private Task<ReportRequest?> LoadRequestAsync(Guid id, CancellationToken cancellationToken)
    {
        return context.ReportRequests
            .Include(r => r.AssignedTeacher)
            .Include(r => r.RequestedByUser)
            .Include(r => r.TargetStudentProfile)
            .Include(r => r.TargetClass)
            .Include(r => r.TargetSession)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    private sealed record ValidatedRequestTarget(
        Guid? TargetClassId,
        Guid? LinkedSessionReportId,
        Guid? LinkedMonthlyReportId);
}
