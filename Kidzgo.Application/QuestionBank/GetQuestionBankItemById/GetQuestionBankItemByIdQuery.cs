using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.QuestionBank.GetQuestionBankItemById;

public sealed class GetQuestionBankItemByIdQuery : IQuery<QuestionBankItemDto>
{
    public Guid Id { get; init; }
}
