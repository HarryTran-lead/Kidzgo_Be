using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Homework.AiGradeHomework;
using Kidzgo.Application.Homework.CreateHomeworkAssignment;
using Kidzgo.Application.Homework.CreateMultipleChoiceHomework;
using Kidzgo.Application.Homework.CreateMultipleChoiceHomeworkFromBank;
using Kidzgo.Application.Homework.DeleteHomeworkAssignment;
using Kidzgo.Application.Homework.GetHomeworkAssignmentById;
using Kidzgo.Application.Homework.GetHomeworkAssignments;
using Kidzgo.Application.Homework.GetMyCreatedHomeworkAssignments;
using Kidzgo.Application.Homework.GetHomeworkSubmissionDetail;
using Kidzgo.Application.Homework.GetHomeworkSubmissions;
using Kidzgo.Application.Homework.GetStudentHomeworkHistory;
using Kidzgo.Application.Homework.GradeHomework;
using Kidzgo.Application.Homework.MarkHomeworkLateOrMissing;
using Kidzgo.Application.Homework.SetHomeworkRewardStars;
using Kidzgo.Application.Homework.UpdateHomeworkAssignment;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/homework")]
[ApiController]
[Authorize]
public class HomeworkController : ControllerBase
{
    private readonly ISender _mediator;

