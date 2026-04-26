using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Attendance.GetStudentAttendanceHistory;
using Kidzgo.Application.Authentication.ChangePassword;
using Kidzgo.Application.Exams.GetStudentExamResults;
using Kidzgo.Application.Invoices.GetParentInvoices;
using Kidzgo.Application.MakeupCredits.GetParentStudentsWithMakeupOrLeave;
using Kidzgo.Application.Media.GetMedia;
using Kidzgo.Application.Media.Shared;
using Kidzgo.Application.Notifications.GetParentNotifications;
using Kidzgo.Application.Payments.GetParentPayments;
using Kidzgo.Application.Registrations.GetParentEnrollmentConfirmationPdfHistory;
using Kidzgo.Application.Registrations.GetParentEnrollmentConfirmationPdfPreview;
using Kidzgo.Application.Registrations.GetParentRegistrationById;
using Kidzgo.Application.Registrations.GetParentRegistrations;
using Kidzgo.Application.Sessions.GetStudentTimetable;
using Kidzgo.Application.Users.GetCurrentUser;
using Kidzgo.Application.Users.GetParentOverview;
using Kidzgo.Application.Users.UpdateCurrentUser;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/parent")]
[ApiController]
[Authorize]
public class ParentController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;
    private readonly IUserContext _userContext;

    public ParentController(ISender mediator, IDbContext context, IUserContext userContext)
    {
        _mediator = mediator;
        _context = context;
        _userContext = userContext;
    }

    [HttpGet("overview")]
    public async Task<IResult> GetOverview(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sessionId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentOverviewQuery
        {
            StudentProfileId = studentProfileId,
            ClassId = classId,
            SessionId = sessionId,
            FromDate = fromDate,
            ToDate = toDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("registrations")]
    public async Task<IResult> GetRegistrations(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] string? status,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentRegistrationsQuery
        {
            StudentProfileId = studentProfileId,
            BranchId = branchId,
            ProgramId = programId,
            Status = status,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("registrations/{id:guid}")]
    public async Task<IResult> GetRegistrationById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetParentRegistrationByIdQuery
        {
            Id = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("registrations/{id:guid}/enrollment-confirmation-pdf")]
    public async Task<IResult> GetEnrollmentConfirmationPdfPreview(
        Guid id,
        [FromQuery] string track = "primary",
        [FromQuery] string formType = "auto",
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetParentEnrollmentConfirmationPdfPreviewQuery
        {
            RegistrationId = id,
            Track = track,
            FormType = formType
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("registrations/{id:guid}/enrollment-confirmation-pdf/history")]
    public async Task<IResult> GetEnrollmentConfirmationPdfHistory(
        Guid id,
        [FromQuery] string? track,
        [FromQuery] string? formType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetParentEnrollmentConfirmationPdfHistoryQuery
        {
            RegistrationId = id,
            Track = track,
            FormType = formType,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("students-with-makeup-or-leave")]
    public async Task<IResult> GetStudentsWithMakeupOrLeave(
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentStudentsWithMakeupOrLeaveQuery
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("timetable")]
    public async Task<IResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentTimetableQuery
        {
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("attendance")]
    public async Task<IResult> GetAttendance(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentAttendanceHistoryQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("exam-results")]
    public async Task<IResult> GetExamResults(
        [FromQuery] string? examType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Kidzgo.Domain.Exams.ExamType? parsedExamType = null;
        if (!string.IsNullOrWhiteSpace(examType) &&
            Enum.TryParse<Kidzgo.Domain.Exams.ExamType>(examType, true, out var parsed))
        {
            parsedExamType = parsed;
        }

        var query = new GetStudentExamResultsQuery
        {
            ExamType = parsedExamType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("invoices")]
    public async Task<IResult> GetInvoices(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        InvoiceStatus? invoiceStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<InvoiceStatus>(status, true, out var parsed))
        {
            invoiceStatus = parsed;
        }

        var query = new GetParentInvoicesByCurrentUserQuery
        {
            StudentProfileId = studentProfileId,
            Status = invoiceStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("payments")]
    public async Task<IResult> GetPayments(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? invoiceId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentPaymentsQuery
        {
            StudentProfileId = studentProfileId,
            InvoiceId = invoiceId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("media")]
    public async Task<IResult> GetMedia(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] string? monthTag,
        [FromQuery] DateTime? date,
        [FromQuery] string? type,
        CancellationToken cancellationToken = default)
    {
        var mediaQuery = _context.MediaAssets
            .AsNoTracking()
            .Where(m => !m.IsDeleted);

        mediaQuery = await MediaReadAccessHelper.ApplyReadAccessFilterAsync(
            mediaQuery,
            _context,
            _userContext,
            classId,
            studentProfileId,
            cancellationToken);

        if (!await mediaQuery.AnyAsync(cancellationToken))
        {
            var student = await ResolveStudentProfileAsync(studentProfileId, cancellationToken);
            if (student is null)
            {
                return NotFoundProblem("StudentProfile", "Student profile not linked to current parent");
            }
        }

        if (classId.HasValue)
        {
            mediaQuery = mediaQuery.Where(m => m.ClassId == classId.Value);
        }

        if (!string.IsNullOrWhiteSpace(monthTag))
        {
            mediaQuery = mediaQuery.Where(m => m.MonthTag == monthTag);
        }

        if (date.HasValue)
        {
            var startOfDay = date.Value.Date;
            var endOfDay = startOfDay.AddDays(1);
            mediaQuery = mediaQuery.Where(m => m.CreatedAt >= startOfDay && m.CreatedAt < endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MediaType>(type, true, out var parsedType))
        {
            mediaQuery = mediaQuery.Where(m => m.Type == parsedType);
        }

        var items = await mediaQuery
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .Select(m => new
            {
                id = m.Id,
                albumId = m.MonthTag ?? "general",
                title = m.Caption ?? m.OriginalFileName ?? m.Url,
                type = m.Type.ToString(),
                date = m.CreatedAt,
                coverUrl = m.Url,
                count = 1,
                url = m.Url
            })
            .ToListAsync(cancellationToken);

        return OkData(new
        {
            albums = items
                .GroupBy(m => new { m.albumId, month = m.date.ToString("yyyy-MM"), m.type })
                .Select(g => new
                {
                    albumId = g.Key.albumId,
                    title = g.Key.month,
                    type = g.Key.type,
                    date = g.Max(x => x.date),
                    coverUrl = g.First().coverUrl,
                    count = g.Count()
                })
                .OrderByDescending(a => a.date)
                .ToList(),
            items
        });
    }

    [HttpGet("notifications")]
    public async Task<IResult> GetNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetParentNotificationsQuery
        {
            UnreadOnly = unreadOnly,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("homework")]
    public async Task<IResult> GetParentHomework(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var student = await ResolveStudentProfileAsync(studentProfileId, cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not linked to current parent");
        }

        HomeworkStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HomeworkStatus>(status, true, out var homeworkStatus))
        {
            parsedStatus = homeworkStatus;
        }

        var query = _context.HomeworkStudents
            .AsNoTracking()
            .Where(hs => hs.StudentProfileId == student.Id);

        if (parsedStatus.HasValue)
        {
            query = query.Where(hs => hs.Status == parsedStatus.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(hs => hs.Assignment.DueAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(hs => new
            {
                id = hs.Id,
                subject = hs.Assignment.Class.Program.Name,
                title = hs.Assignment.Title,
                description = hs.Assignment.Description,
                dueDate = hs.Assignment.DueAt,
                status = hs.Status.ToString(),
                submittedAt = hs.SubmittedAt,
                score = hs.Score,
                priority = hs.Status == HomeworkStatus.Late || hs.Status == HomeworkStatus.Missing ? "High" : "Normal",
                attachmentCount = string.IsNullOrWhiteSpace(hs.AttachmentUrl) ? 0 : 1
            })
            .ToListAsync(cancellationToken);

        return OkData(new Page<object>(items.Cast<object>().ToList(), totalCount, pageNumber, pageSize));
    }

    [HttpGet("progress")]
    public async Task<IResult> GetParentProgress(
        [FromQuery] Guid? studentProfileId,
        CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(studentProfileId, cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not linked to current parent");
        }

        var attendanceTotal = await _context.Attendances
            .AsNoTracking()
            .CountAsync(a => a.StudentProfileId == student.Id, cancellationToken);
        var attendancePresent = attendanceTotal == 0
            ? 0
            : await _context.Attendances
                .AsNoTracking()
                .CountAsync(a => a.StudentProfileId == student.Id && a.AttendanceStatus == AttendanceStatus.Present, cancellationToken);

        var recentScores = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.StudentProfileId == student.Id)
            .OrderByDescending(er => er.CreatedAt)
            .Take(12)
            .Select(er => new
            {
                id = er.Id,
                title = er.Exam.Description ?? er.Exam.ExamType.ToString(),
                subject = er.Exam.Class.Program.Name,
                score = er.Score,
                maxScore = er.Exam.MaxScore,
                gradedAt = er.GradedAt
            })
            .ToListAsync(cancellationToken);

        var monthlySummaries = await _context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => r.StudentProfileId == student.Id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .Take(12)
            .Select(r => new
            {
                id = r.Id,
                r.Month,
                r.Year,
                status = r.Status.ToString(),
                summary = r.FinalContent ?? r.DraftContent
            })
            .ToListAsync(cancellationToken);

        var teacherComments = await _context.ReportComments
            .AsNoTracking()
            .Where(rc => rc.Report != null && rc.Report.StudentProfileId == student.Id)
            .OrderByDescending(rc => rc.CreatedAt)
            .Take(10)
            .Select(rc => new
            {
                id = rc.Id,
                content = rc.Content,
                author = rc.CommenterUser.Name,
                createdAt = rc.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return OkData(new
        {
            attendanceRate = attendanceTotal == 0 ? 0 : Math.Round((decimal)attendancePresent * 100 / attendanceTotal, 2),
            overallProgress = recentScores.Count == 0
                ? 0
                : Math.Round(recentScores.Average(x => ((x.score ?? 0) * 100m) / ((x.maxScore ?? 0) == 0 ? 1 : (x.maxScore ?? 1))), 2),
            skills = recentScores
                .GroupBy(x => x.subject)
                .Select(g => new { skill = g.Key, average = Math.Round(g.Average(x => ((x.score ?? 0) * 100m) / ((x.maxScore ?? 0) == 0 ? 1 : (x.maxScore ?? 1))), 2) })
                .ToList(),
            recentScores,
            teacherComments,
            monthlySummaries
        });
    }

    [HttpGet("approvals")]
    public async Task<IResult> GetParentApprovals(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(studentProfileId, cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not linked to current parent");
        }

        var pauseRequests = await _context.PauseEnrollmentRequests
            .AsNoTracking()
            .Where(r => r.StudentProfileId == student.Id)
            .OrderByDescending(r => r.RequestedAt)
            .Select(r => new
            {
                id = r.Id,
                title = "Pause Enrollment Request",
                description = r.Reason,
                type = "PauseEnrollment",
                status = r.Status.ToString(),
                createdAt = r.RequestedAt,
                dueAt = (DateTime?)r.PauseFrom.ToDateTime(TimeOnly.MinValue),
                actionUrl = $"/parent/approvals/{r.Id}"
            })
            .ToListAsync(cancellationToken);

        var invoiceApprovals = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.StudentProfileId == student.Id && (i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue))
            .OrderBy(i => i.DueDate)
            .Select(i => new
            {
                id = i.Id,
                title = "Invoice Payment Pending",
                description = i.Description,
                type = "Invoice",
                status = i.Status.ToString(),
                createdAt = i.IssuedAt ?? VietnamTime.UtcNow(),
                dueAt = i.DueDate.HasValue ? i.DueDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                actionUrl = i.PayosPaymentLink
            })
            .ToListAsync(cancellationToken);

        var approvals = pauseRequests.Cast<object>().Concat(invoiceApprovals.Cast<object>()).ToList();

        if (!string.IsNullOrWhiteSpace(status))
        {
            approvals = approvals
                .Where(item => item.GetType().GetProperty("status")?.GetValue(item)?.ToString()?.Equals(status, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();
        }

        return OkData(approvals);
    }

    [HttpGet("tests")]
    public async Task<IResult> GetParentTests(
        [FromQuery] Guid? studentProfileId,
        CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(studentProfileId, cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not linked to current parent");
        }

        var items = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.StudentProfileId == student.Id)
            .OrderByDescending(er => er.Exam.Date)
            .Select(er => new
            {
                id = er.Id,
                title = er.Exam.Description ?? er.Exam.ExamType.ToString(),
                type = er.Exam.ExamType.ToString(),
                subject = er.Exam.Class.Program.Name,
                className = er.Exam.Class.Title,
                testDate = er.Exam.Date,
                duration = er.Exam.TimeLimitMinutes,
                status = er.GradedAt != null ? "Graded" : "Pending",
                score = er.Score,
                maxScore = er.Exam.MaxScore,
                percentage = (er.Exam.MaxScore ?? 0) == 0 ? 0 : Math.Round((er.Score ?? 0) * 100 / (er.Exam.MaxScore ?? 1), 2),
                averageScore = _context.ExamResults.Where(x => x.ExamId == er.ExamId).Average(x => (decimal?)x.Score),
                rank = _context.ExamResults.Count(x => x.ExamId == er.ExamId && x.Score > er.Score) + 1,
                totalStudents = _context.ExamResults.Count(x => x.ExamId == er.ExamId)
            })
            .ToListAsync(cancellationToken);

        return OkData(items);
    }

    [HttpGet("tests/{id:guid}")]
    public async Task<IResult> GetParentTestDetail(
        Guid id,
        [FromQuery] Guid? studentProfileId,
        CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(studentProfileId, cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not linked to current parent");
        }

        var examResult = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.Id == id && er.StudentProfileId == student.Id)
            .Select(er => new
            {
                er.Id,
                title = er.Exam.Description ?? er.Exam.ExamType.ToString(),
                type = er.Exam.ExamType.ToString(),
                subject = er.Exam.Class.Program.Name,
                className = er.Exam.Class.Title,
                testDate = er.Exam.Date,
                duration = er.Exam.TimeLimitMinutes,
                status = er.GradedAt != null ? "Graded" : "Pending",
                score = er.Score,
                maxScore = er.Exam.MaxScore,
                percentage = (er.Exam.MaxScore ?? 0) == 0 ? 0 : Math.Round((er.Score ?? 0) * 100 / (er.Exam.MaxScore ?? 1), 2),
                averageScore = _context.ExamResults.Where(x => x.ExamId == er.ExamId).Average(x => (decimal?)x.Score),
                rank = _context.ExamResults.Count(x => x.ExamId == er.ExamId && x.Score > er.Score) + 1,
                totalStudents = _context.ExamResults.Count(x => x.ExamId == er.ExamId),
                feedback = er.Comment
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (examResult is null)
        {
            return NotFoundProblem("ExamResult", "Exam result not found");
        }

        return OkData(new
        {
            examResult,
            skillBreakdown = Array.Empty<object>(),
            sections = Array.Empty<object>(),
            feedback = examResult.feedback,
            ranking = new { examResult.rank, examResult.totalStudents }
        });
    }

    [HttpGet("account")]
    public async Task<IResult> GetParentAccount(CancellationToken cancellationToken)
    {
        var currentUser = await _mediator.Send(new GetCurrentUserQuery(), cancellationToken);
        if (!currentUser.IsSuccess)
        {
            return CustomResults.Problem(currentUser);
        }

        var parentProfile = await ResolveParentProfileAsync(cancellationToken);

        return OkData(new
        {
            user = currentUser.Value,
            parentProfile
        });
    }

    [HttpPut("account")]
    public async Task<IResult> UpdateParentAccount(
        [FromBody] UpdateCurrentUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCurrentUserCommand
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Profiles = request.Profiles?.Select(p => new Kidzgo.Application.Users.UpdateCurrentUser.UpdateProfileDto
            {
                Id = p.Id,
                DisplayName = p.DisplayName
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("account/change-password")]
    public async Task<IResult> ChangeParentAccountPassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangePasswordCommand
        {
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        }, cancellationToken);

        return result.MatchOk();
    }

    private async Task<Profile?> ResolveStudentProfileAsync(Guid? requestedStudentProfileId, CancellationToken cancellationToken)
    {
        var parentProfile = await ResolveParentProfileAsync(cancellationToken);
        if (parentProfile is null)
        {
            return null;
        }

        var linkedStudentId = requestedStudentProfileId
            ?? _userContext.StudentId
            ?? await _context.ParentStudentLinks
                .AsNoTracking()
                .Where(link => link.ParentProfileId == parentProfile.Id)
                .Select(link => (Guid?)link.StudentProfileId)
                .FirstOrDefaultAsync(cancellationToken);

        if (!linkedStudentId.HasValue)
        {
            return null;
        }

        return await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == linkedStudentId.Value &&
                p.ProfileType == ProfileType.Student &&
                !p.IsDeleted &&
                _context.ParentStudentLinks.Any(link =>
                    link.ParentProfileId == parentProfile.Id &&
                    link.StudentProfileId == p.Id), cancellationToken);
    }

    private async Task<Profile?> ResolveParentProfileAsync(CancellationToken cancellationToken)
    {
        return await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.UserId == _userContext.UserId &&
                p.ProfileType == ProfileType.Parent &&
                !p.IsDeleted &&
                p.IsActive, cancellationToken);
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }

    private static IResult NotFoundProblem(string title, string detail)
    {
        return Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);
    }
}
