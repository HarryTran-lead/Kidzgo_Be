namespace Kidzgo.Application.FaqCategories.CreateFaqCategory;

public sealed class CreateFaqCategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
