using FluentValidation;

namespace Kidzgo.Application.ReportsV3.ShareReport;

public sealed class UpdateShareCallbackCommandValidator : AbstractValidator<UpdateShareCallbackCommand>
{
    public UpdateShareCallbackCommandValidator()
    {
        RuleFor(x => x.ProviderMessageId).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.ErrorMessage));
    }
}
