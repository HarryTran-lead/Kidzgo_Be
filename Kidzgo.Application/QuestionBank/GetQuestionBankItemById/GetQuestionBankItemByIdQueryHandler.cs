using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.GetQuestionBankItemById;

public sealed class GetQuestionBankItemByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetQuestionBankItemByIdQuery, QuestionBankItemDto>
{
    public async Task<Result<QuestionBankItemDto>> Handle(
        GetQuestionBankItemByIdQuery query,
        CancellationToken cancellationToken)
    {
        var item = await context.QuestionBankItems
            .FirstOrDefaultAsync(q => q.Id == query.Id && !q.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Result.Failure<QuestionBankItemDto>(
                HomeworkErrors.QuestionBankItemNotFound(query.Id));
        }

        return item.ToDto();
    }
}
