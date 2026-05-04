using FluentValidation;

namespace Kidzgo.Application.Sessions.ChangeSessionRoom;

public sealed class ChangeSessionRoomCommandValidator : AbstractValidator<ChangeSessionRoomCommand>
{
    public ChangeSessionRoomCommandValidator()
    {
        RuleFor(command => command.SessionIds)
            .NotEmpty()
            .WithMessage("At least one session ID is required");

        RuleForEach(command => command.SessionIds)
            .NotEmpty()
            .WithMessage("Session ID cannot be empty");

        RuleFor(command => command.RoomId)
            .NotEmpty()
            .WithMessage("Room ID is required");
    }
}

