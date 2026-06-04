using FluentValidation;

namespace Kidzgo.Application.TicketTypeCompatibilities.UpsertTicketTypeCompatibilityOverrides;

public sealed class UpsertTicketTypeCompatibilityOverridesCommandValidator
    : AbstractValidator<UpsertTicketTypeCompatibilityOverridesCommand>
{
    public UpsertTicketTypeCompatibilityOverridesCommandValidator()
    {
        RuleFor(x => x.LearningTicketTypeId)
            .NotEmpty();

        RuleFor(x => x.Items)
            .NotNull();

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.SlotTypeId)
                    .NotEmpty();
            });

        RuleFor(x => x.Items)
            .Must(items => items is not null && items.Select(i => i.SlotTypeId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate slot type ids are not allowed.");
    }
}
