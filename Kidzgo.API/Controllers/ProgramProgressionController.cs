using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.ProgramProgressions.ApproveProgramProgressionAssessment;
using Kidzgo.Application.ProgramProgressions.BulkApproveProgramProgressionAssessments;
using Kidzgo.Application.ProgramProgressions.CancelProgramProgressionSchedule;
using Kidzgo.Application.ProgramProgressions.CreateProgramProgressionAssessment;
using Kidzgo.Application.ProgramProgressions.CreateProgramProgressionSchedule;
using Kidzgo.Application.ProgramProgressions.CreateProgramProgressionRule;
using Kidzgo.Application.ProgramProgressions.GetMyProgramProgressionSchedules;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessmentById;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessments;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleAvailability;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionScheduleById;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionSchedules;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionRuleById;
using Kidzgo.Application.ProgramProgressions.GetProgramProgressionRules;
using Kidzgo.Application.ProgramProgressions.MarkProgramProgressionScheduleParticipantNoShow;
using Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionSchedule;
using Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionAssessment;
using Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionRule;
using Kidzgo.Domain.ProgramProgressions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/program-progressions")]
[ApiController]
[Authorize]
public class ProgramProgressionController : ControllerBase
{
    private readonly ISender _mediator;

    public ProgramProgressionController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("rules")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRules(
        [FromQuery] Guid? sourceProgramId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramProgressionRulesQuery
        {
            SourceProgramId = sourceProgramId,
            IsActive = isActive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("rules/{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRuleById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgramProgressionRuleByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("rules")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> CreateRule(
        [FromBody] SaveProgramProgressionRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProgramProgressionRuleCommand
        {
            SourceProgramId = request.SourceProgramId,
            TargetProgramId = request.TargetProgramId,
            Method = request.Method,
            MinimumShieldCount = request.MinimumShieldCount,
            MinimumSkillShieldCount = request.MinimumSkillShieldCount,
            MinimumOverallScore = request.MinimumOverallScore,
            CarryOverRemainingSessions = request.CarryOverRemainingSessions,
            StopCurrentEnrollmentOnApproval = request.StopCurrentEnrollmentOnApproval,
            IsActive = request.IsActive,
            Notes = request.Notes,
            ShieldMappings = request.ShieldMappings,
            ClassificationBands = request.ClassificationBands
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(rule => $"/api/program-progressions/rules/{rule.Id}");
    }

    [HttpPut("rules/{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> UpdateRule(
        Guid id,
        [FromBody] SaveProgramProgressionRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProgramProgressionRuleCommand
        {
            Id = id,
            SourceProgramId = request.SourceProgramId,
            TargetProgramId = request.TargetProgramId,
            Method = request.Method,
            MinimumShieldCount = request.MinimumShieldCount,
            MinimumSkillShieldCount = request.MinimumSkillShieldCount,
            MinimumOverallScore = request.MinimumOverallScore,
            CarryOverRemainingSessions = request.CarryOverRemainingSessions,
            StopCurrentEnrollmentOnApproval = request.StopCurrentEnrollmentOnApproval,
            IsActive = request.IsActive,
            Notes = request.Notes,
            ShieldMappings = request.ShieldMappings,
            ClassificationBands = request.ClassificationBands
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("schedules/availability")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetScheduleAvailability(
        [FromQuery] Guid sourceClassId,
        [FromQuery] DateTime scheduledAt,
        [FromQuery] int? durationMinutes,
        [FromQuery] Guid? excludeScheduleId,
        [FromQuery] bool includeUnavailable = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramProgressionScheduleAvailabilityQuery
        {
            SourceClassId = sourceClassId,
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            ExcludeScheduleId = excludeScheduleId,
            IncludeUnavailable = includeUnavailable
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("schedules")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetSchedules(
        [FromQuery] Guid? sourceClassId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? assignedTeacherUserId,
        [FromQuery] ProgramProgressionScheduleStatus? status,
        [FromQuery] ProgramProgressionScheduleParticipantStatus? participantStatus,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramProgressionSchedulesQuery
        {
            SourceClassId = sourceClassId,
            StudentProfileId = studentProfileId,
            AssignedTeacherUserId = assignedTeacherUserId,
            Status = status,
            ParticipantStatus = participantStatus,
            From = from,
            To = to,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("schedules/{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetScheduleById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgramProgressionScheduleByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("schedules")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> CreateSchedule(
        [FromBody] SaveProgramProgressionScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProgramProgressionScheduleCommand
        {
            SourceClassId = request.SourceClassId,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            RoomId = request.RoomId,
            AssignedTeacherUserId = request.AssignedTeacherUserId,
            Notes = request.Notes,
            StudentProfileIds = request.StudentProfileIds
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(schedule => $"/api/program-progressions/schedules/{schedule.Id}");
    }

    [HttpPut("schedules/{id:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> UpdateSchedule(
        Guid id,
        [FromBody] SaveProgramProgressionScheduleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProgramProgressionScheduleCommand
        {
            Id = id,
            ScheduledAt = request.ScheduledAt,
            DurationMinutes = request.DurationMinutes,
            RoomId = request.RoomId,
            AssignedTeacherUserId = request.AssignedTeacherUserId,
            Notes = request.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("schedules/{id:guid}/cancel")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> CancelSchedule(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CancelProgramProgressionScheduleCommand
        {
            Id = id
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("schedules/participants/{participantId:guid}/no-show")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> MarkParticipantNoShow(
        Guid participantId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MarkProgramProgressionScheduleParticipantNoShowCommand
        {
            ParticipantId = participantId
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpGet("my-assessment-schedules")]
    [Authorize(Roles = "Teacher,Student,Parent")]
    public async Task<IResult> GetMyAssessmentSchedules(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] ProgramProgressionScheduleStatus? status,
        [FromQuery] ProgramProgressionScheduleParticipantStatus? participantStatus,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMyProgramProgressionSchedulesQuery
        {
            StudentProfileId = studentProfileId,
            Status = status,
            ParticipantStatus = participantStatus,
            From = from,
            To = to,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("assessments")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetAssessments(
        [FromQuery] Guid? sourceRegistrationId,
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? sourceProgramId,
        [FromQuery] ProgramProgressionMethod? method,
        [FromQuery] ProgramProgressionAssessmentStatus? status,
        [FromQuery] bool? isEligible,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProgramProgressionAssessmentsQuery
        {
            SourceRegistrationId = sourceRegistrationId,
            StudentProfileId = studentProfileId,
            SourceProgramId = sourceProgramId,
            Method = method,
            Status = status,
            IsEligible = isEligible,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("assessments/{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetAssessmentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetProgramProgressionAssessmentByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("assessments")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateAssessment(
        [FromBody] SaveProgramProgressionAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProgramProgressionAssessmentCommand
        {
            SourceRegistrationId = request.SourceRegistrationId,
            ScheduleParticipantId = request.ScheduleParticipantId,
            AssessmentDate = request.AssessmentDate,
            PassedInClass = request.PassedInClass,
            ListeningScore = request.ListeningScore,
            SpeakingScore = request.SpeakingScore,
            ReadingWritingScore = request.ReadingWritingScore,
            ReadingScore = request.ReadingScore,
            WritingScore = request.WritingScore,
            Comment = request.Comment,
            AttachmentUrls = request.AttachmentUrls
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(assessment => $"/api/program-progressions/assessments/{assessment.Id}");
    }

    [HttpPut("assessments/{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateAssessment(
        Guid id,
        [FromBody] SaveProgramProgressionAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateProgramProgressionAssessmentCommand
        {
            Id = id,
            AssessmentDate = request.AssessmentDate,
            PassedInClass = request.PassedInClass,
            ListeningScore = request.ListeningScore,
            SpeakingScore = request.SpeakingScore,
            ReadingWritingScore = request.ReadingWritingScore,
            ReadingScore = request.ReadingScore,
            WritingScore = request.WritingScore,
            Comment = request.Comment,
            AttachmentUrls = request.AttachmentUrls
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("assessments/{id:guid}/approve")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> ApproveAssessment(
        Guid id,
        [FromBody] ApproveProgramProgressionAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveProgramProgressionAssessmentCommand
        {
            AssessmentId = id,
            TuitionPlanId = request.TuitionPlanId,
            ApprovalNote = request.ApprovalNote
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("assessments/bulk-approve")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> BulkApproveAssessments(
        [FromBody] BulkApproveProgramProgressionAssessmentsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BulkApproveProgramProgressionAssessmentsCommand
        {
            Items = request.Items.Select(item => new BulkApproveProgramProgressionAssessmentItem
            {
                AssessmentId = item.AssessmentId,
                TuitionPlanId = item.TuitionPlanId,
                ApprovalNote = item.ApprovalNote
            }).ToList()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
