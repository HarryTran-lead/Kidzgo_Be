using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Application.Registrations;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Kidzgo.Domain.Sessions;
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
        var track = RegistrationTrackHelper.NormalizeTrack(command.Track);
        var trackType = RegistrationTrackHelper.ToTrackType(track);

        var registration = await context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.TuitionPlan)
            .FirstOrDefaultAsync(r => r.Id == command.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(
                RegistrationErrors.NotFound(command.RegistrationId));
        }

        var formTypeResult = await ResolveFormTypeAsync(registration, command.FormType, cancellationToken);
        if (formTypeResult.Error is not null)
        {
            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(formTypeResult.Error);
        }

        var formType = formTypeResult.FormType!.Value;

        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.Class)
                .ThenInclude(c => c.MainTeacher)
            .Include(e => e.TuitionPlan)
            .Include(e => e.ScheduleSegments)
            .Where(e => e.RegistrationId == registration.Id &&
                        e.Track == trackType &&
                        e.Status == EnrollmentStatus.Active)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<GenerateEnrollmentConfirmationPdfResponse>(
                Error.NotFound(
                    "Registration.EnrollmentNotFound",
                    $"No active enrollment was found for registration {registration.Id} and track '{track}'."));
        }

        var studyDateRange = await GetStudyDateRangeAsync(enrollment.Id, cancellationToken);
        var firstStudyDate = studyDateRange.FirstDate;
        var expectedEndDate = studyDateRange.LastDate ?? enrollment.Class.EndDate;
        var tuitionPlan = enrollment.TuitionPlan ?? registration.TuitionPlan;

        var existingPdf = await context.EnrollmentConfirmationPdfs
            .AsNoTracking()
            .Where(p => p.EnrollmentId == enrollment.Id &&
                        p.Track == track &&
                        p.FormType == formType &&
                        p.IsActive)
            .OrderByDescending(p => p.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);

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

        var parentSource = await context.ParentStudentLinks
            .AsNoTracking()
            .Where(l => l.StudentProfileId == registration.StudentProfileId)
            .OrderBy(l => l.CreatedAt)
            .Select(l => new
            {
                UserName = l.ParentProfile.User.Name,
                ProfileName = l.ParentProfile.Name,
                ProfileDisplayName = l.ParentProfile.DisplayName,
                PhoneNumber = l.ParentProfile.User.PhoneNumber
            })
            .FirstOrDefaultAsync(cancellationToken);
        var parent = parentSource is null
            ? null
            : new ParentContactDto(
                FirstNonEmpty(parentSource.UserName, parentSource.ProfileName, parentSource.ProfileDisplayName),
                parentSource.PhoneNumber);

        var issuedByName = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == userContext.UserId)
            .Select(u => u.Name ?? u.Email)
            .FirstOrDefaultAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        var entryType = trackType == RegistrationTrackType.Secondary
            ? registration.SecondaryEntryType?.ToString()
            : registration.EntryType?.ToString();
        var schedule = RegistrationActualStudyScheduleMapper
            .Map(new[] { enrollment })
            .FirstOrDefault();
        var totalPayment = tuitionPlan.TuitionAmount;
        var paymentSetting = await GetPaymentSettingAsync(registration.BranchId, cancellationToken);
        var paymentTransferContent = $"{registration.StudentProfile.DisplayName} - {enrollment.Class.Code}";
        var paymentQrUrl = paymentSetting is null
            ? null
            : EnrollmentConfirmationPaymentQrBuilder.BuildVietQrUrl(
                paymentSetting.BankBin,
                paymentSetting.BankCode,
                paymentSetting.AccountNumber,
                paymentSetting.AccountName,
                paymentTransferContent,
                tuitionPlan.TuitionAmount,
                paymentSetting.VietQrTemplate);

        var document = new EnrollmentConfirmationPdfDocument
        {
            RegistrationId = registration.Id,
            EnrollmentId = enrollment.Id,
            FormType = formType,
            StudentName = registration.StudentProfile.DisplayName,
            StudentDateOfBirth = registration.StudentProfile.DateOfBirth,
            ParentName = parent?.Name,
            ParentPhoneNumber = parent?.PhoneNumber,
            BranchName = registration.Branch.Name,
            BranchAddress = registration.Branch.Address,
            BranchPhoneNumber = registration.Branch.ContactPhone,
            ProgramName = enrollment.Class.Program.Name,
            ProgramCode = enrollment.Class.Program.Code,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            TeacherName = enrollment.Class.MainTeacher?.Name ?? enrollment.Class.MainTeacher?.Email,
            EnrollDate = enrollment.EnrollDate,
            FirstStudyDate = firstStudyDate,
            ExpectedEndDate = expectedEndDate,
            StudyDaySummary = schedule?.StudyDaySummary,
            TuitionPlanName = tuitionPlan.Name,
            CourseDurationText = BuildCourseDurationText(tuitionPlan, firstStudyDate, expectedEndDate),
            TotalSessions = tuitionPlan.TotalSessions,
            TuitionAmount = tuitionPlan.TuitionAmount,
            UnitPriceSession = tuitionPlan.UnitPriceSession,
            DiscountAmount = 0m,
            MaterialFee = 0m,
            TotalPayment = totalPayment,
            Currency = tuitionPlan.Currency,
            Track = track,
            EntryType = entryType ?? string.Empty,
            GeneratedAt = now,
            IssuedByName = issuedByName,
            Reconciliation = formType == EnrollmentConfirmationPdfFormType.ContinuingStudent
                ? await BuildContinuingReconciliationAsync(registration, enrollment, cancellationToken)
                : null,
            PaymentMethod = paymentSetting?.PaymentMethod ?? "Tiền mặt / Chuyển khoản",
            PaymentAccountName = paymentSetting?.AccountName,
            PaymentAccountNumber = paymentSetting?.AccountNumber,
            PaymentBankName = paymentSetting?.BankName,
            PaymentTransferContent = paymentTransferContent,
            PaymentQrUrl = paymentQrUrl,
            HeaderLogoUrl = paymentSetting?.LogoUrl
        };

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

    private async Task<EnrollmentConfirmationPaymentSetting?> GetPaymentSettingAsync(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var branchScopeKey = EnrollmentConfirmationPaymentSetting.BuildScopeKey(branchId);
        var branchSetting = await context.EnrollmentConfirmationPaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ScopeKey == branchScopeKey && x.IsActive, cancellationToken);

        if (branchSetting is not null)
        {
            return branchSetting;
        }

        return await context.EnrollmentConfirmationPaymentSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ScopeKey == EnrollmentConfirmationPaymentSetting.BuildScopeKey(null) && x.IsActive,
                cancellationToken);
    }

    private async Task<FormTypeResolution> ResolveFormTypeAsync(
        Registration registration,
        string? requestedFormType,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(requestedFormType) ||
            string.Equals(requestedFormType, "auto", StringComparison.OrdinalIgnoreCase))
        {
            return new FormTypeResolution(
                await HasPriorLearningRegistrationAsync(registration, cancellationToken)
                    ? EnrollmentConfirmationPdfFormType.ContinuingStudent
                    : EnrollmentConfirmationPdfFormType.NewStudent,
                null);
        }

        return requestedFormType.Trim().ToLowerInvariant() switch
        {
            "new" or "newstudent" or "new-student" or "first" or "firsttime" or "first-time" =>
                new FormTypeResolution(EnrollmentConfirmationPdfFormType.NewStudent, null),
            "continuing" or "continuingstudent" or "continuing-student" or "continue" or "renewal" or "re-enroll" =>
                new FormTypeResolution(EnrollmentConfirmationPdfFormType.ContinuingStudent, null),
            _ => new FormTypeResolution(null, RegistrationErrors.InvalidEnrollmentConfirmationPdfFormType(requestedFormType))
        };
    }

    private async Task<bool> HasPriorLearningRegistrationAsync(
        Registration registration,
        CancellationToken cancellationToken)
    {
        if (registration.OriginalRegistrationId.HasValue ||
            registration.OperationType is OperationType.Renewal or OperationType.Upgrade)
        {
            return true;
        }

        return await context.Registrations
            .AsNoTracking()
            .AnyAsync(r => r.StudentProfileId == registration.StudentProfileId &&
                           r.Id != registration.Id &&
                           r.Status != RegistrationStatus.Cancelled &&
                           (r.RegistrationDate < registration.RegistrationDate ||
                            r.CreatedAt < registration.CreatedAt),
                cancellationToken);
    }

    private async Task<EnrollmentReconciliationPdfSection?> BuildContinuingReconciliationAsync(
        Registration registration,
        ClassEnrollment currentEnrollment,
        CancellationToken cancellationToken)
    {
        var previousRegistrationId = await GetPreviousRegistrationIdAsync(registration, cancellationToken);
        var previousEnrollmentsQuery = context.ClassEnrollments
            .AsNoTracking()
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.Class)
                .ThenInclude(c => c.MainTeacher)
            .Include(e => e.TuitionPlan)
            .Where(e => e.StudentProfileId == registration.StudentProfileId &&
                        e.Id != currentEnrollment.Id &&
                        e.Track == currentEnrollment.Track);

        previousEnrollmentsQuery = previousRegistrationId.HasValue
            ? previousEnrollmentsQuery.Where(e => e.RegistrationId == previousRegistrationId.Value)
            : previousEnrollmentsQuery.Where(e => e.CreatedAt < currentEnrollment.CreatedAt);

        var previousEnrollments = await previousEnrollmentsQuery
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

        if (previousEnrollments.Count == 0)
        {
            return new EnrollmentReconciliationPdfSection
            {
                Note = "Chưa có dữ liệu enrollment khóa trước để đối soát."
            };
        }

        var mainPreviousEnrollment = previousEnrollments[0];
        var previousEnrollmentIds = previousEnrollments.Select(e => e.Id).ToList();
        var previousRange = await GetStudyDateRangeAsync(previousEnrollmentIds, cancellationToken);
        var absences = await GetAbsenceRowsAsync(registration.StudentProfileId, previousEnrollmentIds, cancellationToken);
        var makeupRows = await GetMakeupRowsAsync(registration.StudentProfileId, previousEnrollmentIds, cancellationToken);
        var reservation = await GetReservationSectionAsync(
            registration.StudentProfileId,
            previousEnrollmentIds,
            cancellationToken);

        var excusedAbsences = absences
            .Where(a => a.AbsenceType is AbsenceType.WithNotice24H or AbsenceType.LongTerm)
            .ToList();
        var unexcusedAbsences = absences
            .Where(a => a.AbsenceType is AbsenceType.Under24H or AbsenceType.NoNotice)
            .ToList();
        var reconciledEndDate = MaxDate(
            previousRange.LastDate ?? mainPreviousEnrollment.Class.EndDate,
            MaxDate(makeupRows.Select(m => m.TargetDate)));

        return new EnrollmentReconciliationPdfSection
        {
            PreviousClassCode = mainPreviousEnrollment.Class.Code,
            PreviousClassTitle = mainPreviousEnrollment.Class.Title,
            PreviousProgramName = mainPreviousEnrollment.Class.Program.Name,
            PreviousTeacherName = mainPreviousEnrollment.Class.MainTeacher?.Name ?? mainPreviousEnrollment.Class.MainTeacher?.Email,
            CourseStartDate = previousRange.FirstDate ?? mainPreviousEnrollment.EnrollDate,
            CourseEndDate = previousRange.LastDate ?? mainPreviousEnrollment.Class.EndDate,
            TotalSessions = mainPreviousEnrollment.TuitionPlan?.TotalSessions ?? previousRange.AssignedSessionCount,
            AssignedSessionCount = previousRange.AssignedSessionCount,
            ExcusedAbsenceCount = excusedAbsences.Count,
            ExcusedAbsenceDetails = FormatDateList(excusedAbsences.Select(a => a.Date)),
            UnexcusedAbsenceCount = unexcusedAbsences.Count,
            UnexcusedAbsenceDetails = FormatDateList(unexcusedAbsences.Select(a => a.Date)),
            MakeupScheduledCount = makeupRows.Count,
            MakeupScheduledDetails = FormatDateList(makeupRows.Select(m => m.TargetDate)),
            ReconciledEndDate = reconciledEndDate,
            Reservation = reservation,
            Note = "Chỉ áp dụng học bù đối với các buổi nghỉ có phép theo chính sách trung tâm."
        };
    }

    private async Task<EnrollmentReservationPdfSection?> GetReservationSectionAsync(
        Guid studentProfileId,
        IReadOnlyCollection<Guid> enrollmentIds,
        CancellationToken cancellationToken)
    {
        if (enrollmentIds.Count == 0)
        {
            return null;
        }

        var rows = await context.PauseEnrollmentRequestHistories
            .AsNoTracking()
            .Where(history => history.StudentProfileId == studentProfileId &&
                              history.EnrollmentId.HasValue &&
                              enrollmentIds.Contains(history.EnrollmentId.Value) &&
                              history.NewStatus == EnrollmentStatus.Paused &&
                              history.PauseEnrollmentRequest.Status == PauseEnrollmentRequestStatus.Approved)
            .Select(history => new
            {
                history.PauseEnrollmentRequestId,
                history.PauseFrom,
                history.PauseTo,
                history.ReservedSessionCount,
                RequestReservedSessionCount = history.PauseEnrollmentRequest.ReservedSessionCount,
                history.PauseEnrollmentRequest.ReservationExpiresOn,
                SnapshotAt = history.PauseEnrollmentRequest.ReservationSnapshotAt,
                history.PauseEnrollmentRequest.ApprovedAt
            })
            .ToListAsync(cancellationToken);

        var latestRequest = rows
            .GroupBy(row => row.PauseEnrollmentRequestId)
            .Select(group => new
            {
                PauseFrom = group.Min(row => row.PauseFrom),
                PauseTo = group.Max(row => row.PauseTo),
                ReservedSessionCount = group.Sum(row => row.ReservedSessionCount),
                RequestReservedSessionCount = group.Max(row => row.RequestReservedSessionCount),
                ReservationExpiresOn = group.Select(row => row.ReservationExpiresOn).FirstOrDefault(date => date.HasValue),
                SnapshotAt = group.Select(row => row.SnapshotAt).FirstOrDefault(date => date.HasValue),
                ApprovedAt = group.Select(row => row.ApprovedAt).FirstOrDefault(date => date.HasValue)
            })
            .OrderByDescending(row => row.ApprovedAt)
            .ThenByDescending(row => row.PauseFrom)
            .FirstOrDefault();

        if (latestRequest is null || !latestRequest.SnapshotAt.HasValue)
        {
            return null;
        }

        var reservedSessionCount = latestRequest.ReservedSessionCount > 0
            ? latestRequest.ReservedSessionCount
            : latestRequest.RequestReservedSessionCount;

        return new EnrollmentReservationPdfSection
        {
            ReservedSessionCount = reservedSessionCount,
            PauseFrom = latestRequest.PauseFrom,
            PauseTo = latestRequest.PauseTo,
            ReservationExpiresOn = latestRequest.ReservationExpiresOn ?? latestRequest.PauseFrom.AddMonths(3)
        };
    }

    private async Task<Guid?> GetPreviousRegistrationIdAsync(
        Registration registration,
        CancellationToken cancellationToken)
    {
        if (registration.OriginalRegistrationId.HasValue)
        {
            return registration.OriginalRegistrationId.Value;
        }

        return await context.Registrations
            .AsNoTracking()
            .Where(r => r.StudentProfileId == registration.StudentProfileId &&
                        r.Id != registration.Id &&
                        r.Status != RegistrationStatus.Cancelled &&
                        (r.RegistrationDate < registration.RegistrationDate ||
                         r.CreatedAt < registration.CreatedAt))
            .OrderByDescending(r => r.RegistrationDate)
            .ThenByDescending(r => r.CreatedAt)
            .Select(r => (Guid?)r.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<StudyDateRange> GetStudyDateRangeAsync(Guid enrollmentId, CancellationToken cancellationToken)
        => await GetStudyDateRangeAsync(new[] { enrollmentId }, cancellationToken);

    private async Task<StudyDateRange> GetStudyDateRangeAsync(
        IReadOnlyCollection<Guid> enrollmentIds,
        CancellationToken cancellationToken)
    {
        if (enrollmentIds.Count == 0)
        {
            return StudyDateRange.Empty;
        }

        var plannedDates = await context.StudentSessionAssignments
            .AsNoTracking()
            .Where(a => enrollmentIds.Contains(a.ClassEnrollmentId) &&
                        a.Status == StudentSessionAssignmentStatus.Assigned)
            .OrderBy(a => a.Session.PlannedDatetime)
            .Select(a => a.Session.PlannedDatetime)
            .ToListAsync(cancellationToken);

        if (plannedDates.Count == 0)
        {
            return StudyDateRange.Empty;
        }

        return new StudyDateRange(
            VietnamTime.ToVietnamDateOnly(plannedDates[0]),
            VietnamTime.ToVietnamDateOnly(plannedDates[^1]),
            plannedDates.Count);
    }

    private async Task<List<AbsenceRow>> GetAbsenceRowsAsync(
        Guid studentProfileId,
        IReadOnlyCollection<Guid> enrollmentIds,
        CancellationToken cancellationToken)
    {
        if (enrollmentIds.Count == 0)
        {
            return new List<AbsenceRow>();
        }

        var rows = await context.Attendances
            .AsNoTracking()
            .Where(a => a.StudentProfileId == studentProfileId &&
                        a.AttendanceStatus == AttendanceStatus.Absent &&
                        context.StudentSessionAssignments.Any(sa =>
                            sa.StudentProfileId == studentProfileId &&
                            sa.SessionId == a.SessionId &&
                            enrollmentIds.Contains(sa.ClassEnrollmentId)))
            .OrderBy(a => a.Session.PlannedDatetime)
            .Select(a => new
            {
                a.AbsenceType,
                a.Session.PlannedDatetime
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(a => new AbsenceRow(
                VietnamTime.ToVietnamDateOnly(a.PlannedDatetime),
                a.AbsenceType))
            .ToList();
    }

    private async Task<List<MakeupRow>> GetMakeupRowsAsync(
        Guid studentProfileId,
        IReadOnlyCollection<Guid> enrollmentIds,
        CancellationToken cancellationToken)
    {
        if (enrollmentIds.Count == 0)
        {
            return new List<MakeupRow>();
        }

        var rows = await context.MakeupAllocations
            .AsNoTracking()
            .Where(m => m.Status != MakeupAllocationStatus.Cancelled &&
                        m.MakeupCredit.StudentProfileId == studentProfileId &&
                        context.StudentSessionAssignments.Any(sa =>
                            sa.StudentProfileId == studentProfileId &&
                            sa.SessionId == m.MakeupCredit.SourceSessionId &&
                            enrollmentIds.Contains(sa.ClassEnrollmentId)))
            .OrderBy(m => m.TargetSession.PlannedDatetime)
            .Select(m => new
            {
                m.TargetSession.PlannedDatetime
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(m => new MakeupRow(VietnamTime.ToVietnamDateOnly(m.PlannedDatetime)))
            .ToList();
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
            FormType = ToApiFormType(formType),
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
            Currency = tuitionPlan.Currency
        };
    }

    private static string BuildCourseDurationText(
        TuitionPlan tuitionPlan,
        DateOnly? firstStudyDate,
        DateOnly? expectedEndDate)
    {
        var duration = tuitionPlan.TotalSessions > 0
            ? $"{tuitionPlan.TotalSessions} buổi"
            : tuitionPlan.Name;

        if (firstStudyDate.HasValue && expectedEndDate.HasValue)
        {
            return $"{duration} ({firstStudyDate:dd/MM/yyyy} - {expectedEndDate:dd/MM/yyyy})";
        }

        return duration;
    }

    private static string? FormatDateList(IEnumerable<DateOnly> dates)
    {
        var values = dates
            .Distinct()
            .OrderBy(date => date)
            .Select(date => date.ToString("dd/MM/yyyy"))
            .ToList();

        return values.Count == 0 ? null : string.Join(", ", values);
    }

    private static DateOnly? MaxDate(DateOnly? first, DateOnly? second)
    {
        if (!first.HasValue)
        {
            return second;
        }

        if (!second.HasValue)
        {
            return first;
        }

        return first.Value > second.Value ? first : second;
    }

    private static DateOnly? MaxDate(IEnumerable<DateOnly> dates)
    {
        var values = dates.ToList();
        return values.Count == 0 ? null : values.Max();
    }

    private static string ToApiFormType(EnrollmentConfirmationPdfFormType formType)
        => formType == EnrollmentConfirmationPdfFormType.ContinuingStudent
            ? "continuingStudent"
            : "newStudent";

    private static string? FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private sealed record ParentContactDto(string? Name, string? PhoneNumber);
    private sealed record FormTypeResolution(EnrollmentConfirmationPdfFormType? FormType, Error? Error);
    private sealed record StudyDateRange(DateOnly? FirstDate, DateOnly? LastDate, int AssignedSessionCount)
    {
        public static StudyDateRange Empty { get; } = new(null, null, 0);
    }

    private sealed record AbsenceRow(DateOnly Date, AbsenceType? AbsenceType);
    private sealed record MakeupRow(DateOnly TargetDate);
}
