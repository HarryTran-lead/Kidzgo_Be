using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.MonthlyReports.AddReportComment;
using Kidzgo.Application.MonthlyReports.AggregateMonthlyReportData;
using Kidzgo.Application.MonthlyReports.ApproveMonthlyReport;
using Kidzgo.Application.MonthlyReports.CreateMonthlyReportJob;
using Kidzgo.Application.MonthlyReports.GenerateMonthlyReportDraft;
using Kidzgo.Application.MonthlyReports.GenerateMonthlyReportPdf;
using Kidzgo.Application.MonthlyReports.GetMonthlyReportById;
using Kidzgo.Application.MonthlyReports.GetMonthlyReportJobById;
using Kidzgo.Application.MonthlyReports.GetMonthlyReportJobs;
using Kidzgo.Application.MonthlyReports.GetMonthlyReports;
using Kidzgo.Application.MonthlyReports.PublishMonthlyReport;
using Kidzgo.Application.MonthlyReports.RejectMonthlyReport;
using Kidzgo.Application.MonthlyReports.SubmitMonthlyReport;
using Kidzgo.Application.MonthlyReports.UnpublishMonthlyReport;
using Kidzgo.Application.MonthlyReports.UpdateMonthlyReportDraft;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/monthly-reports")]
[ApiController]
[Authorize]
public class MonthlyReportController : ControllerBase
{
    private readonly ISender _mediator;

    public MonthlyReportController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-174: Tạo Monthly Report Job
    /// </summary>
    [HttpPost("jobs")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateMonthlyReportJob(
        [FromBody] CreateMonthlyReportJobRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateMonthlyReportJobCommand
        {
            Month = request.Month,
            Year = request.Year,
            BranchId = request.BranchId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(job => $"/api/monthly-reports/jobs/{job.Id}");
    }

    /// <summary>
    /// UC-177: Xem danh sách Monthly Report Jobs
    /// </summary>
    [HttpGet("jobs")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetMonthlyReportJobs(
        [FromQuery] Guid? branchId,
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyReportJobsQuery
        {
            BranchId = branchId,
            Month = month,
            Year = year,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-178: Xem trạng thái Monthly Report Job
    /// </summary>
    [HttpGet("jobs/{jobId:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetMonthlyReportJobById(
        [FromRoute] Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyReportJobByIdQuery
        {
            JobId = jobId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-175: Gom dữ liệu cho Monthly Report (trigger cho tất cả students trong job)
    /// </summary>
    [HttpPost("jobs/{jobId:guid}/aggregate")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AggregateMonthlyReportData(
        [FromRoute] Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var command = new AggregateMonthlyReportDataCommand { JobId = jobId };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get list of Monthly Reports with filters
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff,Parent")]
    public async Task<IResult> GetMonthlyReports(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] Guid? jobId,
        [FromQuery] int? month,
        [FromQuery] int? year,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyReportsQuery
        {
            StudentProfileId = studentProfileId,
            ClassId = classId,
            JobId = jobId,
            Month = month,
            Year = year,
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-179: Teacher xem draft Monthly Report
    /// UC-186: Parent/Student xem Monthly Report
    /// </summary>
    [HttpGet("{reportId:guid}")]
    [Authorize(Roles = "Teacher,Admin,ManagementStaff,Parent")]
    public async Task<IResult> GetMonthlyReportById(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMonthlyReportByIdQuery
        {
            ReportId = reportId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-176: Generate draft Monthly Report bằng AI
    /// </summary>
    [HttpPost("{reportId:guid}/generate-draft")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GenerateMonthlyReportDraft(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateMonthlyReportDraftCommand(reportId);

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-180: Teacher chỉnh sửa draft Monthly Report
    /// </summary>
    [HttpPut("{reportId:guid}/draft")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> UpdateMonthlyReportDraft(
        [FromRoute] Guid reportId,
        [FromBody] UpdateMonthlyReportDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateMonthlyReportDraftCommand
        {
            ReportId = reportId,
            DraftContent = request.DraftContent
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-181: Teacher submit Monthly Report
    /// </summary>
    [HttpPost("{reportId:guid}/submit")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> SubmitMonthlyReport(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new SubmitMonthlyReportCommand
        {
            ReportId = reportId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-182: Staff/Admin comment Monthly Report
    /// </summary>
    [HttpPost("{reportId:guid}/comments")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AddReportComment(
        [FromRoute] Guid reportId,
        [FromBody] AddReportCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new AddReportCommentCommand
        {
            ReportId = reportId,
            Content = request.Content
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(comment => $"/api/monthly-reports/{reportId}/comments/{comment.Id}");
    }

    /// <summary>
    /// UC-183: Staff/Admin approve Monthly Report
    /// </summary>
    [HttpPost("{reportId:guid}/approve")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ApproveMonthlyReport(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new ApproveMonthlyReportCommand
        {
            ReportId = reportId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-184: Staff/Admin reject Monthly Report
    /// </summary>
    [HttpPost("{reportId:guid}/reject")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> RejectMonthlyReport(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new RejectMonthlyReportCommand
        {
            ReportId = reportId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-185: Publish Monthly Report
    /// </summary>
    [HttpPost("{reportId:guid}/publish")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> PublishMonthlyReport(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new PublishMonthlyReportCommand
        {
            ReportId = reportId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{reportId:guid}/unpublish")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UnpublishMonthlyReport(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new UnpublishMonthlyReportCommand
        {
            ReportId = reportId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Generate or regenerate PDF for a Monthly Report (on-demand)
    /// </summary>
    [HttpPost("{reportId:guid}/generate-pdf")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GenerateMonthlyReportPdfResponse))]
    public async Task<IResult> GenerateMonthlyReportPdf(
        [FromRoute] Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateMonthlyReportPdfCommand(reportId);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
