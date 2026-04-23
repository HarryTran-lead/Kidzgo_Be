namespace Kidzgo.API.Requests;

public sealed class CreateFaqRequest
{
    public Guid CategoryId { get; set; }
    public string Question { get; set; } = null!;
    public string Answer { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
}
