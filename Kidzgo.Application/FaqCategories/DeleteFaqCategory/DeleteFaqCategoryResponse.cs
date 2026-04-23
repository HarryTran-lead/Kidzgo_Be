namespace Kidzgo.Application.FaqCategories.DeleteFaqCategory;

public sealed class DeleteFaqCategoryResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public DateTime UpdatedAt { get; init; }
}
