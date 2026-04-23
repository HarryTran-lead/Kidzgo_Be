using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Faqs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.FaqCategories.UpdateFaqCategory;

public sealed class UpdateFaqCategoryCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateFaqCategoryCommand, UpdateFaqCategoryResponse>
{
    public async Task<Result<UpdateFaqCategoryResponse>> Handle(UpdateFaqCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await context.FaqCategories
            .FirstOrDefaultAsync(c => c.Id == command.Id, cancellationToken);

        if (category is null || category.IsDeleted)
        {
            return Result.Failure<UpdateFaqCategoryResponse>(FaqErrors.CategoryNotFound(command.Id));
        }

        var normalizedName = command.Name.Trim();
        var normalizedNameLower = normalizedName.ToLower();

        var categoryExists = await context.FaqCategories
            .AnyAsync(
                c => c.Id != command.Id &&
                     !c.IsDeleted &&
                     c.Name.ToLower() == normalizedNameLower,
                cancellationToken);

        if (categoryExists)
        {
            return Result.Failure<UpdateFaqCategoryResponse>(FaqErrors.CategoryNameAlreadyExists(normalizedName));
        }

        category.Name = normalizedName;
        category.Icon = string.IsNullOrWhiteSpace(command.Icon) ? null : command.Icon.Trim();
        category.SortOrder = command.SortOrder;
        category.IsActive = command.IsActive;
        category.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateFaqCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            IsDeleted = category.IsDeleted,
            UpdatedAt = category.UpdatedAt
        };
    }
}
