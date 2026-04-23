using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Faqs;
using Kidzgo.Domain.Faqs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Faqs.CreateFaq;

public sealed class CreateFaqCommandHandler(
    IDbContext context
) : ICommandHandler<CreateFaqCommand, CreateFaqResponse>
{
    public async Task<Result<CreateFaqResponse>> Handle(CreateFaqCommand command, CancellationToken cancellationToken)
    {
        var category = await context.FaqCategories
            .FirstOrDefaultAsync(c => c.Id == command.CategoryId && !c.IsDeleted, cancellationToken);

        if (category is null)
        {
            return Result.Failure<CreateFaqResponse>(FaqErrors.CategoryNotFound(command.CategoryId));
        }

        var now = VietnamTime.UtcNow();
        var faq = new FaqItem
        {
            Id = Guid.NewGuid(),
            CategoryId = command.CategoryId,
            Question = command.Question.Trim(),
            Answer = command.Answer.Trim(),
            SortOrder = command.SortOrder,
            IsPublished = command.IsPublished,
            IsDeleted = false,
            PublishedAt = command.IsPublished ? now : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.FaqItems.Add(faq);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateFaqResponse
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
            CreatedAt = faq.CreatedAt,
            UpdatedAt = faq.UpdatedAt
        };
    }
}
