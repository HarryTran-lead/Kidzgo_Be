using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.FaqCategories.CreateFaqCategory;

public sealed class CreateFaqCategoryCommand : ICommand<CreateFaqCategoryResponse>
{
    public string Name { get; init; } = null!;
    public string? Icon { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
}
