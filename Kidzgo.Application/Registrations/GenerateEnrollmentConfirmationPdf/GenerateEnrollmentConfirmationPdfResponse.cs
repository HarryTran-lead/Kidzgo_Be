namespace Kidzgo.Application.Registrations.GenerateEnrollmentConfirmationPdf;

public sealed class GenerateEnrollmentConfirmationPdfResponse
{
    public Guid RegistrationId { get; init; }
    public Guid EnrollmentId { get; init; }
    public Guid? PdfRecordId { get; init; }
    public string Track { get; init; } = null!;
    public string FormType { get; init; } = null!;
    public string PdfUrl { get; init; } = null!;
    public DateTime PdfGeneratedAt { get; init; }
    public bool ReusedExistingPdf { get; init; }
    public DateOnly EnrollDate { get; init; }
    public DateOnly? FirstStudyDate { get; init; }
    public string StudentName { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public string ProgramName { get; init; } = null!;
    public string TuitionPlanName { get; init; } = null!;
    public decimal TuitionAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal CarryOverCreditAmount { get; init; }
    public decimal FinalTuitionAmount { get; init; }
    public string Currency { get; init; } = null!;
}
