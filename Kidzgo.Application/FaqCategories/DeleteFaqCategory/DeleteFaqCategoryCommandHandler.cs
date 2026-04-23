using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Faqs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.FaqCategories.DeleteFaqCategory;

public sealed class DeleteFaqCategoryCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteFaqCategoryCommand, DeleteFaqCategoryResponse>
{
    public async Task<Result<DeleteFaqCategoryResponse>> Handle(DeleteFaqCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await context.FaqCategories
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure<DeleteFaqCategoryResponse>(FaqErrors.CategoryNotFound(command.Id));
        }

        if (category.IsDeleted)
        {
            return Result.Failure<DeleteFaqCategoryResponse>(FaqErrors.CategoryAlreadyDeleted);
        }

        var hasFaqItems = await context.FaqItems
            .AnyAsync(f => f.CategoryId == category.Id && !f.IsDeleted, cancellationToken);

        if (hasFaqItems)
        {
            return Result.Failure<DeleteFaqCategoryResponse>(FaqErrors.CategoryHasFaqItems);
        }

        category.IsDeleted = true;
        category.IsActive = false;
        category.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteFaqCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            IsDeleted = category.IsDeleted,
            UpdatedAt = category.UpdatedAt
        };
    }
}
