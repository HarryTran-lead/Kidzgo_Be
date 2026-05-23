using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Sessions.CancelSession;
using Kidzgo.Application.Sessions.ChangeSessionRoom;
using Kidzgo.Application.Sessions.ChangeSessionTeacher;
using Kidzgo.Application.Sessions.CheckSessionConflicts;
using Kidzgo.Application.Sessions.CompleteSession;
using Kidzgo.Application.Sessions.CreateSession;
using Kidzgo.Application.Sessions.CreateSessionRole;
using Kidzgo.Application.Sessions.DeleteSessionRole;
using Kidzgo.Application.Sessions.GetSessionById;
using Kidzgo.Application.Sessions.GetSessionLessonPlanDocument;
using Kidzgo.Application.Sessions.GetSessionRoles;
using Kidzgo.Application.Sessions.GetSessions;
using Kidzgo.Application.Sessions.GetTeachingLogBySession;
using Kidzgo.Application.Sessions.GenerateSessionsFromPattern;
using Kidzgo.Application.Sessions.UpdateSession;
using Kidzgo.Application.Sessions.UpdateSessionColor;
using Kidzgo.Application.Sessions.UpdateSessionSectionType;
using Kidzgo.Application.Sessions.UpdateSessionRole;
using Kidzgo.Application.Sessions.UpdateTeachingLog;
using Kidzgo.Application.Sessions.GetSessionAvailability;
using Kidzgo.Application.Sessions.UpdateSessionsByClass;
using Kidzgo.Application.Sessions.SubmitTeachingLog;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/sessions")]
[ApiController]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISender _mediator;

    public SessionController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-076: Sinh Sessions tự động từ weekly schedule cho Class/Program
    [HttpPost("generate-from-pattern")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GenerateSessionsFromPattern(
        [FromBody] GenerateSessionsFromPatternRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GenerateSessionsFromPatternCommand
        {
            ClassId = request.ClassId,
            OnlyFutureSessions = request.OnlyFutureSessions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-076 (manual): Tạo Session thủ công
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var participationType = Enum.TryParse<Domain.Sessions.ParticipationType>(
            request.ParticipationType, true, out var parsedType)
            ? parsedType
            : Domain.Sessions.ParticipationType.Main;
        var sectionType = Enum.TryParse<Domain.Sessions.SectionType>(
            request.SectionType, true, out var parsedSectionType)
            ? parsedSectionType
            : Domain.Sessions.SectionType.Normal;

        var command = new CreateSessionCommand
        {
            ClassId = request.ClassId,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId,
            SlotTypeId = request.SlotTypeId,
            ParticipationType = participationType,
            SectionType = sectionType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(s => $"/api/sessions/{s.Id}");
    }

    /// UC-077: Xem danh sách Sessions (Admin/Staff)
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetSessions(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? branchId,
        [FromQuery] Domain.Sessions.SessionStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSessionsQuery
        {
            ClassId = classId,
            BranchId = branchId,
            Status = status,
            From = from,
            To = to,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-078: Xem chi tiết Session
    [HttpGet("{sessionId:guid}")]
    public async Task<IResult> GetSessionById(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionByIdQuery
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{sessionId:guid}/lesson-plan-document")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetSessionLessonPlanDocument(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSessionLessonPlanDocumentQuery
        {
            SessionId = sessionId
        }, cancellationToken);

        return result.MatchOk();
    }

    /// UC-079: Cập nhật Session (giờ/phòng/giáo viên)
    [HttpPut("{sessionId:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateSession(
        Guid sessionId,
        [FromBody] UpdateSessionRequest request,
        CancellationToken cancellationToken)
    {
        var participationType = Enum.TryParse<Domain.Sessions.ParticipationType>(
            request.ParticipationType, true, out var parsedType)
            ? parsedType
            : Domain.Sessions.ParticipationType.Main;
        var sectionType = Enum.TryParse<Domain.Sessions.SectionType>(
            request.SectionType, true, out var parsedSectionType)
            ? parsedSectionType
            : Domain.Sessions.SectionType.Normal;

        var command = new UpdateSessionCommand
        {
            SessionId = sessionId,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId,
            SlotTypeId = request.SlotTypeId,
            ParticipationType = participationType,
            SectionType = sectionType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Update section type for one session only.
    /// Teacher can update only on the same Vietnam date as the session date.
    [HttpPatch("{sessionId:guid}/section-type")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> UpdateSessionSectionType(
        Guid sessionId,
        [FromBody] UpdateSessionSectionTypeRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<Domain.Sessions.SectionType>(request.SectionType, true, out var sectionType))
        {
            return CustomResults.Problem(Result.Failure(SessionErrors.InvalidSectionType(request.SectionType)));
        }

        var command = new UpdateSessionSectionTypeCommand
        {
            SessionId = sessionId,
            SectionType = sectionType,
            IsPrivilegedUser = User.IsInRole("Admin") || User.IsInRole("ManagementStaff")
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Change classroom for one or many ongoing/future sessions; time and other fields are preserved.
    [HttpPatch("change-room")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ChangeSessionRoom(
        [FromBody] ChangeSessionRoomRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeSessionRoomCommand
        {
            SessionIds = BuildSessionIds(request.SessionId, request.SessionIds),
            RoomId = request.RoomId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Change main teacher or assistant teacher for one or many ongoing/future sessions; other fields are preserved.
    [HttpPatch("change-teacher")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ChangeSessionTeacher(
        [FromBody] ChangeSessionTeacherRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseSessionTeacherRole(request.Role, out var role))
        {
            return CustomResults.Problem(Result.Failure(SessionErrors.InvalidTeacherRole(request.Role)));
        }

        var command = new ChangeSessionTeacherCommand
        {
            SessionIds = BuildSessionIds(request.SessionId, request.SessionIds),
            TeacherId = request.TeacherId,
            Role = role
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-079-Color: Cap nhat mau hien thi cua Session
    [HttpPatch("{id:guid}/color")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateSessionColor(
        Guid id,
        [FromBody] UpdateSessionColorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionColorCommand
        {
            SessionId = id,
            Color = request.Color
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return CustomResults.Problem(result);
        }

        return Results.Ok(new { isSuccess = true });
    }

    /// UC-079-Bulk: Cap nhat nhieu Sessions cua mot Class cung luc
    [HttpPut("by-class")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateSessionsByClass(
        [FromBody] UpdateSessionsByClassRequest request,
        CancellationToken cancellationToken)
    {
        var participationType = request.ParticipationType != null
            ? (Enum.TryParse<Domain.Sessions.ParticipationType>(
                request.ParticipationType, true, out var parsedType)
                ? parsedType
                : Domain.Sessions.ParticipationType.Main)
            : (Domain.Sessions.ParticipationType?)null;

        var filterByStatus = request.FilterByStatus != null
            ? (Enum.TryParse<Domain.Sessions.SessionStatus>(
                request.FilterByStatus, true, out var parsedStatus)
                ? parsedStatus
                : (Domain.Sessions.SessionStatus?)null)
            : null;

        var sectionType = request.SectionType != null
            ? (Enum.TryParse<Domain.Sessions.SectionType>(
                request.SectionType, true, out var parsedSectionType)
                ? parsedSectionType
                : (Domain.Sessions.SectionType?)null)
            : null;

        var command = new UpdateSessionsByClassCommand
        {
            ClassId = request.ClassId,
            SessionIds = request.SessionIds,
            FilterByStatus = filterByStatus,
            FromDate = request.FromDate,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId,
            SlotTypeId = request.SlotTypeId,
            ParticipationType = participationType,
            SectionType = sectionType
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-080: Hủy Session (CANCELLED)
    [HttpPost("{sessionId:guid}/cancel")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CancelSession(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var command = new CancelSessionCommand
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-081: Đánh dấu Session hoàn thành (COMPLETED)
    [HttpPost("{sessionId:guid}/complete")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CompleteSession(
        Guid sessionId,
        [FromBody] CompleteSessionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CompleteSessionCommand
        {
            SessionId = sessionId,
            ActualDatetime = request.ActualDatetime
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{sessionId:guid}/teaching-log")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> SubmitTeachingLog(
        Guid sessionId,
        [FromBody] SubmitTeachingLogRequest request,
        CancellationToken cancellationToken)
    {
        var teachingType = Enum.TryParse<TeachingLogTeachingType>(
            request.ActualTeachingType,
            true,
            out var parsedTeachingType)
            ? parsedTeachingType
            : TeachingLogTeachingType.Normal;

        var command = new SubmitTeachingLogCommand
        {
            SessionId = sessionId,
            ActualLessonPlanTemplateId = request.ActualLessonPlanTemplateId,
            ActualTeachingType = teachingType,
            ProgressStatus = request.ProgressStatus,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNote = request.TeacherNote
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(x => $"/api/sessions/{sessionId}/teaching-log");
    }

    [HttpGet("{sessionId:guid}/teaching-log")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetTeachingLog(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetTeachingLogBySessionQuery
        {
            SessionId = sessionId
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPut("{sessionId:guid}/teaching-log")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> UpdateTeachingLog(
        Guid sessionId,
        [FromBody] SubmitTeachingLogRequest request,
        CancellationToken cancellationToken)
    {
        var teachingType = Enum.TryParse<TeachingLogTeachingType>(
            request.ActualTeachingType,
            true,
            out var parsedTeachingType)
            ? parsedTeachingType
            : TeachingLogTeachingType.Normal;

        var command = new UpdateTeachingLogCommand
        {
            SessionId = sessionId,
            ActualLessonPlanTemplateId = request.ActualLessonPlanTemplateId,
            ActualTeachingType = teachingType,
            ProgressStatus = request.ProgressStatus,
            ActualContent = request.ActualContent,
            ActualHomework = request.ActualHomework,
            TeacherNote = request.TeacherNote
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Trả về rooms + teachers kèm isAvailable dựa trên class session conflicts
    [HttpGet("availability")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetSessionAvailability(
        [FromQuery] DateTime scheduledAt,
        [FromQuery] int? durationMinutes,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? excludeSessionId,
        [FromQuery] bool includeUnavailable = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSessionAvailabilityQuery
        {
            ScheduledAt = scheduledAt,
            DurationMinutes = durationMinutes,
            BranchId = branchId,
            ExcludeSessionId = excludeSessionId,
            IncludeUnavailable = includeUnavailable
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-082: Kiểm tra xung đột phòng/giáo viên
    /// UC-083: Gợi ý phòng/slot khác khi xung đột
    [HttpPost("check-conflicts")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CheckSessionConflicts(
        [FromBody] CheckSessionConflictsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new CheckSessionConflictsQuery
        {
            SessionId = request.SessionId,
            PlannedDatetime = request.PlannedDatetime,
            DurationMinutes = request.DurationMinutes,
            PlannedRoomId = request.PlannedRoomId,
            PlannedTeacherId = request.PlannedTeacherId,
            PlannedAssistantId = request.PlannedAssistantId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-085: Tạo Session Role (MAIN_TEACHER/ASSISTANT/CLUB/WORKSHOP)
    [HttpPost("{sessionId:guid}/roles")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateSessionRole(
        Guid sessionId,
        [FromBody] CreateSessionRoleRequest request,
        CancellationToken cancellationToken)
    {
        var roleType = Enum.TryParse<Domain.Payroll.SessionRoleType>(
            request.RoleType, true, out var parsedType)
            ? parsedType
            : throw new ArgumentException($"Invalid role type: {request.RoleType}");

        var command = new CreateSessionRoleCommand
        {
            SessionId = sessionId,
            StaffUserId = request.StaffUserId,
            RoleType = roleType,
            PayableUnitPrice = request.PayableUnitPrice,
            PayableAllowance = request.PayableAllowance
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(sr => $"/api/sessions/{sessionId}/roles/{sr.Id}");
    }

    /// UC-086: Xem danh sách Session Roles của Session
    [HttpGet("{sessionId:guid}/roles")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetSessionRoles(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var query = new GetSessionRolesQuery
        {
            SessionId = sessionId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-087: Cập nhật Session Role
    /// UC-089: Thiết lập payable_unit_price cho Session Role
    /// UC-090: Thiết lập payable_allowance cho Session Role
    [HttpPut("roles/{sessionRoleId:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateSessionRole(
        Guid sessionRoleId,
        [FromBody] UpdateSessionRoleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateSessionRoleCommand
        {
            SessionRoleId = sessionRoleId,
            PayableUnitPrice = request.PayableUnitPrice,
            PayableAllowance = request.PayableAllowance
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-088: Xóa Session Role
    [HttpDelete("roles/{sessionRoleId:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> DeleteSessionRole(
        Guid sessionRoleId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteSessionRoleCommand
        {
            SessionRoleId = sessionRoleId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    private static List<Guid> BuildSessionIds(Guid? sessionId, List<Guid>? sessionIds)
    {
        var ids = sessionIds?.Where(id => id != Guid.Empty).ToList() ?? new List<Guid>();

        if (sessionId.HasValue && sessionId.Value != Guid.Empty)
        {
            ids.Add(sessionId.Value);
        }

        return ids.Distinct().ToList();
    }

    private static bool TryParseSessionTeacherRole(string? role, out SessionTeacherRole parsedRole)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            parsedRole = default;
            return false;
        }

        parsedRole = role.Trim().ToLowerInvariant() switch
        {
            "mainteacher" or "main" or "teacher" => SessionTeacherRole.MainTeacher,
            "assistant" or "assistantteacher" => SessionTeacherRole.Assistant,
            _ => default
        };

        return role.Trim().ToLowerInvariant() is
            "mainteacher" or
            "main" or
            "teacher" or
            "assistant" or
            "assistantteacher";
    }
}
