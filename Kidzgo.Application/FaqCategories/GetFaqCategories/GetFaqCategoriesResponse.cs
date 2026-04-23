namespace Kidzgo.Application.FaqCategories.GetFaqCategories;

public sealed class GetFaqCategoriesResponse
{
    public IReadOnlyList<FaqCategoryDto> Categories { get; init; } = Array.Empty<FaqCategoryDto>();
}

public sealed class FaqCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
    public int TotalFaqCount { get; init; }
    public int PublishedFaqCount { get; init; }
}
