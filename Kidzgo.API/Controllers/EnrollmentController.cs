using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Enrollments.AddEnrollmentScheduleSegment;
using Kidzgo.Application.Enrollments.AssignTuitionPlan;
using Kidzgo.Application.Enrollments.BackfillStudentSessionAssignments;
using Kidzgo.Application.Enrollments.CreateEnrollment;
using Kidzgo.Application.Enrollments.DropEnrollment;
using Kidzgo.Application.Enrollments.GetEnrollmentById;
using Kidzgo.Application.Enrollments.GetEnrollments;
using Kidzgo.Application.Enrollments.GetStudentEnrollmentHistory;
using Kidzgo.Application.Enrollments.PauseEnrollment;
using Kidzgo.Application.Enrollments.ReactivateEnrollment;
using Kidzgo.Application.Enrollments.UpdateEnrollment;
using Kidzgo.Domain.Classes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/enrollments")]
[ApiController]
public class EnrollmentController : ControllerBase
{
    private readonly ISender _mediator;

    public EnrollmentController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-067: Ghi danh học sinh vào Class
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateEnrollment(
        [FromBody] CreateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateEnrollmentCommand
        {
            ClassId = request.ClassId,
            StudentProfileId = request.StudentProfileId,
            EnrollDate = request.EnrollDate,
            RegistrationId = request.RegistrationId,
            TuitionPlanId = request.TuitionPlanId,
            Track = request.Track,
            WeeklyPattern = request.WeeklyPattern,
            AllowCrossBranchEnrollment = request.AllowCrossBranchEnrollment
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(e => $"/api/enrollments/{e.Id}");
    }

    [HttpPost("backfill-session-assignments")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> BackfillStudentSessionAssignments(
        [FromBody] BackfillStudentSessionAssignmentsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BackfillStudentSessionAssignmentsCommand
        {
            EnrollmentId = request.EnrollmentId,
            ClassId = request.ClassId,
            StudentProfileId = request.StudentProfileId,
            BatchSize = request.BatchSize
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Add a dated schedule segment for a supplementary enrollment.
    [HttpPost("{id:guid}/schedule-segments")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AddScheduleSegment(
        Guid id,
        [FromBody] AddEnrollmentScheduleSegmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddEnrollmentScheduleSegmentCommand
        {
            EnrollmentId = id,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            WeeklyPattern = request.WeeklyPattern,
            ClearWeeklyPattern = request.ClearWeeklyPattern
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-068: Xem danh sach hoc sinh trong Class
    /// <param name="classId">Filter by class ID</param>
    /// <param name="studentProfileId">Filter by student profile ID</param>
    /// <param name="status">Enrollment status: Active, Paused, or Dropped</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    // [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetEnrollments(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        EnrollmentStatus? enrollmentStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<EnrollmentStatus>(status, true, out var parsedStatus))
        {
            enrollmentStatus = parsedStatus;
        }

        var query = new GetEnrollmentsQuery
        {
            ClassId = classId,
            StudentProfileId = studentProfileId,
            Status = enrollmentStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-069: Xem chi tiết Enrollment
    [HttpGet("{id:guid}")]
    // [Authorize(Roles = "Admin,Staff")]
    public async Task<IResult> GetEnrollmentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetEnrollmentByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-070: Cập nhật Enrollment
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateEnrollment(
        Guid id,
        [FromBody] UpdateEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEnrollmentCommand
        {
            Id = id,
            EnrollDate = request.EnrollDate,
            TuitionPlanId = request.TuitionPlanId,
            Track = request.Track,
            WeeklyPattern = request.WeeklyPattern,
            ClearWeeklyPattern = request.ClearWeeklyPattern
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-071: Tạm dừng Enrollment (PAUSED)
    [HttpPatch("{id:guid}/pause")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> PauseEnrollment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new PauseEnrollmentCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-072: Hủy Enrollment (DROPPED)
    [HttpPatch("{id:guid}/drop")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DropEnrollment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DropEnrollmentCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-073: Kích hoạt lại Enrollment
    [HttpPatch("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ReactivateEnrollment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ReactivateEnrollmentCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-074: Gán Tuition Plan cho Enrollment
    [HttpPatch("{id:guid}/assign-tuition-plan")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AssignTuitionPlan(
        Guid id,
        [FromBody] AssignTuitionPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignTuitionPlanCommand
        {
            Id = id,
            TuitionPlanId = request.TuitionPlanId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-075: Xem lịch sử Enrollment của học sinh
    /// <param name="studentProfileId">Student profile ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet("student/{studentProfileId:guid}/history")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetStudentEnrollmentHistory(
        Guid studentProfileId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentEnrollmentHistoryQuery
        {
            StudentProfileId = studentProfileId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

