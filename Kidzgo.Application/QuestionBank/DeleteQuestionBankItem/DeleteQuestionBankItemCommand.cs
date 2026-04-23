using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.QuestionBank.DeleteQuestionBankItem;

public sealed class DeleteQuestionBankItemCommand : ICommand
{
    public Guid Id { get; init; }
}
