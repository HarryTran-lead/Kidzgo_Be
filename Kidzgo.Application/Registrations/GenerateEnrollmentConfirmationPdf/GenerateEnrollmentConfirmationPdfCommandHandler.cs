using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GenerateEnrollmentConfirmationPdf;

public sealed class GenerateEnrollmentConfirmationPdfCommandHandler(
    IDbContext context,
    IEnrollmentConfirmationPdfGenerator pdfGenerator,
    IFileStorageService fileStorage,
    IUserContext userContext,
    ILogger<GenerateEnrollmentConfirmationPdfCommandHandler> logger
) : ICommandHandler<GenerateEnrollmentConfirmationPdfCommand, GenerateEnrollmentConfirmationPdfResponse>
{
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Result<GenerateEnrollmentConfirmationPdfResponse>> Handle(
        GenerateEnrollmentConfirmationPdfCommand command,
        CancellationToken cancellationToken)
    {
        var previewResult = await EnrollmentConfirmationPdfPreviewBuilder.BuildAsync(
            context,
            userContext,
            command.RegistrationId,
            command.Track,
            command.FormType,
            cancellationToken);

        if (previewResult.IsFailure)
        {
            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(previewResult.Error);
        }

        var preview = previewResult.Value;
        var registration = preview.Registration;
        var enrollment = preview.Enrollment;
        var track = preview.Track;
        var formType = preview.FormType;
        var firstStudyDate = preview.FirstStudyDate;
        var tuitionPlan = preview.TuitionPlan;
        var existingPdf = preview.ActivePdf;

        if (!command.Regenerate && existingPdf is not null)
        {
            return BuildResponse(
                registration,
                enrollment,
                existingPdf.Id,
                track,
                formType,
                GetDownloadUrl(existingPdf.PdfUrl),
                existingPdf.GeneratedAt,
                reusedExistingPdf: true,
                firstStudyDate,
                tuitionPlan);
        }

        var document = preview.Document;
        var now = document.GeneratedAt;

        try
        {
            var pdfUrl = await pdfGenerator.GeneratePdfAsync(document, cancellationToken);

            var activePdfs = await context.EnrollmentConfirmationPdfs
                .Where(p => p.EnrollmentId == enrollment.Id &&
                            p.Track == track &&
                            p.FormType == formType &&
                            p.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var activePdf in activePdfs)
            {
                activePdf.IsActive = false;
            }

            var pdfRecord = new EnrollmentConfirmationPdf
            {
                Id = Guid.NewGuid(),
                RegistrationId = registration.Id,
                EnrollmentId = enrollment.Id,
                Track = track,
                FormType = formType,
                PdfUrl = pdfUrl,
                GeneratedAt = now,
                GeneratedBy = userContext.UserId,
                IsActive = true,
                SnapshotJson = JsonSerializer.Serialize(document, SnapshotJsonOptions)
            };

            context.EnrollmentConfirmationPdfs.Add(pdfRecord);

            enrollment.EnrollmentConfirmationPdfUrl = pdfUrl;
            enrollment.EnrollmentConfirmationPdfGeneratedAt = now;
            enrollment.EnrollmentConfirmationPdfGeneratedBy = userContext.UserId;
            enrollment.UpdatedAt = now;

            await context.SaveChangesAsync(cancellationToken);

            return BuildResponse(
                registration,
                enrollment,
                pdfRecord.Id,
                track,
                formType,
                GetDownloadUrl(pdfUrl),
                now,
                reusedExistingPdf: false,
                firstStudyDate,
                tuitionPlan);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate enrollment confirmation PDF for registration {RegistrationId}, enrollment {EnrollmentId}",
                registration.Id,
                enrollment.Id);

            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(
                Error.Failure(
                    "Registration.EnrollmentConfirmationPdfGenerationFailed",
                    $"Failed to generate enrollment confirmation PDF: {ex.Message}"));
        }
    }

    private string GetDownloadUrl(string pdfUrl)
    {
        try
        {
            return fileStorage.GetDownloadUrl(pdfUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create download URL for enrollment confirmation PDF");
            return pdfUrl;
        }
    }

    private static GenerateEnrollmentConfirmationPdfResponse BuildResponse(
        Registration registration,
        ClassEnrollment enrollment,
        Guid? pdfRecordId,
        string track,
        EnrollmentConfirmationPdfFormType formType,
        string pdfUrl,
        DateTime pdfGeneratedAt,
        bool reusedExistingPdf,
        DateOnly? firstStudyDate,
        TuitionPlan tuitionPlan)
    {
        return new GenerateEnrollmentConfirmationPdfResponse
        {
            RegistrationId = registration.Id,
            EnrollmentId = enrollment.Id,
            PdfRecordId = pdfRecordId,
            Track = track,
            FormType = EnrollmentConfirmationPdfPreviewBuilder.ToApiFormType(formType),
            PdfUrl = pdfUrl,
            PdfGeneratedAt = pdfGeneratedAt,
            ReusedExistingPdf = reusedExistingPdf,
            EnrollDate = enrollment.EnrollDate,
            FirstStudyDate = firstStudyDate,
            StudentName = registration.StudentProfile.DisplayName,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            ProgramName = enrollment.Class.Program.Name,
            TuitionPlanName = tuitionPlan.Name,
            TuitionAmount = tuitionPlan.TuitionAmount,
            DiscountAmount = registration.DiscountAmount ?? 0m,
            CarryOverCreditAmount = registration.CarryOverCreditAmount ?? 0m,
            FinalTuitionAmount = registration.FinalTuitionAmount ?? ((registration.OriginalTuitionAmount ?? tuitionPlan.TuitionAmount) - (registration.DiscountAmount ?? 0m) - (registration.CarryOverCreditAmount ?? 0m)),
            Currency = tuitionPlan.Currency
        };
    }
}
