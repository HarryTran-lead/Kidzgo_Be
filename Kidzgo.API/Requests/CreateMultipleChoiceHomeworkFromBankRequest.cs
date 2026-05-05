namespace Kidzgo.API.Requests;

public sealed class CreateMultipleChoiceHomeworkFromBankRequest
{
    public Guid ClassId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid? SessionId { get; init; }
    public string Title { get; init; } = null!;
    public string? Description { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? DueAt { get; init; }
    public string? Skills { get; init; }
    public string? Topic { get; init; }
    public List<string>? GrammarTags { get; init; }
    public List<string>? VocabularyTags { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public int? MaxAttempts { get; init; }
    public bool? AllowResubmit { get; init; }
    public bool? AiHintEnabled { get; init; }
    public bool? AiRecommendEnabled { get; init; }
    public string? Instructions { get; init; }
    public string? Attachment { get; init; }
    public List<QuestionLevelDistributionRequest> Distribution { get; init; } = new();
}

public sealed class QuestionLevelDistributionRequest
{
    public string Level { get; init; } = null!;
    public int Count { get; init; }
}
