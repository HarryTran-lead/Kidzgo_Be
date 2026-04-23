using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.FaqCategories.GetFaqCategories;

public sealed class GetFaqCategoriesQueryHandler(
    IDbContext context
) : IQueryHandler<GetFaqCategoriesQuery, GetFaqCategoriesResponse>
{
    public async Task<Result<GetFaqCategoriesResponse>> Handle(GetFaqCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categoriesQuery = context.FaqCategories
            .AsNoTracking()
            .AsQueryable();

        if (query.PublicOnly)
        {
            categoriesQuery = categoriesQuery.Where(c =>
                c.IsActive &&
                !c.IsDeleted &&
                c.FaqItems.Any(f => f.IsPublished && !f.IsDeleted));
        }
        else
        {
            if (!query.IncludeDeleted)
            {
                categoriesQuery = categoriesQuery.Where(c => !c.IsDeleted);
            }

            if (!query.IncludeInactive)
            {
                categoriesQuery = categoriesQuery.Where(c => c.IsActive);
            }
        }

        var categories = await categoriesQuery
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Select(c => new FaqCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                IsDeleted = c.IsDeleted,
                TotalFaqCount = c.FaqItems.Count(f => !f.IsDeleted),
                PublishedFaqCount = c.FaqItems.Count(f => f.IsPublished && !f.IsDeleted)
            })
            .ToListAsync(cancellationToken);

        return new GetFaqCategoriesResponse
        {
            Categories = categories
        };
    }
}
