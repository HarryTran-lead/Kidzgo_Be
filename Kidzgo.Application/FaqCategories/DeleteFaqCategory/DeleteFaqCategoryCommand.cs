using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.FaqCategories.DeleteFaqCategory;

public sealed class DeleteFaqCategoryCommand : ICommand<DeleteFaqCategoryResponse>
{
    public Guid Id { get; init; }
}
