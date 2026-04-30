using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LearningHistory.GetLearningHistory;

public sealed class GetLearningHistoryQuery : IQuery<GetLearningHistoryResponse>
{
    public Guid? StudentProfileId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public int SessionPageNumber { get; init; } = 1;
    public int SessionPageSize { get; init; } = 20;
    public int MissionPageNumber { get; init; } = 1;
    public int MissionPageSize { get; init; } = 20;
}
