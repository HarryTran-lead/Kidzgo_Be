using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.FaqCategories.UpdateFaqCategory;

public sealed class UpdateFaqCategoryCommand : ICommand<UpdateFaqCategoryResponse>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}
