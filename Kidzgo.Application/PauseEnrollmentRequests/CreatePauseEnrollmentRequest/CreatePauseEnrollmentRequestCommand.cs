using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestCommand : ICommand<CreatePauseEnrollmentRequestResponse>
{
    public Guid StudentProfileId { get; init; }
    public DateOnly PauseFrom { get; init; }
    public DateOnly PauseTo { get; init; }
    public string? Reason { get; init; }
    public Guid? ClassId { get; init; }
}
