using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;

namespace Kidzgo.Application.Homework.CreateMultipleChoiceHomework;

public sealed class CreateMultipleChoiceHomeworkCommand : ICommand<CreateMultipleChoiceHomeworkResponse>
{
    public Guid ClassId { get; init; }
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
    public int MaxAttempts { get; init; } = 1;
    public bool? AiHintEnabled { get; init; }
    public bool? AiRecommendEnabled { get; init; }
    public Guid? MissionId { get; init; }
    public string? Instructions { get; init; }
    public string? AttachmentUrl { get; init; }
    public List<CreateHomeworkQuestionDto> Questions { get; init; } = new();
}

public class CreateHomeworkQuestionDto
{
    public string QuestionText { get; init; } = null!;
    public HomeworkQuestionType QuestionType { get; init; }
    public List<string> Options { get; init; } = new();
    public string CorrectAnswer { get; init; } = null!; // Option index (0, 1, 2, 3) or text answer
    public int Points { get; init; } = 1;
    public string? Explanation { get; init; }
}

