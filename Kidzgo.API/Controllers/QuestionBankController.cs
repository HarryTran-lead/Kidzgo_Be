using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.QuestionBank;
using Kidzgo.Application.QuestionBank.CreateQuestionBankItems;
using Kidzgo.Application.QuestionBank.GetQuestionBank;
using Kidzgo.Application.QuestionBank.ImportQuestionBank;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/question-bank")]
[ApiController]
[Authorize]
public class QuestionBankController : ControllerBase
{
    private readonly ISender _mediator;

    public QuestionBankController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Create questions in question bank (manual input)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateQuestionBankItems(
        [FromBody] CreateQuestionBankItemsRequest request,
        CancellationToken cancellationToken)
    {
        var items = new List<CreateQuestionBankItemDto>();

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];

            if (!Enum.TryParse<HomeworkQuestionType>(item.QuestionType, ignoreCase: true, out var questionType))
            {
                return Results.BadRequest($"Invalid question type: {item.QuestionType}");
            }

            if (!Enum.TryParse<QuestionLevel>(item.Level, ignoreCase: true, out var level))
            {
                return Results.BadRequest($"Invalid level: {item.Level}");
            }

            items.Add(new CreateQuestionBankItemDto
            {
                QuestionText = item.QuestionText,
                QuestionType = questionType,
                Options = item.Options,
                CorrectAnswer = item.CorrectAnswer,
                Points = item.Points,
                Explanation = item.Explanation,
                Topic = item.Topic,
                Skill = item.Skill,
                GrammarTags = item.GrammarTags,
                VocabularyTags = item.VocabularyTags,
                Level = level
            });
        }

        var command = new CreateQuestionBankItemsCommand
        {
            ProgramId = request.ProgramId,
            Items = items
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// List question bank items by program
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetQuestionBankItems(
        [FromQuery] Guid? programId,
        [FromQuery] string? level,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        QuestionLevel? parsedLevel = null;
        if (!string.IsNullOrWhiteSpace(level))
        {
            if (!Enum.TryParse<QuestionLevel>(level, ignoreCase: true, out var tmpLevel))
            {
                return Results.BadRequest($"Invalid level: {level}");
            }
            parsedLevel = tmpLevel;
        }

        var query = new GetQuestionBankItemsQuery
        {
            ProgramId = programId,
            Level = parsedLevel,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Import question bank from CSV/Excel/Word/PDF file
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(20_971_520)]
    public async Task<IResult> ImportQuestionBank(
        [FromQuery] Guid programId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new { error = "No file provided" });
        }

        var command = new ImportQuestionBankFromFileCommand
        {
            ProgramId = programId,
            FileName = file.FileName,
            FileStream = file.OpenReadStream()
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Generate draft question bank items with AI Creator
    /// </summary>
    [HttpPost("ai-generate")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GenerateQuestionBankItems(
        [FromBody] GenerateQuestionBankItemsRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HomeworkQuestionType>(request.QuestionType, ignoreCase: true, out var questionType))
        {
            return Results.BadRequest($"Invalid question type: {request.QuestionType}");
        }

        if (!Enum.TryParse<QuestionLevel>(request.Level, ignoreCase: true, out var level))
        {
            return Results.BadRequest($"Invalid level: {request.Level}");
        }

        var taskStyle = string.IsNullOrWhiteSpace(request.TaskStyle)
            ? "standard"
            : request.TaskStyle.Trim().ToLowerInvariant();

        if (taskStyle is not ("standard" or "translation"))
        {
            return Results.BadRequest($"Invalid task style: {request.TaskStyle}");
        }

        var query = new GenerateAiQuestionBankItemsQuery
        {
            ProgramId = request.ProgramId,
            Topic = request.Topic,
            QuestionType = questionType,
            QuestionCount = request.QuestionCount,
            Level = level,
            Skill = request.Skill,
            TaskStyle = taskStyle,
            GrammarTags = request.GrammarTags,
            VocabularyTags = request.VocabularyTags,
            Instructions = request.Instructions,
            Language = request.Language,
            PointsPerQuestion = request.PointsPerQuestion
        };

        var result = await _mediator.Send(query, cancellationToken);
        return MatchAiGenerationResult(result);
    }

    /// <summary>
    /// Generate draft question bank items with AI Creator from an optional source file.
    /// If no file is provided, the endpoint falls back to the same fields as the JSON AI Creator flow.
    /// </summary>
    [HttpPost("ai-generate/from-file")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    [RequestSizeLimit(20_971_520)]
    [Consumes("multipart/form-data")]
    public async Task<IResult> GenerateQuestionBankItemsFromFile(
        [FromForm] GenerateQuestionBankItemsFromFileRequest request,
        CancellationToken cancellationToken)
    {
        if (request.File is { Length: 0 })
        {
            return Results.BadRequest(new { error = "File is empty" });
        }

        if (!Enum.TryParse<HomeworkQuestionType>(request.QuestionType, ignoreCase: true, out var questionType))
        {
            return Results.BadRequest($"Invalid question type: {request.QuestionType}");
        }

        if (!Enum.TryParse<QuestionLevel>(request.Level, ignoreCase: true, out var level))
        {
            return Results.BadRequest($"Invalid level: {request.Level}");
        }

        var taskStyle = string.IsNullOrWhiteSpace(request.TaskStyle)
            ? "standard"
            : request.TaskStyle.Trim().ToLowerInvariant();

        if (taskStyle is not ("standard" or "translation"))
        {
            return Results.BadRequest($"Invalid task style: {request.TaskStyle}");
        }

        using var sourceFileStream = request.File?.OpenReadStream();
        var query = new GenerateAiQuestionBankItemsQuery
        {
            ProgramId = request.ProgramId,
            Topic = request.Topic ?? string.Empty,
            QuestionType = questionType,
            QuestionCount = request.QuestionCount,
            Level = level,
            Skill = request.Skill,
            TaskStyle = taskStyle,
            GrammarTags = NormalizeTags(request.GrammarTags),
            VocabularyTags = NormalizeTags(request.VocabularyTags),
            Instructions = request.Instructions,
            Language = request.Language,
            PointsPerQuestion = request.PointsPerQuestion,
            SourceFileName = request.File?.FileName,
            SourceFileStream = sourceFileStream
        };

        var result = await _mediator.Send(query, cancellationToken);
        return MatchAiGenerationResult(result);
    }

    private static List<string> NormalizeTags(IEnumerable<string>? tags)
    {
        return tags?
            .SelectMany(tag => tag.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? new List<string>();
    }

    private static IResult MatchAiGenerationResult(Result<GenerateAiQuestionBankItemsResponse> result)
    {
        if (result.IsSuccess)
        {
            return result.MatchOk();
        }

        if (result.Error.Code is "Homework.AiCreatorBusy" or "Homework.AiCreatorUnavailable")
        {
            return Results.Problem(
                title: result.Error.Code == "Homework.AiCreatorBusy"
                    ? "AI đang bận"
                    : "AI tạm gián đoạn",
                detail: result.Error.Description,
                statusCode: StatusCodes.Status503ServiceUnavailable,
                extensions: new Dictionary<string, object?>
                {
                    ["code"] = result.Error.Code
                });
        }

        return result.MatchOk();
    }
}
