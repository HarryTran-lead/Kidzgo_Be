namespace Kidzgo.Application.Homework.CreateHomeworkAssignment;

public sealed class CreateHomeworkAssignmentResponse
{
    public Guid Id { get; init; }
    public Guid ClassId { get; init; }
    public Guid? SessionId { get; init; }
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
    public int AssignedStudentsCount { get; init; }
}

