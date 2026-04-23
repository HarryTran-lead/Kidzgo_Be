namespace Kidzgo.API.Requests;

public sealed class UpdateFaqCategoryRequest
{
    public string Name { get; set; } = null!;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
