namespace Kidzgo.Application.Faqs.CreateFaq;

public sealed class CreateFaqResponse
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = null!;
    public string Question { get; init; } = null!;
    public string Answer { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsPublished { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime? PublishedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
