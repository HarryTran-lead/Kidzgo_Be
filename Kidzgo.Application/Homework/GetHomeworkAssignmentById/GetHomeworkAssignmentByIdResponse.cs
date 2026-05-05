using Kidzgo.Application.Homework.Shared;

namespace Kidzgo.Application.Homework.GetHomeworkAssignmentById;

public sealed class GetHomeworkAssignmentByIdResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public Guid? SessionId { get; init; }
    public string? SessionTitle { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? DueAt { get; init; }
    public string? Book { get; init; }
    public string? Pages { get; init; }
    public string? Skills { get; init; }
    public string? Topic { get; init; }
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public string SubmissionType { get; init; } = null!;
    public decimal? MaxScore { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public bool AllowResubmit { get; init; }
    public int MaxAttempts { get; init; }
    public bool AiHintEnabled { get; init; }
    public bool AiRecommendEnabled { get; init; }
    public string? Instructions { get; init; }
    public string? ExpectedAnswer { get; init; }
    public string? Rubric { get; init; }
    public string? SpeakingMode { get; init; }
    public List<string> TargetWords { get; init; } = new();
    public string? SpeakingExpectedText { get; init; }
    public string? AttachmentUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<HomeworkStudentDto> Students { get; init; } = new();
    
    public List<HomeworkQuestionDetailDto> Questions { get; init; } = new();
    
    public bool IsListeningQuiz => HomeworkDeliveryMetadata.IsListeningQuiz(Skills, AttachmentUrl);
}

public sealed class HomeworkStudentDto
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public string StudentName { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime? SubmittedAt { get; init; }
    public DateTime? GradedAt { get; init; }
    public decimal? Score { get; init; }
    public string? TeacherFeedback { get; init; }
    public int AttemptCount { get; init; }
}

/// <summary>
/// DTO chi tiết câu hỏi cho homework (bao gồm đáp án đúng - dành cho Teacher/Staff)
/// </summary>
public sealed class HomeworkQuestionDetailDto
{
    public Guid Id { get; init; }
    public int OrderIndex { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public List<string> Options { get; init; } = new();
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
}