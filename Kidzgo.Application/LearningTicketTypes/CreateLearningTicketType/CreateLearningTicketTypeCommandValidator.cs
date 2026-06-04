using FluentValidation;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.LearningTicketTypes.CreateLearningTicketType;

public sealed class CreateLearningTicketTypeCommandValidator : AbstractValidator<CreateLearningTicketTypeCommand>
{
    public CreateLearningTicketTypeCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleForEach(x => x.AllowedDayGroups)
            .Must(value => value != SlotDayGroup.None && TicketCompatibilityRuleSupport.IsValidSingleDayGroup(value))
            .WithMessage("Allowed day groups only support Weekday or Weekend.");

        RuleForEach(x => x.AllowedTimeBands)
            .Must(value => value != SlotTimeBand.None && TicketCompatibilityRuleSupport.IsValidSingleTimeBand(value))
            .WithMessage("Allowed time bands only support Morning, Afternoon, or Evening.");

        RuleForEach(x => x.AllowedTeacherTypes)
            .Must(value => value != SlotTeacherType.None && TicketCompatibilityRuleSupport.IsValidSingleTeacherType(value))
            .WithMessage("Allowed teacher types only support Standard or Native.");

        RuleForEach(x => x.AllowedUsageTypes)
            .Must(value => value != SlotUsageType.None && TicketCompatibilityRuleSupport.IsValidSingleUsageType(value))
            .WithMessage("Allowed usage types are invalid.");
    }
}

