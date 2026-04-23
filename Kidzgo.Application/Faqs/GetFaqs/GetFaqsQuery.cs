using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;

namespace Kidzgo.Application.Faqs.GetFaqs;

public sealed class GetFaqsQuery : IQuery<GetFaqsResponse>, IPageableQuery
{
    public Guid? CategoryId { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsPublished { get; init; }
    public bool IncludeDeleted { get; init; } = false;
    public bool PublicOnly { get; init; } = false;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
