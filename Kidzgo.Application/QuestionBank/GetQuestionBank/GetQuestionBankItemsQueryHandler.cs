using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.QuestionBank;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.GetQuestionBank;

public sealed class GetQuestionBankItemsQueryHandler(
    IDbContext context
) : IQueryHandler<GetQuestionBankItemsQuery, GetQuestionBankItemsResponse>
{
    public async Task<Result<GetQuestionBankItemsResponse>> Handle(
        GetQuestionBankItemsQuery query,
        CancellationToken cancellationToken)
    {
        var itemsQuery = context.QuestionBankItems
            .AsQueryable()
            .Where(q => !q.IsDeleted);

        if (query.ProgramId.HasValue && query.ProgramId.Value != Guid.Empty)
        {
            itemsQuery = itemsQuery.Where(q => q.ProgramId == query.ProgramId.Value);
        }

        if (query.Level.HasValue)
        {
            itemsQuery = itemsQuery.Where(q => q.Level == query.Level.Value);
        }

        var totalCount = await itemsQuery.CountAsync(cancellationToken);

        var entities = await itemsQuery
            .OrderByDescending(q => q.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var items = entities.Select(q => q.ToDto()).ToList();

        var page = new Page<QuestionBankItemDto>(
            items,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetQuestionBankItemsResponse
        {
            Items = page
        };
    }
}
