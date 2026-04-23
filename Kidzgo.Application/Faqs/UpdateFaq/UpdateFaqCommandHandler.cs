using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Faqs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Faqs.UpdateFaq;

public sealed class UpdateFaqCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateFaqCommand, UpdateFaqResponse>
{
    public async Task<Result<UpdateFaqResponse>> Handle(UpdateFaqCommand command, CancellationToken cancellationToken)
    {
        var faq = await context.FaqItems
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == command.Id, cancellationToken);

        if (faq is null || faq.IsDeleted)
        {
            return Result.Failure<UpdateFaqResponse>(FaqErrors.ItemNotFound(command.Id));
        }

        var category = await context.FaqCategories
            .FirstOrDefaultAsync(c => c.Id == command.CategoryId && !c.IsDeleted, cancellationToken);

        if (category is null)
        {
            return Result.Failure<UpdateFaqResponse>(FaqErrors.CategoryNotFound(command.CategoryId));
        }

        var now = VietnamTime.UtcNow();

        faq.CategoryId = command.CategoryId;
        faq.Question = command.Question.Trim();
        faq.Answer = command.Answer.Trim();
        faq.SortOrder = command.SortOrder;

        if (command.IsPublished && !faq.IsPublished)
        {
            faq.PublishedAt = now;
        }
        else if (!command.IsPublished)
        {
            faq.PublishedAt = null;
        }

        faq.IsPublished = command.IsPublished;
        faq.UpdatedAt = now;

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateFaqResponse
        {
            Id = faq.Id,
            CategoryId = faq.CategoryId,
            CategoryName = category.Name,
            Question = faq.Question,
            Answer = faq.Answer,
            SortOrder = faq.SortOrder,
            IsPublished = faq.IsPublished,
            IsDeleted = faq.IsDeleted,
            PublishedAt = faq.PublishedAt,
            UpdatedAt = faq.UpdatedAt
        };
    }
}
