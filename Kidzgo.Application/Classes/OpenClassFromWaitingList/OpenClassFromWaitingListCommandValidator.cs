using FluentValidation;
using Kidzgo.Application.Classes.CreateClass;
using Kidzgo.Application.Registrations;

namespace Kidzgo.Application.Classes.OpenClassFromWaitingList;

public sealed class OpenClassFromWaitingListCommandValidator : AbstractValidator<OpenClassFromWaitingListCommand>
{
    public OpenClassFromWaitingListCommandValidator()
    {
        RuleFor(command => command.CreateClass)
            .SetValidator(new CreateClassCommandValidator());

        RuleFor(command => command.Track)
            .NotEmpty().WithMessage("Track is required")
            .Must(track =>
                string.Equals(track, RegistrationTrackHelper.PrimaryTrack, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(track, RegistrationTrackHelper.SecondaryTrack, StringComparison.OrdinalIgnoreCase))
            .WithMessage("Track must be primary or secondary");
    }
}
