using FluentValidation;
using Kidzgo.Application.Services;

namespace Kidzgo.Application.SlotTypes.UpdateSlotType;

public sealed class UpdateSlotTypeCommandValidator : AbstractValidator<UpdateSlotTypeCommand>
{
    public UpdateSlotTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Description)
            .MaximumLength(500);

        RuleFor(x => x.DayGroup)
            .Must(TicketCompatibilityRuleSupport.IsValidSingleDayGroup)
            .WithMessage("DayGroup is invalid.");

        RuleFor(x => x.TimeBand)
            .Must(TicketCompatibilityRuleSupport.IsValidSingleTimeBand)
            .WithMessage("TimeBand is invalid.");

        RuleFor(x => x.TeacherType)
            .Must(TicketCompatibilityRuleSupport.IsValidSingleTeacherType)
            .WithMessage("TeacherType is invalid.");

        RuleFor(x => x.UsageType)
            .Must(TicketCompatibilityRuleSupport.IsValidSingleUsageType)
            .WithMessage("UsageType is invalid.");
    }
}

