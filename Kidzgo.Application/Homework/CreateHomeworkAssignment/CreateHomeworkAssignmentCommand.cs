using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.CreateHomeworkAssignment;

public sealed class CreateHomeworkAssignmentCommand : ICommand<CreateHomeworkAssignmentResponse>
{
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
    public List<string>? GrammarTags { get; init; }
    public List<string>? VocabularyTags { get; init; }
    public SubmissionType SubmissionType { get; init; }
    public decimal? MaxScore { get; init; }
    public int? RewardStars { get; init; }
    public int? TimeLimitMinutes { get; init; }
    public int MaxAttempts { get; init; } = 1;
    public bool? AiHintEnabled { get; init; }
    public bool? AiRecommendEnabled { get; init; }
    public Guid? MissionId { get; init; }
    public string? Instructions { get; init; }
    public string? ExpectedAnswer { get; init; }
    public string? Rubric { get; init; }
    public string? SpeakingMode { get; init; }
    public List<string>? TargetWords { get; init; }
    public string? SpeakingExpectedText { get; init; }
    public string? AttachmentUrl { get; init; }
}

