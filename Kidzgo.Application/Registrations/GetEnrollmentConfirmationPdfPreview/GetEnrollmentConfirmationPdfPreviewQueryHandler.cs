using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;

public sealed class GetEnrollmentConfirmationPdfPreviewQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IFileStorageService fileStorage,
    ILogger<GetEnrollmentConfirmationPdfPreviewQueryHandler> logger)
    : IQueryHandler<GetEnrollmentConfirmationPdfPreviewQuery, GetEnrollmentConfirmationPdfPreviewResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPdfPreviewResponse>> Handle(
        GetEnrollmentConfirmationPdfPreviewQuery query,
        CancellationToken cancellationToken)
    {
        var buildResult = await EnrollmentConfirmationPdfPreviewBuilder.BuildAsync(
            context,
            userContext,
            query.RegistrationId,
            query.Track,
            query.FormType,
            cancellationToken);

        if (buildResult.IsFailure)
        {
            return Result.Failure<GetEnrollmentConfirmationPdfPreviewResponse>(buildResult.Error);
        }

        var preview = buildResult.Value;
        var activePdf = preview.ActivePdf;

        return Result.Success(new GetEnrollmentConfirmationPdfPreviewResponse
        {
            RegistrationId = preview.Registration.Id,
            EnrollmentId = preview.Enrollment.Id,
            TrackRequested = string.IsNullOrWhiteSpace(query.Track)
                ? RegistrationTrackHelper.PrimaryTrack
                : query.Track.Trim(),
            TrackResolved = preview.Track,
            FormTypeRequested = string.IsNullOrWhiteSpace(query.FormType)
                ? "auto"
                : query.FormType.Trim(),
            FormTypeResolved = EnrollmentConfirmationPdfPreviewBuilder.ToApiFormType(preview.FormType),
            CanGenerate = true,
            PaymentSettingScope = preview.PaymentSettingScope,
            Warnings = preview.Warnings.ToList(),
            ActivePdf = activePdf is null
                ? null
                : new EnrollmentConfirmationPdfActiveFileDto
                {
                    PdfRecordId = activePdf.Id,
                    PdfUrl = GetDownloadUrl(activePdf.PdfUrl),
                    GeneratedAt = activePdf.GeneratedAt,
                    GeneratedBy = activePdf.GeneratedBy,
                    GeneratedByName = preview.ActivePdfGeneratedByName,
                    IsActive = activePdf.IsActive,
                    HasSnapshot = !string.IsNullOrWhiteSpace(activePdf.SnapshotJson)
                },
            Preview = MapPreview(preview.Document)
        });
    }

    private EnrollmentConfirmationPdfPreviewDto MapPreview(
        Kidzgo.Application.Abstraction.Reports.EnrollmentConfirmationPdfDocument document)
    {
        return new EnrollmentConfirmationPdfPreviewDto
        {
            StudentName = document.StudentName,
            StudentDateOfBirth = document.StudentDateOfBirth,
            ParentName = document.ParentName,
            ParentPhoneNumber = document.ParentPhoneNumber,
            BranchName = document.BranchName,
            BranchAddress = document.BranchAddress,
            BranchPhoneNumber = document.BranchPhoneNumber,
            ProgramName = document.ProgramName,
            ProgramCode = document.ProgramCode,
            ClassCode = document.ClassCode,
            ClassTitle = document.ClassTitle,
            TeacherName = document.TeacherName,
            EnrollDate = document.EnrollDate,
            FirstStudyDate = document.FirstStudyDate,
            ExpectedEndDate = document.ExpectedEndDate,
            StudyDaySummary = document.StudyDaySummary,
            TuitionPlanName = document.TuitionPlanName,
            CourseDurationText = document.CourseDurationText,
            TotalSessions = document.TotalSessions,
            TuitionAmount = document.TuitionAmount,
            UnitPriceSession = document.UnitPriceSession,
            DiscountAmount = document.DiscountAmount,
            MaterialFee = document.MaterialFee,
            TotalPayment = document.TotalPayment,
            Currency = document.Currency,
            Track = document.Track,
            EntryType = document.EntryType,
            GeneratedAt = document.GeneratedAt,
            IssuedByName = document.IssuedByName,
            PaymentMethod = document.PaymentMethod,
            PaymentAccountName = document.PaymentAccountName,
            PaymentAccountNumber = document.PaymentAccountNumber,
            PaymentBankName = document.PaymentBankName,
            PaymentTransferContent = document.PaymentTransferContent,
            PaymentQrUrl = document.PaymentQrUrl,
            HeaderLogoUrl = document.HeaderLogoUrl,
            Reconciliation = document.Reconciliation is null
                ? null
                : new EnrollmentConfirmationPdfReconciliationDto
                {
                    PreviousClassCode = document.Reconciliation.PreviousClassCode,
                    PreviousClassTitle = document.Reconciliation.PreviousClassTitle,
                    PreviousProgramName = document.Reconciliation.PreviousProgramName,
                    PreviousTeacherName = document.Reconciliation.PreviousTeacherName,
                    CourseStartDate = document.Reconciliation.CourseStartDate,
                    CourseEndDate = document.Reconciliation.CourseEndDate,
                    TotalSessions = document.Reconciliation.TotalSessions,
                    AssignedSessionCount = document.Reconciliation.AssignedSessionCount,
                    ExcusedAbsenceCount = document.Reconciliation.ExcusedAbsenceCount,
                    ExcusedAbsenceDetails = document.Reconciliation.ExcusedAbsenceDetails,
                    UnexcusedAbsenceCount = document.Reconciliation.UnexcusedAbsenceCount,
                    UnexcusedAbsenceDetails = document.Reconciliation.UnexcusedAbsenceDetails,
                    MakeupScheduledCount = document.Reconciliation.MakeupScheduledCount,
                    MakeupScheduledDetails = document.Reconciliation.MakeupScheduledDetails,
                    ReconciledEndDate = document.Reconciliation.ReconciledEndDate,
                    Reservation = document.Reconciliation.Reservation is null
                        ? null
                        : new EnrollmentConfirmationPdfReservationDto
                        {
                            ReservedSessionCount = document.Reconciliation.Reservation.ReservedSessionCount,
                            PauseFrom = document.Reconciliation.Reservation.PauseFrom,
                            PauseTo = document.Reconciliation.Reservation.PauseTo,
                            ReservationExpiresOn = document.Reconciliation.Reservation.ReservationExpiresOn
                        },
                    Note = document.Reconciliation.Note
                }
        };
    }

    private string GetDownloadUrl(string pdfUrl)
    {
        try
        {
            return fileStorage.GetDownloadUrl(pdfUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create download URL for enrollment confirmation PDF preview");
            return pdfUrl;
        }
    }
}
