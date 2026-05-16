namespace Kidzgo.API.Requests;

public sealed class CreateRemedialPlanRequest
{
    public Guid StudentProfileId { get; init; }
    public Guid ModuleId { get; init; }
    public string WeakSkills { get; init; } = null!;
    public int RecommendedSessionCount { get; init; }
    public string? Notes { get; init; }
}
