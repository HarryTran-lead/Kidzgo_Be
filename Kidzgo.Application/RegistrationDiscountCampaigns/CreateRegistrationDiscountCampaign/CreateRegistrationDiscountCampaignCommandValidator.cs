using FluentValidation;
using Kidzgo.Domain.Registrations;

namespace Kidzgo.Application.RegistrationDiscountCampaigns.CreateRegistrationDiscountCampaign;

public sealed class CreateRegistrationDiscountCampaignCommandValidator : AbstractValidator<CreateRegistrationDiscountCampaignCommand>
{
    public CreateRegistrationDiscountCampaignCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Code)
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.DiscountType)
            .IsInEnum();

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0m);

        RuleFor(x => x.Priority)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate);

        RuleFor(x => x)
            .Must(x => x.ApplyForInitialRegistration || x.ApplyForRenewal || x.ApplyForUpgrade)
            .WithMessage("Campaign must apply to at least one of initial registration, renewal, or upgrade.");

        When(x => x.DiscountType == RegistrationDiscountType.Percentage, () =>
        {
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100m);
        });
    }
}
