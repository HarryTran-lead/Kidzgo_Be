using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Faqs.GetFaqs;

public sealed class GetFaqsQueryHandler(
    IDbContext context
) : IQueryHandler<GetFaqsQuery, GetFaqsResponse>
{
    public async Task<Result<GetFaqsResponse>> Handle(GetFaqsQuery query, CancellationToken cancellationToken)
    {
        var faqQuery = context.FaqItems
            .AsNoTracking()
            .Include(f => f.Category)
            .AsQueryable();

        if (query.PublicOnly)
        {
            faqQuery = faqQuery.Where(f =>
                f.IsPublished &&
                !f.IsDeleted &&
                f.Category.IsActive &&
                !f.Category.IsDeleted);
        }
        else
        {
            if (query.IsPublished.HasValue)
            {
                faqQuery = faqQuery.Where(f => f.IsPublished == query.IsPublished.Value);
            }

            if (!query.IncludeDeleted)
            {
                faqQuery = faqQuery.Where(f => !f.IsDeleted);
            }
        }

        if (query.CategoryId.HasValue)
        {
            faqQuery = faqQuery.Where(f => f.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            faqQuery = faqQuery.Where(f =>
                f.Question.Contains(term) ||
                f.Answer.Contains(term) ||
                f.Category.Name.Contains(term));
        }

        var totalCount = await faqQuery.CountAsync(cancellationToken);

        var faqs = await faqQuery
            .OrderBy(f => f.Category.SortOrder)
            .ThenBy(f => f.SortOrder)
            .ThenBy(f => f.Question)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(f => new FaqDto
            {
                Id = f.Id,
                CategoryId = f.CategoryId,
                CategoryName = f.Category.Name,
                CategoryIcon = f.Category.Icon,
                CategorySortOrder = f.Category.SortOrder,
                Question = f.Question,
                Answer = f.Answer,
                SortOrder = f.SortOrder,
                IsPublished = f.IsPublished,
                IsDeleted = f.IsDeleted,
                PublishedAt = f.PublishedAt,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var page = new Page<FaqDto>(
            faqs,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetFaqsResponse
        {
            Faqs = page
        };
    }
}
