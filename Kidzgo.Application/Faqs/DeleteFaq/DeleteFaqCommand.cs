using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Faqs.DeleteFaq;

public sealed class DeleteFaqCommand : ICommand<DeleteFaqResponse>
{
    public Guid Id { get; init; }
}
