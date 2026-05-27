using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.LessonPlanTemplates.CreateLessonPlanTemplate;
using Kidzgo.Application.LessonPlanTemplates.DeleteLessonPlanTemplate;
using Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplateById;
using Kidzgo.Application.LessonPlanTemplates.GetLessonPlanTemplates;
using Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplates;
using Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;
using Kidzgo.Application.LessonPlanTemplates.MoveLessonPlanTemplateUnit;
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
            SyllabusId = request.SyllabusId,
            ModuleId = request.ModuleId,
            LessonPlanUnitId = request.LessonPlanUnitId,
            OrderIndexInUnit = request.OrderIndexInUnit,
            Title = request.Title,
            SessionIndex = request.SessionIndex,
            SessionOrder = request.SessionOrder,
            SyllabusMetadata = request.SyllabusMetadata,
            SyllabusContent = request.SyllabusContent,
            Objectives = request.Objectives,
            LanguageContent = request.LanguageContent,
            Vocabulary = request.Vocabulary,
            Grammar = request.Grammar,
            TeachingMethodology = request.TeachingMethodology,
            TeacherMaterials = request.TeacherMaterials,
            StudentMaterials = request.StudentMaterials,
            Procedure = request.Procedure,
            Evaluation = request.Evaluation,
            SourceFileName = request.SourceFileName,
            Attachment = request.Attachment
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/lesson-plan-templates/{r.Id}");
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetLessonPlanTemplates(
        [FromQuery] Guid? syllabusId,
        [FromQuery] Guid? moduleId,
        [FromQuery] string? title,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLessonPlanTemplatesQuery
        {
            SyllabusId = syllabusId,
            ModuleId = moduleId,
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
        [FromQuery] Guid syllabusId,
        [FromQuery] Guid? moduleId,
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
            SyllabusId = syllabusId,
            ModuleId = moduleId,
            OverwriteExisting = overwriteExisting,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Import one lesson plan Word file for a specific module.
    /// </summary>
    /// <remarks>
    /// Use one lesson docx from a UNIT or REVISION folder. The importer maps headings like Objectives, Vocabulary, Grammar, Procedure, Evaluation, and Homework.
    /// </remarks>
    [HttpPost("import-word")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(41_943_040)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ImportLessonPlanTemplateFromWord(
        [FromQuery] Guid syllabusId,
        [FromQuery] Guid moduleId,
        [FromQuery] Guid? lessonPlanUnitId,
        [FromQuery] int? sessionIndexOverride,
        IFormFile file,
        [FromQuery] bool overwriteExisting = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var result = await _mediator.Send(new ImportLessonPlanTemplateFromWordCommand
        {
            SyllabusId = syllabusId,
            ModuleId = moduleId,
            LessonPlanUnitId = lessonPlanUnitId,
            SessionIndexOverride = sessionIndexOverride,
            OverwriteExisting = overwriteExisting,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        }, cancellationToken);

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
            SyllabusId = request.SyllabusId,
            ModuleId = request.ModuleId,
            LessonPlanUnitId = request.LessonPlanUnitId,
            OrderIndexInUnit = request.OrderIndexInUnit,
            Title = request.Title,
            SessionIndex = request.SessionIndex,
            SessionOrder = request.SessionOrder,
            SyllabusMetadata = request.SyllabusMetadata,
            SyllabusContent = request.SyllabusContent,
            Objectives = request.Objectives,
            LanguageContent = request.LanguageContent,
            Vocabulary = request.Vocabulary,
            Grammar = request.Grammar,
            TeachingMethodology = request.TeachingMethodology,
            TeacherMaterials = request.TeacherMaterials,
            StudentMaterials = request.StudentMaterials,
            Procedure = request.Procedure,
            Evaluation = request.Evaluation,
            SourceFileName = request.SourceFileName,
            Attachment = request.Attachment,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> DeleteLessonPlanTemplate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new DeleteLessonPlanTemplateCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/unit")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> MoveLessonPlanTemplateUnit(
        Guid id,
        [FromBody] MoveLessonPlanTemplateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new MoveLessonPlanTemplateUnitCommand
        {
            Id = id,
            LessonPlanUnitId = request.LessonPlanUnitId,
            OrderIndexInUnit = request.OrderIndexInUnit
        }, cancellationToken);

        return result.MatchOk();
    }
}
