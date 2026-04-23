using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Faqs.UpdateFaq;

public sealed class UpdateFaqCommand : ICommand<UpdateFaqResponse>
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string Question { get; init; } = null!;
    public string Answer { get; init; } = null!;
    public int SortOrder { get; init; }
    public bool IsPublished { get; init; }
}
