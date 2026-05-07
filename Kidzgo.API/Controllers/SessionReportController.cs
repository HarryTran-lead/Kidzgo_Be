using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.SessionReports.CreateSessionReport;
using Kidzgo.Application.SessionReports.GetSessionReportById;
using Kidzgo.Application.SessionReports.GetSessionReports;
using Kidzgo.Application.SessionReports.GetTeacherSessionReports;
using Kidzgo.Application.SessionReports.UpdateSessionReport;
using Kidzgo.Application.SessionReports.SubmitSessionReport;
using Kidzgo.Application.SessionReports.ApproveSessionReport;
using Kidzgo.Application.SessionReports.RejectSessionReport;
using Kidzgo.Application.SessionReports.PublishSessionReport;
using Kidzgo.Application.SessionReports.AddSessionReportComment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/session-reports")]
[ApiController]
[Authorize]
public class SessionReportController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly IAiFeedbackEnhancer _feedbackEnhancer;

    public SessionReportController(ISender mediator, IAiFeedbackEnhancer feedbackEnhancer)
    {
        _mediator = mediator;
        _feedbackEnhancer = feedbackEnhancer;
    }

    /// UC-174: AI enhance draft feedback (preview - KHONG luu DB)
    [HttpPost("ai/enhance-feedback")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> EnhanceFeedback(
        [FromBody] EnhanceFeedbackRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Draft))
        {
            return Results.BadRequest(new { isSuccess = false, message = "Draft cannot be empty" });
        }

        var result = await _feedbackEnhancer.EnhanceAsync(request.Draft, cancellationToken);

        return Results.Ok(new
        {
            isSuccess = true,
            data = new
            {
                enhancedFeedback = result.EnhancedFeedback,
                originalFeedback = result.OriginalFeedback
            }
        });
    }

    /// UC-163: Teacher tạo Session Report
    [HttpPost]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> CreateSessionReport(
        [FromBody] CreateSessionReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateSessionReportCommand
        {
            SessionId = request.SessionId,
            StudentProfileId = request.StudentProfileId,
            ReportDate = request.ReportDate,
            Feedback = request.Feedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(sr => $"/api/session-reports/{sr.Id}");
    }

    /// UC-165: Xem danh sách Session Reports
    /// UC-168: Filter Session Reports theo date range
    [HttpGet]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff,Parent")]
    public async Task<IResult> GetSessionReports(
        [FromQuery] Guid? sessionId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? teacherUserId,
        [FromQuery] Guid? classId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSessionReportsQuery
        {
            SessionId = sessionId,
            StudentProfileId = studentProfileId,
            TeacherUserId = teacherUserId,
            ClassId = classId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-166: Xem chi tiết Session Report
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff,Parent")]
    public async Task<IResult> GetSessionReportById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionReportByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-167: Cập nhật Session Report
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> UpdateSessionReport(
        Guid id,
        [FromBody] UpdateSessionReportRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionReportCommand
        {
            Id = id,
            Feedback = request.Feedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-169: Xem Session Reports của giáo viên trong tháng
    [HttpGet("teachers/{teacherUserId:guid}/monthly")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff")]
    public async Task<IResult> GetTeacherSessionReports(
        Guid teacherUserId,
        [FromQuery] int year,
        [FromQuery] int month,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTeacherSessionReportsQuery
        {
            TeacherUserId = teacherUserId,
            Year = year,
            Month = month,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-175: Teacher submit Session Report cho duyet (DRAFT -> REVIEW)
    [HttpPost("{id:guid}/submit")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> SubmitSessionReport(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new SubmitSessionReportCommand(id);

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-176: Staff/Admin approve Session Report (REVIEW -> APPROVED)
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ApproveSessionReport(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ApproveSessionReportCommand(id);

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-177: Staff/Admin reject Session Report (REVIEW -> REJECTED)
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> RejectSessionReport(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RejectSessionReportCommand(id);

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-178: Publish Session Report (APPROVED -> PUBLISHED)
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> PublishSessionReport(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PublishSessionReportCommand(id);

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-180*: Admin/ManagementStaff comment tren Session Report
    [HttpPost("{id:guid}/comments")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AddComment(
        Guid id,
        [FromBody] AddSessionReportCommentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddSessionReportCommentCommand(id, request.Content);

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(c => $"/api/session-reports/{id}/comments/{c.Id}");
    }
}
