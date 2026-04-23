using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Faqs.Errors;

public static class FaqErrors
{
    public static Error CategoryNotFound(Guid? categoryId) => Error.NotFound(
        "FaqCategory.NotFound",
        $"FAQ category with Id = '{categoryId}' was not found");

    public static Error ItemNotFound(Guid? faqId) => Error.NotFound(
        "FaqItem.NotFound",
        $"FAQ item with Id = '{faqId}' was not found");

    public static Error CategoryNameAlreadyExists(string name) => Error.Conflict(
        "FaqCategory.NameAlreadyExists",
        $"FAQ category '{name}' already exists");

    public static readonly Error CategoryAlreadyDeleted = Error.Conflict(
        "FaqCategory.AlreadyDeleted",
        "FAQ category has already been deleted");

    public static readonly Error ItemAlreadyDeleted = Error.Conflict(
        "FaqItem.AlreadyDeleted",
        "FAQ item has already been deleted");

    public static readonly Error CategoryHasFaqItems = Error.Conflict(
        "FaqCategory.HasFaqItems",
        "Cannot delete FAQ category while it still contains FAQ items");
}
