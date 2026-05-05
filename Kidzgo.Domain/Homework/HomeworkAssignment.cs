using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Homework;

public class HomeworkAssignment : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid? SessionId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueAt { get; set; }
    public string? Book { get; set; }
    public string? Pages { get; set; }
    public string? Skills { get; set; }
    public string? Topic { get; set; }
    public string? GrammarTags { get; set; }
    public string? VocabularyTags { get; set; }
    public SubmissionType SubmissionType { get; set; }
    public decimal? MaxScore { get; set; }
    public int? RewardStars { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public bool AiHintEnabled { get; set; }
    public bool AiRecommendEnabled { get; set; }
    public Guid? MissionId { get; set; }
    public string? Instructions { get; set; }
    public string? ExpectedAnswer { get; set; }
    public string? Rubric { get; set; }
    public string? SpeakingMode { get; set; }
    public string? TargetWords { get; set; }
    public string? SpeakingExpectedText { get; set; }
    public string? AttachmentUrl { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Class Class { get; set; } = null!;
    public Session? Session { get; set; }
    public Mission? Mission { get; set; }
    public User? CreatedByUser { get; set; }
    public ICollection<HomeworkStudent> HomeworkStudents { get; set; } = new List<HomeworkStudent>();
}
