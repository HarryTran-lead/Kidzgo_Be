using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;

public sealed class GetEnrollmentConfirmationPdfHistoryQuery
    : IQuery<GetEnrollmentConfirmationPdfHistoryResponse>
{
    public Guid RegistrationId { get; init; }
    public string? Track { get; init; }
    public string? FormType { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
