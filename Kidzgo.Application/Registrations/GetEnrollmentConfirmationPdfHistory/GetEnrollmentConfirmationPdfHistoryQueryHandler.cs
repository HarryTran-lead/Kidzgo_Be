using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;

public sealed class GetEnrollmentConfirmationPdfHistoryQueryHandler(
    IDbContext context,
    IFileStorageService fileStorage,
    ILogger<GetEnrollmentConfirmationPdfHistoryQueryHandler> logger)
    : IQueryHandler<GetEnrollmentConfirmationPdfHistoryQuery, GetEnrollmentConfirmationPdfHistoryResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPdfHistoryResponse>> Handle(
        GetEnrollmentConfirmationPdfHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var registrationExists = await context.Registrations
            .AsNoTracking()
            .AnyAsync(r => r.Id == query.RegistrationId, cancellationToken);

        if (!registrationExists)
        {
            return Result.Failure<GetEnrollmentConfirmationPdfHistoryResponse>(
                RegistrationErrors.NotFound(query.RegistrationId));
        }

        var normalizedFormType = string.Equals(query.FormType, "auto", StringComparison.OrdinalIgnoreCase)
            ? null
            : query.FormType;

        var parsedFormType = EnrollmentConfirmationPdfPreviewBuilder.ParseFormTypeFilter(normalizedFormType);
        if (parsedFormType.IsFailure)
        {
            return Result.Failure<GetEnrollmentConfirmationPdfHistoryResponse>(parsedFormType.Error);
        }

        var normalizedTrack = string.IsNullOrWhiteSpace(query.Track)
            ? null
            : RegistrationTrackHelper.NormalizeTrack(query.Track);

        var pdfsQuery = context.EnrollmentConfirmationPdfs
            .AsNoTracking()
            .Where(p => p.RegistrationId == query.RegistrationId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedTrack))
        {
            pdfsQuery = pdfsQuery.Where(p => p.Track == normalizedTrack);
        }

        if (parsedFormType.Value.HasValue)
        {
            pdfsQuery = pdfsQuery.Where(p => p.FormType == parsedFormType.Value.Value);
        }

        var totalCount = await pdfsQuery.CountAsync(cancellationToken);

        var rows = await pdfsQuery
            .OrderByDescending(p => p.GeneratedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(p => new HistoryRow
            {
                PdfRecordId = p.Id,
                RegistrationId = p.RegistrationId,
                EnrollmentId = p.EnrollmentId,
                Track = p.Track,
                FormType = p.FormType,
                PdfUrl = p.PdfUrl,
                GeneratedAt = p.GeneratedAt,
                GeneratedBy = p.GeneratedBy,
                IsActive = p.IsActive,
                HasSnapshot = p.SnapshotJson != null,
                StudentName = p.Registration.StudentProfile.DisplayName,
                ClassCode = p.Enrollment.Class.Code,
                ClassTitle = p.Enrollment.Class.Title,
                ProgramName = p.Enrollment.Class.Program.Name
            })
            .ToListAsync(cancellationToken);

        var generatedByIds = rows
            .Where(row => row.GeneratedBy.HasValue)
            .Select(row => row.GeneratedBy!.Value)
            .Distinct()
            .ToList();

        var generatedByLookup = generatedByIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await context.Users
                .AsNoTracking()
                .Where(u => generatedByIds.Contains(u.Id))
                .ToDictionaryAsync(
                    u => u.Id,
                    u => (string?)(u.Name ?? u.Email),
                    cancellationToken);

        var items = rows
            .Select(row => new EnrollmentConfirmationPdfHistoryItemDto
            {
                PdfRecordId = row.PdfRecordId,
                RegistrationId = row.RegistrationId,
                EnrollmentId = row.EnrollmentId,
                Track = row.Track,
                FormType = EnrollmentConfirmationPdfPreviewBuilder.ToApiFormType(row.FormType),
                PdfUrl = GetDownloadUrl(row.PdfUrl),
                GeneratedAt = row.GeneratedAt,
                GeneratedBy = row.GeneratedBy,
                GeneratedByName = row.GeneratedBy.HasValue && generatedByLookup.TryGetValue(row.GeneratedBy.Value, out var generatedByName)
                    ? generatedByName
                    : null,
                IsActive = row.IsActive,
                HasSnapshot = row.HasSnapshot,
                StudentName = row.StudentName,
                ClassCode = row.ClassCode,
                ClassTitle = row.ClassTitle,
                ProgramName = row.ProgramName
            })
            .ToList();

        return Result.Success(new GetEnrollmentConfirmationPdfHistoryResponse
        {
            Pdfs = new Page<EnrollmentConfirmationPdfHistoryItemDto>(
                items,
                totalCount,
                query.PageNumber,
                query.PageSize)
        });
    }

    private string GetDownloadUrl(string pdfUrl)
    {
        try
        {
            return fileStorage.GetDownloadUrl(pdfUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to create download URL for enrollment confirmation PDF history");
            return pdfUrl;
        }
    }

    private sealed class HistoryRow
    {
        public Guid PdfRecordId { get; init; }
        public Guid RegistrationId { get; init; }
        public Guid EnrollmentId { get; init; }
        public string Track { get; init; } = null!;
        public Kidzgo.Domain.Registrations.EnrollmentConfirmationPdfFormType FormType { get; init; }
        public string PdfUrl { get; init; } = null!;
        public DateTime GeneratedAt { get; init; }
        public Guid? GeneratedBy { get; init; }
        public bool IsActive { get; init; }
        public bool HasSnapshot { get; init; }
        public string? StudentName { get; init; }
        public string? ClassCode { get; init; }
        public string? ClassTitle { get; init; }
        public string? ProgramName { get; init; }
    }
}
