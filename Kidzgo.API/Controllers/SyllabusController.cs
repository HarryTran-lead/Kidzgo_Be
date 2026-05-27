using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Syllabuses.CreateSyllabus;
using Kidzgo.Application.Syllabuses.AddSyllabusSection;
using Kidzgo.Application.Syllabuses.AddSyllabusTableRow;
using Kidzgo.Application.Syllabuses.ArchiveSyllabusDocument;
using Kidzgo.Application.Syllabuses.DeleteSyllabusTableRow;
using Kidzgo.Application.Syllabuses.DeleteSyllabus;
using Kidzgo.Application.Syllabuses.GetCurriculumImportConfiguration;
using Kidzgo.Application.Syllabuses.GetSyllabusById;
using Kidzgo.Application.Syllabuses.GetSyllabusDocument;
using Kidzgo.Application.Syllabuses.GetSyllabusVersions;
using Kidzgo.Application.Syllabuses.GetSyllabuses;
using Kidzgo.Application.Syllabuses.GetSyllabusUnitLessonPlans;
using Kidzgo.Application.Syllabuses.ImportCurriculumArchive;
using Kidzgo.Application.Syllabuses.ImportSyllabusCommit;
using Kidzgo.Application.Syllabuses.ImportSyllabusPreview;
using Kidzgo.Application.Syllabuses.ImportLessonPlanWords;
using Kidzgo.Application.Syllabuses.ImportSyllabusFromWord;
using Kidzgo.Application.Syllabuses.PublishSyllabusDocument;
using Kidzgo.Application.Syllabuses.ReorderSyllabusSections;
using Kidzgo.Application.Syllabuses.UpsertCurriculumImportConfiguration;
using Kidzgo.Application.Syllabuses.UpdateSyllabusDocumentMetadata;
using Kidzgo.Application.Syllabuses.UpdateSyllabusSection;
using Kidzgo.Application.Syllabuses.UpdateSyllabusTableCell;
using Kidzgo.Application.Syllabuses.UpdateSyllabus;
using Kidzgo.Application.Syllabuses.Shared;
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
            Status = request.Status,
            SourceType = request.SourceType,
            IsActive = request.IsActive
        }, cancellationToken);

        return result.MatchCreated(x => $"/api/syllabuses/{x.Id}");
    }

    /// <summary>
    /// List syllabuses with pagination and filters.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
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

    [HttpGet("versions")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IResult> GetVersions(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] Guid? levelId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetSyllabusVersionsQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            LevelId = levelId,
            ActiveOnly = activeOnly
        }, cancellationToken);

        return result.MatchOk();
    }

    /// <summary>
    /// View syllabus detail including units, lessons, resources, and session templates.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSyllabusByIdQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{id:guid}/document")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSyllabusDocumentQuery { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Hard delete one syllabus and all lesson plan templates that belong to it.
    /// </summary>
    [HttpDelete("{id:guid}/hard-delete")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> HardDelete(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSyllabusCommand { Id = id }, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// View imported lesson plan Word files grouped by Unit or Revision for a syllabus.
    /// </summary>
    [HttpGet("{id:guid}/unit-lesson-plans")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetUnitLessonPlans(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetSyllabusUnitLessonPlansQuery { SyllabusId = id }, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get curriculum import rules for a program level before importing syllabus archives.
    /// </summary>
    [HttpGet("import-configuration")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetImportConfiguration(
        [FromQuery] Guid programId,
        [FromQuery] Guid levelId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCurriculumImportConfigurationQuery
        {
            ProgramId = programId,
            LevelId = levelId
        }, cancellationToken);

        return result.MatchOk();
    }

    /// <summary>
    /// Create or update curriculum import rules for a program level.
    /// </summary>
    [HttpPut("import-configuration")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> UpsertImportConfiguration(
        [FromQuery] Guid programId,
        [FromQuery] Guid levelId,
        [FromBody] UpsertCurriculumImportConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpsertCurriculumImportConfigurationCommand
        {
            ProgramId = programId,
            LevelId = levelId,
            RegularUnitLessonPlanCount = request.RegularUnitLessonPlanCount,
            StarterUnitLessonPlanCount = request.StarterUnitLessonPlanCount,
            RevisionLessonPlanCount = request.RevisionLessonPlanCount,
            IsActive = request.IsActive,
            Rules = request.Rules.Select(x => new UpsertCurriculumImportModuleRuleModel
            {
                ModuleId = x.ModuleId,
                IncludeStarterUnit = x.IncludeStarterUnit,
                UnitFrom = x.UnitFrom,
                UnitTo = x.UnitTo,
                RevisionNumber = x.RevisionNumber,
                OrderIndex = x.OrderIndex
            }).ToList()
        }, cancellationToken);

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
        [FromQuery] Guid? branchId,
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
            BranchId = branchId,
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

    [HttpPost("import-preview")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(41_943_040)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> ImportPreview(
        [FromForm] Guid programId,
        [FromForm] Guid levelId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var result = await mediator.Send(new ImportSyllabusPreviewCommand
        {
            ProgramId = programId,
            LevelId = levelId,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("import-commit")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(41_943_040)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ImportCommit(
        [FromForm] Guid? branchId,
        [FromForm] Guid programId,
        [FromForm] Guid levelId,
        [FromForm] string code,
        [FromForm] string? title,
        [FromForm] string? edition,
        IFormFile file,
        [FromForm] bool asDraft = true,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var result = await mediator.Send(new ImportSyllabusCommitCommand
        {
            BranchId = branchId,
            ProgramId = programId,
            LevelId = levelId,
            Code = code,
            Title = title,
            Edition = edition,
            AsDraft = asDraft,
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
        [FromQuery] Guid? branchId,
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
            BranchId = branchId,
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
    /// Import multiple lesson plan Word files without a zip archive.
    /// </summary>
    /// <remarks>
    /// When moduleId is omitted, backend maps each file to a module and absolute session index using the active curriculum import configuration.
    /// If moduleId is provided, all files are imported into that module and session index falls back to the lesson number in each Word file.
    /// </remarks>
    [HttpPost("import-lesson-plan-words")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    [RequestSizeLimit(536_870_912)]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IResult> ImportLessonPlanWords(
        [FromQuery] Guid programId,
        [FromQuery] Guid levelId,
        [FromQuery] Guid syllabusId,
        [FromQuery] Guid? moduleId,
        [FromForm] List<IFormFile> files,
        [FromQuery] bool overwriteExisting = true,
        CancellationToken cancellationToken = default)
    {
        if (files is null || files.Count == 0 || files.All(x => x.Length == 0))
        {
            return Results.BadRequest(new { error = "No files provided" });
        }

        var result = await mediator.Send(new ImportLessonPlanWordsCommand
        {
            ProgramId = programId,
            LevelId = levelId,
            SyllabusId = syllabusId,
            ModuleId = moduleId,
            OverwriteExisting = overwriteExisting,
            Files = files
                .Where(x => x.Length > 0)
                .Select(x => new ImportLessonPlanWordFile
                {
                    FileName = x.FileName,
                    FileStream = x.OpenReadStream()
                })
                .ToList()
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/metadata")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> UpdateMetadata(
        Guid id,
        [FromBody] UpdateSyllabusMetadataRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSyllabusDocumentMetadataCommand
        {
            Id = id,
            ExpectedVersion = request.ExpectedVersion,
            Code = request.Code,
            Title = request.Title,
            Edition = request.Edition,
            MinutesPerPeriod = request.MinutesPerPeriod
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{id:guid}/sections")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> AddSection(
        Guid id,
        [FromBody] AddSyllabusSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AddSyllabusSectionCommand
        {
            Id = id,
            ExpectedVersion = request.ExpectedVersion,
            Section = MapSection(request.Section)
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/sections/{sectionId:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> UpdateSection(
        Guid id,
        Guid sectionId,
        [FromBody] UpdateSyllabusSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSyllabusSectionCommand
        {
            Id = id,
            SectionId = sectionId,
            ExpectedVersion = request.ExpectedVersion,
            Title = request.Title,
            Content = request.Content,
            Items = request.Items
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/sections/{sectionId:guid}/rows/{rowId:guid}/cells/{columnKey}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> UpdateTableCell(
        Guid id,
        Guid sectionId,
        Guid rowId,
        string columnKey,
        [FromBody] UpdateSyllabusTableCellRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateSyllabusTableCellCommand
        {
            Id = id,
            SectionId = sectionId,
            RowId = rowId,
            ColumnKey = columnKey,
            ExpectedVersion = request.ExpectedVersion,
            Value = request.Value,
            RowSpan = request.RowSpan,
            ColSpan = request.ColSpan,
            Align = request.Align,
            Bold = request.Bold
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{id:guid}/sections/{sectionId:guid}/rows")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> AddTableRow(
        Guid id,
        Guid sectionId,
        [FromBody] AddSyllabusTableRowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new AddSyllabusTableRowCommand
        {
            Id = id,
            SectionId = sectionId,
            ExpectedVersion = request.ExpectedVersion,
            OrderIndex = request.OrderIndex,
            Cells = request.Cells.Select(MapCell).ToList()
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpDelete("{id:guid}/sections/{sectionId:guid}/rows/{rowId:guid}")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> DeleteTableRow(
        Guid id,
        Guid sectionId,
        Guid rowId,
        [FromQuery] int expectedVersion,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteSyllabusTableRowCommand
        {
            Id = id,
            SectionId = sectionId,
            RowId = rowId,
            ExpectedVersion = expectedVersion
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPatch("{id:guid}/sections/reorder")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> ReorderSections(
        Guid id,
        [FromBody] ReorderSyllabusSectionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ReorderSyllabusSectionsCommand
        {
            Id = id,
            ExpectedVersion = request.ExpectedVersion,
            Orders = request.Orders.Select(x => new ReorderSyllabusSectionItem
            {
                SectionId = x.SectionId,
                OrderIndex = x.OrderIndex
            }).ToList()
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> Publish(
        Guid id,
        [FromBody] PublishSyllabusDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new PublishSyllabusDocumentCommand
        {
            Id = id,
            ExpectedVersion = request.ExpectedVersion
        }, cancellationToken);

        return result.MatchOk();
    }

    [HttpPost("{id:guid}/archive")]
    [Authorize(Roles = "ManagementStaff,Admin")]
    public async Task<IResult> Archive(
        Guid id,
        [FromBody] ArchiveSyllabusDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ArchiveSyllabusDocumentCommand
        {
            Id = id,
            ExpectedVersion = request.ExpectedVersion,
            Reason = request.Reason
        }, cancellationToken);

        return result.MatchOk();
    }

    private static SyllabusDocumentSectionDto MapSection(SyllabusSectionRequest request)
    {
        return new SyllabusDocumentSectionDto
        {
            SectionId = Guid.NewGuid(),
            Type = request.Type,
            Title = request.Title,
            OrderIndex = request.OrderIndex,
            Editable = true,
            Content = request.Content,
            Items = request.Items,
            Table = request.Table is null
                ? null
                : new SyllabusDocumentTableDto
                {
                    Columns = request.Table.Columns.Select(x => new SyllabusDocumentTableColumnDto
                    {
                        Key = x.Key,
                        Label = x.Label,
                        Width = x.Width,
                        Sticky = x.Sticky
                    }).ToList(),
                    Rows = request.Table.Rows.Select(x => new SyllabusDocumentTableRowDto
                    {
                        RowId = Guid.NewGuid(),
                        OrderIndex = x.OrderIndex,
                        Group = x.Group is null
                            ? null
                            : new SyllabusDocumentTableRowGroupDto
                            {
                                BlockLabel = x.Group.BlockLabel,
                                TopicGroupId = x.Group.TopicGroupId,
                                TopicRowSpan = x.Group.TopicRowSpan
                            },
                        Cells = x.Cells.Select(MapCell).ToList()
                    }).ToList()
                }
        };
    }

    private static SyllabusDocumentTableCellDto MapCell(SyllabusTableCellRequest request)
    {
        return new SyllabusDocumentTableCellDto
        {
            ColumnKey = request.ColumnKey,
            Value = request.Value,
            RowSpan = request.RowSpan,
            ColSpan = request.ColSpan,
            Align = request.Align,
            Bold = request.Bold
        };
    }
}
