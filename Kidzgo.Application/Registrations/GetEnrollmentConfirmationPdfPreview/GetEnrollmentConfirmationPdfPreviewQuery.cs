using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;

public sealed class GetEnrollmentConfirmationPdfPreviewQuery
    : IQuery<GetEnrollmentConfirmationPdfPreviewResponse>
{
    public Guid RegistrationId { get; init; }
    public string Track { get; init; } = RegistrationTrackHelper.PrimaryTrack;
    public string FormType { get; init; } = "auto";
}
