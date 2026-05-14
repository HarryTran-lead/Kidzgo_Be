using FluentValidation;

namespace Kidzgo.Application.Sessions.UpdateSessionSectionType;

public sealed class UpdateSessionSectionTypeCommandValidator : AbstractValidator<UpdateSessionSectionTypeCommand>
{
    public UpdateSessionSectionTypeCommandValidator()
    {
        RuleFor(c => c.SessionId)
            .NotEmpty();
    }
}
