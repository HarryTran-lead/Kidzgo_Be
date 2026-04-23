using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.FaqCategories.GetFaqCategories;

public sealed class GetFaqCategoriesQuery : IQuery<GetFaqCategoriesResponse>
{
    public bool PublicOnly { get; init; } = false;
    public bool IncludeDeleted { get; init; } = false;
    public bool IncludeInactive { get; init; } = false;
}
