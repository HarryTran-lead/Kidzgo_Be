using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Syllabuses.CreateSyllabus;
using Kidzgo.Application.Syllabuses.GetSyllabusById;
using Kidzgo.Application.Syllabuses.GetSyllabuses;
using Kidzgo.Application.Syllabuses.ImportCurriculumArchive;
using Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;
using Kidzgo.Application.Syllabuses.UpdateSyllabus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/syllabuses")]
[ApiController]
[Authorize]
public class SyllabusController(ISender mediator) : ControllerBase
{
    /// <summary>
    /// Create a syllabus manually.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Create([FromBody] CreateSyllabusRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateSyllabusCommand
        {
            ProgramId = request.ProgramId,
            LevelId = request.LevelId,
            Code = request.Code,
            Version = request.Version,
            Title = request.Title,
            Edition = request.Edition,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            PacingSchemeJson = request.PacingSchemeJson,
            Overview = request.Overview,
            OverallObjectives = request.OverallObjectives,
            SpecificObjectives = request.SpecificObjectives,
            EthicsAndAttitudes = request.EthicsAndAttitudes,
            BookOverview = request.BookOverview,
            TotalPeriods = request.TotalPeriods,
            MinutesPerPeriod = request.MinutesPerPeriod,
            TotalLessons = request.TotalLessons,
            SourceFileName = request.SourceFileName,
            AttachmentUrl = request.AttachmentUrl,
            RawContentJson = request.RawContentJson,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/syllabuses/{x.Id}");
    }

    /// <summary>
    /// List syllabuses with pagination and filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> GetList(
        [FromQuery] Guid? programId,
        [FromQuery] Guid? levelId,
        [FromQuery] string? searchTerm,
        [FromQuery] bool? isActive,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSyllabusesQuery
        {
            ProgramId = programId,
            LevelId = levelId,
            SearchTerm = searchTerm,
            IsActive = isActive,
            IncludeDeleted = includeDeleted,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, cancellationToken);

        return result.MatchOk();
    }

    /// <summary>
    /// View syllabus detail including units, lessons, resources, and session templates.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSyllabusByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Update syllabus metadata.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> Update(Guid id, [FromBody] UpdateSyllabusRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSyllabusCommand
        {
            Id = id,
            Code = request.Code,
            Version = request.Version,
            Title = request.Title,
            Edition = request.Edition,
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            PacingSchemeJson = request.PacingSchemeJson,
            Overview = request.Overview,
            OverallObjectives = request.OverallObjectives,
            SpecificObjectives = request.SpecificObjectives,
            EthicsAndAttitudes = request.EthicsAndAttitudes,
            BookOverview = request.BookOverview,
            TotalPeriods = request.TotalPeriods,
            MinutesPerPeriod = request.MinutesPerPeriod,
            TotalLessons = request.TotalLessons,
            SourceFileName = request.SourceFileName,
            AttachmentUrl = request.AttachmentUrl,
            RawContentJson = request.RawContentJson,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchOk();
    }

    /// <summary>
    /// Import one syllabus Word file into syllabus, syllabus lessons, units, resources, and session templates.
    /// </summary>
    /// <remarks>
    /// Use the file from the PPCT folder. Query params programId, levelId, code, and version are required.
    /// </remarks>
    [HttpPost("import-word")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(41_943_040)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ImportWord(
        [FromQuery] Guid programId,
        [FromQuery] Guid levelId,
        [FromQuery] string code,
        [FromQuery] string version,
        IFormFile file,
        [FromQuery] bool overwriteExisting = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var result = await mediator.Send(new ImportSyllabusFromWordCommand
        {
            ProgramId = programId,
            LevelId = levelId,
            Code = code,
            Version = version,
            OverwriteExisting = overwriteExisting,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        }, cancellationToken);

        return result.MatchOk();
    }

    /// <summary>
    /// Import one curriculum zip archive that contains PPCT, UNIT, and REVISION folders.
    /// </summary>
    /// <remarks>
    /// The importer scans the PPCT folder for the syllabus docx, then scans UNIT and REVISION folders for lesson plan docx files.
    /// </remarks>
    [HttpPost("import-archive")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(536_870_912)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ImportArchive(
        [FromQuery] Guid programId,
        [FromQuery] Guid levelId,
        [FromQuery] string code,
        [FromQuery] string version,
        IFormFile file,
        [FromQuery] bool overwriteExisting = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var result = await mediator.Send(new ImportCurriculumArchiveCommand
        {
            ProgramId = programId,
            LevelId = levelId,
            Code = code,
            Version = version,
            OverwriteExisting = overwriteExisting,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        }, cancellationToken);

        return result.MatchOk();
    }
}
