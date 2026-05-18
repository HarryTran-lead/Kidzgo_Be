using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;

public sealed class UpdatePlacementTestResultsCommand : ICommand<UpdatePlacementTestResultsResponse>
{
    public Guid PlacementTestId { get; init; }
    public decimal? ListeningScore { get; init; }
    public decimal? SpeakingScore { get; init; }
    public decimal? ReadingScore { get; init; }
    public decimal? WritingScore { get; init; }
    public decimal? ResultScore { get; init; }
    public Guid? ProgramRecommendationId { get; init; }
    public Guid? PrimaryLevelRecommendationId { get; init; }
    public Guid? SecondaryLevelRecommendationId { get; init; }
    public string? SecondaryLevelSkillFocus { get; init; }
    public IReadOnlyCollection<string>? AttachmentUrls { get; init; }
}

