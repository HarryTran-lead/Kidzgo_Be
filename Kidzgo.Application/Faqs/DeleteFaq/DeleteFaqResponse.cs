namespace Kidzgo.Application.Faqs.DeleteFaq;

public sealed class DeleteFaqResponse
{
    public Guid Id { get; init; }
    public string Question { get; init; } = null!;
    public bool IsDeleted { get; init; }
    public DateTime UpdatedAt { get; init; }
}
