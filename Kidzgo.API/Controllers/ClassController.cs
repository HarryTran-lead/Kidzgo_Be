using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Classes.AddClassScheduleSegment;
using Kidzgo.Application.Classes.AssignTeacher;
using Kidzgo.Application.Classes.ChangeClassStatus;
using Kidzgo.Application.Classes.CheckClassCapacity;
using Kidzgo.Application.Classes.CreateClass;
using Kidzgo.Application.Classes.DeleteClass;
using Kidzgo.Application.Classes.GetClassById;
using Kidzgo.Application.Classes.GetClasses;
using Kidzgo.Application.Classes.GetClassStudents;
using Kidzgo.Application.Classes.UpdateClass;
using Kidzgo.Application.Classes.UpdateClassColor;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/classes")]
[ApiController]
public class ClassController : ControllerBase
{
    private readonly ISender _mediator;

    public ClassController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// UC-057: Táº¡o Class
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateClass(
        [FromBody] CreateClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateClassCommand
        {
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            LevelId = request.LevelId,
            StartModuleId = request.StartModuleId,
            Code = request.Code,
            Title = request.Name ?? request.Title ?? request.Code,
            RoomId = request.RoomId,
            MainTeacherId = request.MainTeacherId,
            AssistantTeacherId = request.AssistantTeacherId,
            SlotTypeId = request.SlotTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate, 
            Capacity = request.Capacity,
            WeeklyScheduleSlots = request.WeeklyScheduleSlots,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(c => $"/api/classes/{c.Id}");
    }

    /// UC-058: Xem danh sÃ¡ch Classes
    /// <param name="branchId">Filter by branch ID</param>
    /// <param name="programId">Filter by program ID</param>
    /// <param name="studentId">Filter by enrolled student ID</param>
    /// <param name="status">Class status: Planned, Active, or Closed</param>
    /// <param name="searchTerm">Search by class code or title</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff,Parent")]
    public async Task<IResult> GetClasses(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] Guid? teacherId,
        [FromQuery] Guid? studentId,
        [FromQuery] string? status,
        [FromQuery] string? searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        Domain.Classes.ClassStatus? classStatus = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Classes.ClassStatus>(status, true, out var parsedStatus))
        {
            classStatus = parsedStatus;
        }

        var query = new GetClassesQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            TeacherId = teacherId,
            StudentId = studentId,
            Status = classStatus,
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-059: Xem chi tiáº¿t Class
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetClassById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetClassByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("{id:guid}/schedule-segments")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AddScheduleSegment(
        Guid id,
        [FromBody] AddClassScheduleSegmentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddClassScheduleSegmentCommand
        {
            ClassId = id,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            WeeklyScheduleSlots = request.WeeklyScheduleSlots,
            GenerateSessions = request.GenerateSessions,
            OnlyFutureSessions = request.OnlyFutureSessions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}/students")]
    [Authorize(Roles = "Admin,ManagementStaff,Teacher")]
    public async Task<IResult> GetClassStudents(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetClassStudentsQuery
        {
            ClassId = id,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// UC-060: Cáº­p nháº­t Class
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateClass(
        Guid id,
        [FromBody] UpdateClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassCommand
        {
            Id = id,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            LevelId = request.LevelId,
            StartModuleId = request.StartModuleId,
            Code = request.Code,
            Title = request.Name ?? request.Title ?? request.Code,
            RoomId = request.RoomId,
            MainTeacherId = request.MainTeacherId,
            AssistantTeacherId = request.AssistantTeacherId,
            SlotTypeId = request.SlotTypeId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            WeeklyScheduleSlots = request.WeeklyScheduleSlots,
            Description = request.Description
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-061: XÃ³a má»m Class (Set status = Closed)
    [HttpPatch("{classId:guid}/color")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateClassColor(
        Guid classId,
        [FromBody] UpdateClassColorRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateClassColorCommand
        {
            ClassId = classId,
            Color = request.Color
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return CustomResults.Problem(result);
        }

        return Results.Ok(new { isSuccess = true });
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> DeleteClass(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteClassCommand
        {
            Id = id
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-062: Thay Ä‘á»•i tráº¡ng thÃ¡i Class (PLANNED/ACTIVE/CLOSED)
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ChangeClassStatus(
        Guid id,
        [FromBody] ChangeClassStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeClassStatusCommand
        {
            Id = id,
            Status = request.Status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-063/064: GÃ¡n Main Teacher vÃ  Assistant Teacher cho Class
    [HttpPatch("{id:guid}/assign-teacher")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AssignTeacher(
        Guid id,
        [FromBody] AssignTeacherRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignTeacherCommand
        {
            ClassId = id,
            MainTeacherId = request.MainTeacherId,
            AssistantTeacherId = request.AssistantTeacherId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// UC-066: Kiá»ƒm tra capacity trÆ°á»›c khi ghi danh
    [HttpGet("{id:guid}/capacity")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CheckClassCapacity(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new CheckClassCapacityQuery
        {
            ClassId = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
