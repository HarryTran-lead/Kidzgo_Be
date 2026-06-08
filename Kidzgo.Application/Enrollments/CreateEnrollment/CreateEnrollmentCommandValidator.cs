using FluentValidation;

namespace Kidzgo.Application.Enrollments.CreateEnrollment;

public sealed class CreateEnrollmentCommandValidator : AbstractValidator<CreateEnrollmentCommand>
{
    public CreateEnrollmentCommandValidator()
    {
        RuleFor(command => command.ClassId)
            .NotEmpty().WithMessage("Class ID is required");

        RuleFor(command => command.StudentProfileId)
            .NotEmpty().WithMessage("Student Profile ID is required");

        RuleFor(command => command.EnrollDate)
            .NotEmpty().WithMessage("Enroll date is required");

        RuleFor(command => command.RegistrationId)
            .NotEmpty().WithMessage("Registration ID must not be empty")
            .When(command => command.RegistrationId.HasValue);
    }
}

