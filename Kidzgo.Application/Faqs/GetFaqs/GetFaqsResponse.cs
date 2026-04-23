using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Faqs.GetFaqs;

public sealed class GetFaqsResponse
{
    public Page<FaqDto> Faqs { get; init; } = null!;
}

public sealed class FaqDto
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = null!;
    public string? CategoryIcon { get; init; }
    public int CategorySortOrder { get; init; }
    public string Question { get; init; } = null!;
    public string Answer { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsPublished { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
