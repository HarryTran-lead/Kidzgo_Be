using Kidzgo.Domain.Common;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;

public sealed class GetEnrollmentConfirmationPdfHistoryResponse
{
    public Page<EnrollmentConfirmationPdfHistoryItemDto> Pdfs { get; init; } = null!;
}

public sealed class EnrollmentConfirmationPdfHistoryItemDto
{
    public Guid PdfRecordId { get; init; }
    public Guid RegistrationId { get; init; }
    public Guid EnrollmentId { get; init; }
    public string Track { get; init; } = null!;
    public string FormType { get; init; } = null!;
    public string PdfUrl { get; init; } = null!;
    public DateTime GeneratedAt { get; init; }
    public Guid? GeneratedBy { get; init; }
    public string? GeneratedByName { get; init; }
    public bool IsActive { get; init; }
    public bool HasSnapshot { get; init; }
    public string? StudentName { get; init; }
    public string? ClassCode { get; init; }
    public string? ClassTitle { get; init; }
    public string? ProgramName { get; init; }
}
