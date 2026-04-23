using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Faqs;
using Kidzgo.Domain.Faqs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.FaqCategories.CreateFaqCategory;

public sealed class CreateFaqCategoryCommandHandler(
    IDbContext context
) : ICommandHandler<CreateFaqCategoryCommand, CreateFaqCategoryResponse>
{
    public async Task<Result<CreateFaqCategoryResponse>> Handle(CreateFaqCategoryCommand command, CancellationToken cancellationToken)
    {
        var normalizedName = command.Name.Trim();
        var normalizedNameLower = normalizedName.ToLower();

        var categoryExists = await context.FaqCategories
            .AnyAsync(
                c => !c.IsDeleted && c.Name.ToLower() == normalizedNameLower,
                cancellationToken);

        if (categoryExists)
        {
            return Result.Failure<CreateFaqCategoryResponse>(FaqErrors.CategoryNameAlreadyExists(normalizedName));
        }

        var now = VietnamTime.UtcNow();
        var category = new FaqCategory
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            Icon = string.IsNullOrWhiteSpace(command.Icon) ? null : command.Icon.Trim(),
            SortOrder = command.SortOrder,
            IsActive = command.IsActive,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.FaqCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateFaqCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            IsDeleted = category.IsDeleted,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}
