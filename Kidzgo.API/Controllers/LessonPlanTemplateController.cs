using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;
using Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;
using Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;
using Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplates;
using Kidzgo.Application.LessonPlanTemplates.UpdateLessonPlanTemplate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/lesson-plan-templates")]
[ApiController]
[Authorize]
public class LessonPlanTemplateController : ControllerBase
{
    private readonly ISender _mediator;

    public LessonPlanTemplateController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> CreateLessonPlanTemplate(
        [FromBody] CreateLessonPlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateLessonPlanTemplateCommand
        {
            ProgramId = request.ProgramId,
            ModuleId = request.ModuleId,
            Title = request.Title,
            Level = request.Level,
            SessionIndex = request.SessionIndex,
            SessionOrder = request.SessionOrder,
            SyllabusMetadata = request.SyllabusMetadata,
            SyllabusContent = request.SyllabusContent,
            SourceFileName = request.SourceFileName,
            Attachment = request.Attachment
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/lesson-plan-templates/{r.Id}");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanTemplateById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetLessonPlanTemplateByIdQuery
        {
            Id = id
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanTemplates(
        [FromQuery] Guid? programId,
        [FromQuery] string? level,
        [FromQuery] string? title,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLessonPlanTemplatesQuery
        {
            ProgramId = programId,
            Level = level,
            Title = title,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("import")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(20_971_520)]
    public async Task<IResult> ImportLessonPlanTemplates(
        [FromQuery] Guid? programId,
        [FromQuery] string? level,
        IFormFile file,
        [FromQuery] bool overwriteExisting = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new ImportLessonPlanTemplatesFromFileCommand
        {
            ProgramId = programId,
            Level = level,
            OverwriteExisting = overwriteExisting,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> UpdateLessonPlanTemplate(
        Guid id,
        [FromBody] UpdateLessonPlanTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLessonPlanTemplateCommand
        {
            Id = id,
            ModuleId = request.ModuleId,
            Level = request.Level,
            Title = request.Title,
            SessionIndex = request.SessionIndex,
            SessionOrder = request.SessionOrder,
            SyllabusMetadata = request.SyllabusMetadata,
            SyllabusContent = request.SyllabusContent,
            SourceFileName = request.SourceFileName,
            Attachment = request.Attachment,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
