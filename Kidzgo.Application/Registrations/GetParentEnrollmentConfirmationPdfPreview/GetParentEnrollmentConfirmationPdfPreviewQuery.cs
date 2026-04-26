using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;

namespace Kidzgo.Application.Registrations.GetParentEnrollmentConfirmationPdfPreview;

public sealed class GetParentEnrollmentConfirmationPdfPreviewQuery
    : IQuery<GetEnrollmentConfirmationPdfPreviewResponse>
{
    public Guid RegistrationId { get; init; }
    public string Track { get; init; } = RegistrationTrackHelper.PrimaryTrack;
    public string FormType { get; init; } = "auto";
}
