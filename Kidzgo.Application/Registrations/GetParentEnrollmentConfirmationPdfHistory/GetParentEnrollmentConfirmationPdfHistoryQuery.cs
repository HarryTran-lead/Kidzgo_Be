using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;

namespace Kidzgo.Application.Registrations.GetParentEnrollmentConfirmationPdfHistory;

public sealed class GetParentEnrollmentConfirmationPdfHistoryQuery
    : IQuery<GetEnrollmentConfirmationPdfHistoryResponse>
{
    public Guid RegistrationId { get; init; }
    public string? Track { get; init; }
    public string? FormType { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
