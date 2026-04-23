using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.DeleteQuestionBankItem;

public sealed class DeleteQuestionBankItemCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteQuestionBankItemCommand>
{
    public async Task<Result> Handle(
        DeleteQuestionBankItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.QuestionBankItems
            .FirstOrDefaultAsync(q => q.Id == command.Id && !q.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Result.Failure(HomeworkErrors.QuestionBankItemNotFound(command.Id));
        }

        item.IsDeleted = true;
        item.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
