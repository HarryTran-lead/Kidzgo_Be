namespace Kidzgo.API.Requests;

public sealed class UpdateFaqRequest
{
    public Guid CategoryId { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
