namespace Kidzgo.Application.FaqCategories.UpdateFaqCategory;

public sealed class UpdateFaqCategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime UpdatedAt { get; init; }
}
