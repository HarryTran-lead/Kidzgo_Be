using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Faqs.CreateFaq;

public sealed class CreateFaqCommand : ICommand<CreateFaqResponse>
{
    public Guid CategoryId { get; init; }
    public string Question { get; init; } = null!;
    public string Answer { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsPublished { get; init; }
}
