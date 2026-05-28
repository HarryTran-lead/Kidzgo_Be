using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.Reports;

namespace Kidzgo.Application.ReportsV3.GetStudentRecommendations;

public sealed class GetStudentRecommendationsQuery : IQuery<PagedResult<RecommendationDto>>
{
    public Guid StudentId { get; init; }
    public RecommendationStatus? Status { get; init; }
    public RecommendationPriority? Priority { get; init; }
    public DateTime? DueFrom { get; init; }
    public DateTime? DueTo { get; init; }
    public bool? Overdue { get; init; }
    public string? SortBy { get; init; }
    public string? SortDir { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
