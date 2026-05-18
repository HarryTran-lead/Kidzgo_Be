namespace Kidzgo.Application.PlacementTests.GetPlacementTests;

public sealed class GetPlacementTestsResponse
{
    public List<PlacementTestDto> PlacementTests { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
}

public sealed class PlacementTestDto
{
    public Guid Id { get; init; }
    public Guid? LeadId { get; init; }
    public Guid? LeadChildId { get; init; }
    public string? LeadContactName { get; init; }
    public string? ChildName { get; init; }
    public Guid? StudentProfileId { get; init; }
    public string? StudentName { get; init; }
    public Guid? ClassId { get; init; }
    public string? ClassName { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public int DurationMinutes { get; init; }
    public string Status { get; init; } = null!;
    public Guid? RoomId { get; init; }
    public string? RoomName { get; init; }
    public string? Room { get; init; }
    public Guid? InvigilatorUserId { get; init; }
    public string? InvigilatorName { get; init; }
    public decimal? ResultScore { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public Guid? ProgramRecommendationId { get; init; }
    public string? ProgramRecommendationName { get; init; }
    public Guid? PrimaryLevelRecommendationId { get; init; }
    public string? PrimaryLevelRecommendationName { get; init; }
    public Guid? SecondaryLevelRecommendationId { get; init; }
    public string? SecondaryLevelRecommendationName { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    public string? Notes { get; init; }
    public string? AttachmentUrl { get; init; }
    public IReadOnlyList<string> AttachmentUrls { get; init; } = Array.Empty<string>();
    public bool IsAccountProfileCreated { get; init; }
    public bool IsConvertedToEnrolled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