    public HomeworkController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// UC-117: Tạo Homework Assignment
    /// </summary>
[HttpPost]
[Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateHomeworkAssignment(
        [FromBody] CreateHomeworkAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseSubmissionType(request.SubmissionType, out var submissionType))
        {
            return Results.BadRequest(HomeworkErrors.InvalidSubmissionType);
        }

        var command = new CreateHomeworkAssignmentCommand
        {
            ClassId = request.ClassId,
            SessionId = request.SessionId,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            DueAt = request.DueAt,
            Book = request.Book,
            Pages = request.Pages,
            Skills = request.Skills,
            Topic = request.Topic,
            GrammarTags = request.GrammarTags,
            VocabularyTags = request.VocabularyTags,
            SubmissionType = submissionType,
            MaxScore = request.MaxScore,
            RewardStars = request.RewardStars,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = ResolveMaxAttempts(request.MaxAttempts, request.AllowResubmit),
            AiHintEnabled = request.AiHintEnabled,
            AiRecommendEnabled = request.AiRecommendEnabled,
            Instructions = request.Instructions,
            ExpectedAnswer = request.ExpectedAnswer,
            Rubric = request.Rubric,
            SpeakingMode = request.SpeakingMode,
            TargetWords = request.TargetWords,
            SpeakingExpectedText = request.SpeakingExpectedText,
            AttachmentUrl = request.Attachment
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/homework/{r.Id}");
    }

    /// <summary>
    /// Tạo Multiple Choice Homework Assignment
    /// </summary>
[HttpPost("multiple-choice")]
[Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateMultipleChoiceHomework(
        [FromBody] CreateMultipleChoiceHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var questions = new List<CreateHomeworkQuestionDto>();

        for (int i = 0; i < request.Questions.Count; i++)
        {
            var q = request.Questions[i];
            if (!Enum.TryParse<HomeworkQuestionType>(q.QuestionType, ignoreCase: true, out var questionType))
            {
                return Results.BadRequest($"Invalid question type: {q.QuestionType}");
            }

            questions.Add(new CreateHomeworkQuestionDto
            {
                QuestionText = q.QuestionText,
                QuestionType = questionType,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points,
                Explanation = q.Explanation
            });
        }

        var command = new CreateMultipleChoiceHomeworkCommand
        {
            ClassId = request.ClassId,
            SessionId = request.SessionId,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            DueAt = request.DueAt,
            Skills = request.Skills,
            Topic = request.Topic,
            GrammarTags = request.GrammarTags,
            VocabularyTags = request.VocabularyTags,
            RewardStars = request.RewardStars,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = ResolveMaxAttempts(request.MaxAttempts, request.AllowResubmit),
            AiHintEnabled = request.AiHintEnabled,
            AiRecommendEnabled = request.AiRecommendEnabled,
            Instructions = request.Instructions,
            AttachmentUrl = request.Attachment,
            Questions = questions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/homework/{r.Id}");
    }

    /// <summary>
    /// Táº¡o Multiple Choice Homework Assignment tá»« Question Bank
    /// </summary>
    [HttpPost("multiple-choice/from-bank")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> CreateMultipleChoiceHomeworkFromBank(
        [FromBody] CreateMultipleChoiceHomeworkFromBankRequest request,
        CancellationToken cancellationToken)
    {
        var distribution = new List<QuestionLevelCountDto>();

        for (int i = 0; i < request.Distribution.Count; i++)
        {
            var d = request.Distribution[i];
            if (!Enum.TryParse<QuestionLevel>(d.Level, ignoreCase: true, out var level))
            {
                return Results.BadRequest($"Invalid level: {d.Level}");
            }

            distribution.Add(new QuestionLevelCountDto
            {
                Level = level,
                Count = d.Count
            });
        }

        var command = new CreateMultipleChoiceHomeworkFromBankCommand
        {
            ClassId = request.ClassId,
            ProgramId = request.ProgramId,
            SessionId = request.SessionId,
            Title = request.Title,
            Description = request.Description,
            StartDate = request.StartDate,
            DueAt = request.DueAt,
            Skills = request.Skills,
            Topic = request.Topic,
            GrammarTags = request.GrammarTags,
            VocabularyTags = request.VocabularyTags,
            RewardStars = request.RewardStars,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = ResolveMaxAttempts(request.MaxAttempts, request.AllowResubmit),
            AiHintEnabled = request.AiHintEnabled,
            AiRecommendEnabled = request.AiRecommendEnabled,
            Instructions = request.Instructions,
            AttachmentUrl = request.Attachment,
            Distribution = distribution
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/homework/{r.Id}");
    }
    
    /// <summary>
    /// UC-118: Xem danh sách Homework Assignments
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetHomeworkAssignments(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sessionId,
        [FromQuery] string? skill,
        [FromQuery] string? submissionType,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        SubmissionType? parsedSubmissionType = null;
        if (TryParseSubmissionType(submissionType, out var tmpType))
        {
            parsedSubmissionType = tmpType;
        }

        var query = new GetHomeworkAssignmentsQuery
        {
            ClassId = classId,
            SessionId = sessionId,
            Skill = skill,
            SubmissionType = parsedSubmissionType,
            BranchId = branchId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Teacher xem danh sach homework do chinh minh tao
    /// </summary>
    [HttpGet("my-created")]
    [Authorize(Roles = "Teacher")]
    public async Task<IResult> GetMyCreatedHomeworkAssignments(
        [FromQuery] Guid? classId,
        [FromQuery] Guid? sessionId,
        [FromQuery] string? skill,
        [FromQuery] string? submissionType,
        [FromQuery] Guid? branchId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        SubmissionType? parsedSubmissionType = null;
        if (TryParseSubmissionType(submissionType, out var tmpType))
        {
            parsedSubmissionType = tmpType;
        }

        var query = new GetMyCreatedHomeworkAssignmentsQuery
        {
            ClassId = classId,
            SessionId = sessionId,
            Skill = skill,
            SubmissionType = parsedSubmissionType,
            BranchId = branchId,
            FromDate = fromDate,
            ToDate = toDate,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-119: Xem chi tiáº¿t Homework Assignment
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> GetHomeworkAssignmentById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetHomeworkAssignmentByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-120: Cập nhật Homework Assignment
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> UpdateHomeworkAssignment(
        Guid id,
        [FromBody] UpdateHomeworkAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        SubmissionType? parsedSubmissionType = null;
        if (TryParseSubmissionType(request.SubmissionType, out var tmpType))
        {
            parsedSubmissionType = tmpType;
        }

        List<UpdateHomeworkQuestionDto>? questions = null;
        if (request.Questions != null)
        {
            questions = new List<UpdateHomeworkQuestionDto>();
            for (int i = 0; i < request.Questions.Count; i++)
            {
                var q = request.Questions[i];
                if (!Enum.TryParse<HomeworkQuestionType>(q.QuestionType, ignoreCase: true, out var questionType))
                {
                    return Results.BadRequest($"Invalid question type: {q.QuestionType}");
                }

                questions.Add(new UpdateHomeworkQuestionDto
                {
                    Id = q.Id,
                    OrderIndex = i,
                    QuestionText = q.QuestionText,
                    QuestionType = questionType,
                    Options = q.Options,
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points,
                    Explanation = q.Explanation
                });
            }
        }

        var command = new UpdateHomeworkAssignmentCommand
        {
            Id = id,
            Title = request.Title,
            Description = request.Description,
            DueAt = request.DueAt,
            Book = request.Book,
            Pages = request.Pages,
            Skills = request.Skills,
            Topic = request.Topic,
            GrammarTags = request.GrammarTags,
            VocabularyTags = request.VocabularyTags,
            SubmissionType = parsedSubmissionType,
            MaxScore = request.MaxScore,
            RewardStars = request.RewardStars,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = ResolveOptionalMaxAttempts(request.MaxAttempts, request.AllowResubmit),
            AiHintEnabled = request.AiHintEnabled,
            AiRecommendEnabled = request.AiRecommendEnabled,
            Instructions = request.Instructions,
            ExpectedAnswer = request.ExpectedAnswer,
            Rubric = request.Rubric,
            SpeakingMode = request.SpeakingMode,
            TargetWords = request.TargetWords,
            SpeakingExpectedText = request.SpeakingExpectedText,
            AttachmentUrl = request.Attachment,
            Questions = questions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-121: Xóa Homework Assignment
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> DeleteHomeworkAssignment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteHomeworkAssignmentCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-123: Gắn Homework với Mission
    /// </summary>
    /// <summary>
    /// UC-124: Thiết lập reward stars cho Homework
    /// </summary>
    [HttpPut("{id:guid}/reward-stars")]
    [Authorize(Roles = "Teacher,ManagementStaff,Admin")]
    public async Task<IResult> SetHomeworkRewardStars(
        Guid id,
        [FromBody] SetHomeworkRewardStarsRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetHomeworkRewardStarsCommand
        {
            HomeworkId = id,
            RewardStars = request.RewardStars
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-128 & UC-129: Teacher chấm Homework và nhập điểm/feedback
    /// </summary>
    [HttpPost("submissions/{homeworkStudentId:guid}/grade")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> GradeHomework(
        Guid homeworkStudentId,
        [FromBody] GradeHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GradeHomeworkCommand
        {
            HomeworkStudentId = homeworkStudentId,
            Score = request.Score,
            TeacherFeedback = request.TeacherFeedback
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-132: Đánh dấu Homework quá hạn (LATE/MISSING)
    /// </summary>
    /// <summary>
    /// UC-130: Teacher trigger AI quick grade for a homework submission
    /// </summary>
    [HttpPost("submissions/{homeworkStudentId:guid}/quick-grade")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> AiGradeHomework(
        Guid homeworkStudentId,
        [FromBody] AiGradeHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AiGradeHomeworkCommand
        {
            HomeworkStudentId = homeworkStudentId,
            Language = string.IsNullOrWhiteSpace(request.Language) ? "vi" : request.Language,
            Instructions = request.Instructions,
            Rubric = request.Rubric,
            ExpectedAnswerText = request.ExpectedAnswerText
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-132: Mark homework overdue status (LATE/MISSING)
    /// </summary>
    [HttpPut("submissions/{homeworkStudentId:guid}/mark-status")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> MarkHomeworkLateOrMissing(
        Guid homeworkStudentId,
        [FromBody] MarkHomeworkLateOrMissingRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<HomeworkStatus>(request.Status, ignoreCase: true, out var status) ||
            (status != HomeworkStatus.Late && status != HomeworkStatus.Missing))
        {
            return Results.BadRequest(HomeworkErrors.InvalidStatusForMarking);
        }

        var command = new MarkHomeworkLateOrMissingCommand
        {
            HomeworkStudentId = homeworkStudentId,
            Status = status
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-133: Xem lịch sử Homework của học sinh
    /// </summary>
    [HttpGet("students/{studentProfileId:guid}/history")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> GetStudentHomeworkHistory(
        Guid studentProfileId,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentHomeworkHistoryQuery
        {
            StudentProfileId = studentProfileId,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-134: Teacher xem danh sách bài nộp của học sinh (filter theo lớp mình dạy)
    /// </summary>
    [HttpGet("submissions")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> GetHomeworkSubmissions(
        [FromQuery] Guid? classId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        HomeworkStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<HomeworkStatus>(status, ignoreCase: true, out var tmpStatus))
        {
            parsedStatus = tmpStatus;
        }

        var query = new GetHomeworkSubmissionsQuery
        {
            ClassId = classId,
            Status = parsedStatus,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// UC-135: Teacher xem chi tiết bài nộp của học sinh
    /// </summary>
    [HttpGet("submissions/{homeworkStudentId:guid}")]
    [Authorize(Roles = "Teacher,TeachingAssistant,ManagementStaff,Admin")]
    public async Task<IResult> GetHomeworkSubmissionDetail(
        Guid homeworkStudentId,
        CancellationToken cancellationToken)
    {
        var query = new GetHomeworkSubmissionDetailQuery { HomeworkStudentId = homeworkStudentId };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    private static bool TryParseSubmissionType(string? input, out SubmissionType submissionType)
    {
        submissionType = default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        if (string.Equals(input, "MULTIPLE_CHOICE", StringComparison.OrdinalIgnoreCase))
        {
            submissionType = SubmissionType.Quiz;
            return true;
        }

        return Enum.TryParse(input, ignoreCase: true, out submissionType);
    }

    private static int ResolveMaxAttempts(int? maxAttempts, bool? legacyAllowResubmit)
    {
        if (maxAttempts.HasValue)
        {
            return maxAttempts.Value;
        }

        return legacyAllowResubmit == true ? 2 : 1;
    }

    private static int? ResolveOptionalMaxAttempts(int? maxAttempts, bool? legacyAllowResubmit)
    {
        if (maxAttempts.HasValue)
        {
            return maxAttempts.Value;
        }

        return legacyAllowResubmit.HasValue
            ? legacyAllowResubmit.Value ? 2 : 1
            : null;
    }
}

