namespace Kidzgo.Application.ReportsV3.Shared;

public sealed class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
    public int Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public bool HasNext { get; init; }
}
