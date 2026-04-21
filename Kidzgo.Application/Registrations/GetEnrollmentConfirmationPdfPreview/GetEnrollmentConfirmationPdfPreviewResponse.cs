namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;

public sealed class GetEnrollmentConfirmationPdfPreviewResponse
{
    public Guid RegistrationId { get; init; }
    public Guid EnrollmentId { get; init; }
    public string TrackRequested { get; init; } = null!;
    public string TrackResolved { get; init; } = null!;
    public string FormTypeRequested { get; init; } = null!;
    public string FormTypeResolved { get; init; } = null!;
    public bool CanGenerate { get; init; }
    public string PaymentSettingScope { get; init; } = null!;
    public List<string> Warnings { get; init; } = new();
    public EnrollmentConfirmationPdfActiveFileDto? ActivePdf { get; init; }
    public EnrollmentConfirmationPdfPreviewDto Preview { get; init; } = null!;
}

public sealed class EnrollmentConfirmationPdfActiveFileDto
{
    public Guid PdfRecordId { get; init; }
    public string PdfUrl { get; init; } = null!;
    public DateTime GeneratedAt { get; init; }
    public Guid? GeneratedBy { get; init; }
    public string? GeneratedByName { get; init; }
    public bool IsActive { get; init; }
    public bool HasSnapshot { get; init; }
}

public sealed class EnrollmentConfirmationPdfPreviewDto
{
    public string StudentName { get; init; } = null!;
    public DateOnly? StudentDateOfBirth { get; init; }
    public string? ParentName { get; init; }
    public string? ParentPhoneNumber { get; init; }
    public string BranchName { get; init; } = null!;
    public string? BranchAddress { get; init; }
    public string? BranchPhoneNumber { get; init; }
    public string ProgramName { get; init; } = null!;
    public string ProgramCode { get; init; } = null!;
    public string ClassCode { get; init; } = null!;
    public string ClassTitle { get; init; } = null!;
    public string? TeacherName { get; init; }
    public DateOnly EnrollDate { get; init; }
    public DateOnly? FirstStudyDate { get; init; }
    public DateOnly? ExpectedEndDate { get; init; }
    public string? StudyDaySummary { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public string CourseDurationText { get; init; } = null!;
    public int TotalSessions { get; init; }
    public decimal TuitionAmount { get; init; }
    public decimal UnitPriceSession { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal MaterialFee { get; init; }
    public decimal TotalPayment { get; init; }
    public string Currency { get; init; } = null!;
    public string Track { get; init; } = null!;
    public string EntryType { get; init; } = null!;
    public DateTime GeneratedAt { get; init; }
    public string? IssuedByName { get; init; }
    public string PaymentMethod { get; init; } = null!;
    public string? PaymentAccountName { get; init; }
    public string? PaymentAccountNumber { get; init; }
    public string? PaymentBankName { get; init; }
    public string? PaymentTransferContent { get; init; }
    public string? PaymentQrUrl { get; init; }
    public string? HeaderLogoUrl { get; init; }
    public EnrollmentConfirmationPdfReconciliationDto? Reconciliation { get; init; }
}

public sealed class EnrollmentConfirmationPdfReconciliationDto
{
    public string? PreviousClassCode { get; init; }
    public string? PreviousClassTitle { get; init; }
    public string? PreviousProgramName { get; init; }
    public string? PreviousTeacherName { get; init; }
    public DateOnly? CourseStartDate { get; init; }
    public DateOnly? CourseEndDate { get; init; }
    public int TotalSessions { get; init; }
    public int AssignedSessionCount { get; init; }
    public int ExcusedAbsenceCount { get; init; }
    public string? ExcusedAbsenceDetails { get; init; }
    public int UnexcusedAbsenceCount { get; init; }
    public string? UnexcusedAbsenceDetails { get; init; }
    public int MakeupScheduledCount { get; init; }
    public string? MakeupScheduledDetails { get; init; }
    public DateOnly? ReconciledEndDate { get; init; }
    public EnrollmentConfirmationPdfReservationDto? Reservation { get; init; }
    public string? Note { get; init; }
}

public sealed class EnrollmentConfirmationPdfReservationDto
{
    public int ReservedSessionCount { get; init; }
    public DateOnly? PauseFrom { get; init; }
    public DateOnly? PauseTo { get; init; }
    public DateOnly? ReservationExpiresOn { get; init; }
}
