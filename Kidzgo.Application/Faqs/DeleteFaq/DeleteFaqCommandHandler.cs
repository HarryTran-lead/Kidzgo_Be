using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Faqs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Faqs.DeleteFaq;

public sealed class DeleteFaqCommandHandler(
    IDbContext context
) : ICommandHandler<DeleteFaqCommand, DeleteFaqResponse>
{
    public async Task<Result<DeleteFaqResponse>> Handle(DeleteFaqCommand command, CancellationToken cancellationToken)
    {
        var faq = await context.FaqItems
            .FirstOrDefaultAsync(f => f.Id == command.Id, cancellationToken);

        if (faq is null)
        {
            return Result.Failure<DeleteFaqResponse>(FaqErrors.ItemNotFound(command.Id));
        }

        if (faq.IsDeleted)
        {
            return Result.Failure<DeleteFaqResponse>(FaqErrors.ItemAlreadyDeleted);
        }

        faq.IsDeleted = true;
        faq.IsPublished = false;
        faq.PublishedAt = null;
        faq.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteFaqResponse
        {
            Id = faq.Id,
            Question = faq.Question,
            IsDeleted = faq.IsDeleted,
            UpdatedAt = faq.UpdatedAt
        };
    }
}
