using System.Globalization;
using System.Net;
using System.Text;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Domain.Registrations;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Kidzgo.Infrastructure.Reports;

public sealed class EnrollmentConfirmationPdfGenerator(
    IFileStorageService fileStorage,
    ILogger<EnrollmentConfirmationPdfGenerator> logger
) : IEnrollmentConfirmationPdfGenerator
{
    private static readonly CultureInfo ViCulture = new("vi-VN");
    private static readonly SemaphoreSlim DownloadLock = new(1, 1);
    private static bool browserReady;

    public async Task<string> GeneratePdfAsync(
        EnrollmentConfirmationPdfDocument document,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var html = BuildHtml(document);
            var pdfBytes = await GeneratePdfBytesAsync(html, cancellationToken);
            var fileName = BuildFileName(document);

            using var stream = new MemoryStream(pdfBytes);
            return await fileStorage.UploadFileAsync(
                stream,
                fileName,
                "enrollment-confirmations",
                "document",
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to generate enrollment confirmation PDF for registration {RegistrationId}, enrollment {EnrollmentId}",
                document.RegistrationId,
                document.EnrollmentId);
            throw new InvalidOperationException($"Failed to generate enrollment confirmation PDF: {ex.Message}", ex);
        }
    }

    private static string BuildHtml(EnrollmentConfirmationPdfDocument document)
        => document.FormType == EnrollmentConfirmationPdfFormType.ContinuingStudent
            ? BuildContinuingStudentHtml(document)
            : BuildNewStudentHtml(document);

    private static string BuildNewStudentHtml(EnrollmentConfirmationPdfDocument document)
    {
        var studyDaySummary = DisplayText(document.StudyDaySummary, "Theo lịch lớp");

        return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <title>Phiếu thu học phí</title>
    {{CommonStyle()}}
</head>
<body>
    <main class="page">
        {{Header(document)}}

        <h1>PHIẾU THU HỌC PHÍ</h1>

        <section class="section">
            <div class="section-title">1. Thông tin</div>
            <table>
                <tr><th>Họ tên học viên</th><td>{{H(document.StudentName)}}</td></tr>
                <tr><th>SĐT phụ huynh</th><td>{{H(DisplayText(document.ParentPhoneNumber))}}</td></tr>
                <tr><th>Phụ huynh</th><td>{{H(DisplayText(document.ParentName))}}</td></tr>
                <tr><th>Ngày sinh</th><td>{{H(DisplayDate(document.StudentDateOfBirth))}}</td></tr>
                <tr><th>Lớp đăng ký</th><td>{{H($"{document.ClassCode} - {document.ClassTitle}")}}</td></tr>
                <tr><th>Trình độ / khóa học</th><td>{{H($"{document.ProgramName} ({document.ProgramCode})")}}</td></tr>
            </table>
        </section>

        <section class="section">
            <div class="section-title">2. Thông tin khóa học</div>
            <table>
                <tr><th>Thời lượng khóa</th><td>{{H(document.CourseDurationText)}}</td></tr>
                <tr><th>Lịch học</th><td>{{H(studyDaySummary)}}</td></tr>
                <tr><th>Ngày bắt đầu</th><td>{{H(DisplayDate(document.FirstStudyDate ?? document.EnrollDate))}}</td></tr>
                <tr><th>Ngày kết thúc dự kiến</th><td>{{H(DisplayDate(document.ExpectedEndDate))}}</td></tr>
            </table>
        </section>

        <section class="section">
            <div class="section-title">3. Học phí</div>
            <table>
                <tr><th>Gói học</th><td>{{H(document.TuitionPlanName)}}</td></tr>
                <tr><th>Học phí khóa</th><td class="amount">{{H(FormatAmount(document.TuitionAmount, document.Currency))}}</td></tr>
                <tr><th>Ưu đãi (nếu có)</th><td>.....</td></tr>
                <tr><th>Tài liệu</th><td>.....</td></tr>
                <tr><th>Tổng thanh toán</th><td class="amount">.....</td></tr>
            </table>
        </section>

        <section class="section">
            <div class="section-title">4. Chính sách áp dụng</div>
            <ul class="policy-list">
                {{RenderPolicyItems(document.NewStudentPolicyLines)}}
            </ul>
        </section>

        {{PaymentSection(document)}}
        {{Signature(document)}}
        {{Footer(document)}}
    </main>
</body>
</html>
""";
    }

    private static string BuildContinuingStudentHtml(EnrollmentConfirmationPdfDocument document)
    {
        var reconciliation = document.Reconciliation ?? new EnrollmentReconciliationPdfSection
        {
            Note = "Chưa có dữ liệu đối soát khóa trước."
        };
        var previousClass = string.IsNullOrWhiteSpace(reconciliation.PreviousClassCode)
            ? DisplayText(reconciliation.PreviousClassTitle)
            : $"{reconciliation.PreviousClassCode} - {DisplayText(reconciliation.PreviousClassTitle)}";
        var studyDaySummary = DisplayText(document.StudyDaySummary, "Theo lịch lớp");
        var reservation = reconciliation.Reservation;
        var reservedSessionCount = reservation is null
            ? "....."
            : $"{DisplayNumber(reservation.ReservedSessionCount)} buổi";
        var reservationFrom = reservation is not null && reservation.PauseFrom.HasValue
            ? DisplayDate(reservation.PauseFrom.Value)
            : ".....";
        var reservationTo = reservation is not null && reservation.PauseTo.HasValue
            ? DisplayDate(reservation.PauseTo.Value)
            : ".....";
        var reservationExpirySuffix = reservation is not null && reservation.ReservationExpiresOn.HasValue
            ? $", đến {DisplayDate(reservation.ReservationExpiresOn.Value)}"
            : string.Empty;
        var reservationTerm = reservation is not null && reservation.PauseFrom.HasValue
            ? $"03 tháng (tính từ ngày {DisplayDate(reservation.PauseFrom.Value)}{reservationExpirySuffix})"
            : "03 tháng (tính từ ngày ..../..../....)";

        return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8">
    <title>Phiếu đối soát và thu học phí</title>
    {{CommonStyle()}}
</head>
<body>
    <main class="page">
        {{Header(document)}}

        <h1>PHIẾU ĐỐI SOÁT VÀ THU HỌC PHÍ</h1>

        <section class="section">
            <table>
                <tr><th>Họ tên học viên</th><td>{{H(document.StudentName)}}</td></tr>
                <tr><th>Lớp đăng ký tiếp</th><td>{{H($"{document.ClassCode} - {document.ClassTitle}")}}</td></tr>
            </table>
        </section>

        <section class="section">
            <div class="section-title">Bước 1: Đối soát học phí khóa hiện tại</div>
            <table>
                <tr><th>Lớp / khóa hiện tại</th><td>{{H(previousClass)}}</td></tr>
                <tr><th>Chương trình</th><td>{{H(DisplayText(reconciliation.PreviousProgramName))}}</td></tr>
                <tr><th>Thời gian khóa học</th><td>{{H($"{DisplayDate(reconciliation.CourseStartDate)} - {DisplayDate(reconciliation.CourseEndDate)}")}}</td></tr>
                <tr><th>Tổng số buổi theo khóa</th><td>{{H(DisplayNumber(reconciliation.TotalSessions))}} buổi</td></tr>
                <tr><th>Số buổi nghỉ có phép theo quy định</th><td>{{reconciliation.ExcusedAbsenceCount}} buổi</td></tr>
                <tr><th>Chi tiết ngày nghỉ có phép / Lễ Tết</th><td>{{H(DisplayText(reconciliation.ExcusedAbsenceDetails, "Không có"))}}</td></tr>
                <tr><th>Số buổi nghỉ không phép</th><td>{{reconciliation.UnexcusedAbsenceCount}} buổi</td></tr>
                <tr><th>Chi tiết ngày nghỉ không phép</th><td>{{H(DisplayText(reconciliation.UnexcusedAbsenceDetails, "Không có"))}}</td></tr>
                <tr><th>Số buổi đã được sắp xếp học bù</th><td>{{reconciliation.MakeupScheduledCount}} buổi</td></tr>
                <tr><th>Chi tiết ngày học bù</th><td>{{H(DisplayText(reconciliation.MakeupScheduledDetails, "Không có"))}}</td></tr>
                <tr><th>Kết luận</th><td>Sau khi đối soát và học bù, khóa học kết thúc vào ngày: {{H(DisplayDate(reconciliation.ReconciledEndDate))}}</td></tr>
            </table>
            <div class="note">{{H(DisplayText(reconciliation.Note))}}</div>
        </section>

        <section class="section">
            <div class="section-title">Bước 2: Thu học phí khóa tiếp theo / xử lý phát sinh</div>
            <div class="sub-title">Trường hợp 1: Tiếp tục đăng ký học</div>
            <table>
                <tr><th>Thời lượng khóa</th><td>{{H(document.CourseDurationText)}}</td></tr>
                <tr><th>Lịch học</th><td>{{H(studyDaySummary)}}</td></tr>
                <tr><th>Ngày bắt đầu</th><td>{{H(DisplayDate(document.FirstStudyDate ?? document.EnrollDate))}}</td></tr>
                <tr><th>Ngày kết thúc dự kiến</th><td>{{H(DisplayDate(document.ExpectedEndDate))}}</td></tr>
                <tr><th>Học phí gốc</th><td class="amount">{{H(FormatAmount(document.TuitionAmount, document.Currency))}}</td></tr>
                <tr><th>Ưu đãi (nếu có)</th><td>.....</td></tr>
                <tr><th>Tài liệu</th><td>.....</td></tr>
                <tr><th>Tổng thanh toán</th><td class="amount">.....</td></tr>
                <tr><th>Ghi chú</th><td>Học phí chưa bao gồm tài liệu nếu trung tâm có thu riêng.</td></tr>
            </table>
            <div class="sub-title">Trường hợp 2: Ngừng học và chưa thanh toán các buổi phát sinh</div>
            <table>
                <tr><th>Thời gian phát sinh</th><td>từ ..... đến .....</td></tr>
                <tr><th>Số buổi phát sinh</th><td>..... buổi</td></tr>
                <tr><th>Đơn giá/buổi</th><td>.....</td></tr>
                <tr><th>Chi phí khác (nếu có)</th><td>.....<br><span class="muted">(Ví dụ: tài liệu, lệ phí...)</span></td></tr>
                <tr><th>Tổng cần thanh toán</th><td class="amount">.....</td></tr>
            </table>

            <div class="sub-title">Trường hợp 3: Bảo lưu khóa học</div>
            <table>
                <tr><th>Số buổi bảo lưu</th><td>{{H(reservedSessionCount)}}</td></tr>
                <tr><th>Thời gian bảo lưu</th><td>{{H($"{reservationFrom} - {reservationTo}")}}</td></tr>
                <tr><th>Thời hạn bảo lưu</th><td>{{H(reservationTerm)}}</td></tr>
                <tr>
                    <th>Quy định bảo lưu</th>
                    <td>
                        <ul class="policy-list">
                            {{RenderPolicyItems(document.ReservationPolicyLines)}}
                        </ul>
                    </td>
                </tr>
            </table>
        </section>

        {{PaymentSection(document)}}
        {{Signature(document)}}
        {{Footer(document)}}
    </main>
</body>
</html>
""";
    }

    private static string CommonStyle()
    {
        return """
    <style>
        * { box-sizing: border-box; }
        body {
            margin: 0;
            padding: 26px;
            color: #1f2933;
            font-family: Arial, "Helvetica Neue", sans-serif;
            font-size: 13px;
            line-height: 1.5;
        }
        .page {
            border: 1px solid #e0c7c7;
            padding: 26px;
            min-height: 1040px;
        }
        .header {
            border-bottom: 3px solid #9f1d24;
            padding-bottom: 16px;
            margin-bottom: 20px;
            display: flex;
            justify-content: space-between;
            align-items: flex-start;
            gap: 20px;
        }
        .header-main {
            min-width: 0;
        }
        .brand {
            font-size: 20px;
            font-weight: 700;
            color: #9f1d24;
        }
        .header-logo {
            max-width: 145px;
            max-height: 82px;
            object-fit: contain;
        }
        .branch {
            margin-top: 6px;
            color: #52616f;
        }
        h1 {
            margin: 24px 0 18px;
            text-align: center;
            color: #7f1d1d;
            font-size: 23px;
            letter-spacing: .2px;
        }
        .section {
            margin: 16px 0;
        }
        .section-title {
            margin: 0 0 9px;
            color: #9f1d24;
            font-size: 15px;
            font-weight: 700;
            text-transform: uppercase;
        }
        .sub-title {
            margin: 10px 0 8px;
            font-weight: 700;
            color: #344054;
        }
        table {
            width: 100%;
            border-collapse: collapse;
        }
        th, td {
            border: 1px solid #ead7d7;
            padding: 9px 11px;
            vertical-align: top;
        }
        th {
            width: 34%;
            background: #fff5f5;
            text-align: left;
            color: #3b4a54;
            font-weight: 700;
        }
        .amount {
            font-size: 16px;
            font-weight: 700;
            color: #b91c1c;
        }
        .policy-list {
            margin: 0;
            padding-left: 20px;
        }
        .policy-list li {
            margin: 5px 0;
        }
        .muted {
            color: #65727f;
            font-size: 12px;
        }
        .note,
        .case-note {
            margin-top: 12px;
            padding: 10px 12px;
            border-left: 4px solid #9f1d24;
            background: #fff5f5;
            color: #3b4a54;
        }
        .payment-layout {
            display: grid;
            grid-template-columns: minmax(0, 1fr) 150px;
            gap: 14px;
            align-items: start;
        }
        .payment-layout.no-qr {
            display: block;
        }
        .payment-qr {
            border: 1px solid #ead7d7;
            padding: 10px;
            text-align: center;
            color: #52616f;
            font-size: 11px;
        }
        .payment-qr img {
            width: 128px;
            height: 128px;
            object-fit: contain;
            display: block;
            margin: 0 auto 6px;
        }
        .signature {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 36px;
            margin-top: 46px;
            text-align: center;
        }
        .signature strong {
            display: block;
            margin-bottom: 64px;
        }
        .footer {
            margin-top: 32px;
            border-top: 1px solid #ead7d7;
            padding-top: 10px;
            color: #65727f;
            font-size: 12px;
        }
    </style>
""";
    }

    private static string Header(EnrollmentConfirmationPdfDocument document)
    {
        var branchInfo = DisplayText(document.BranchAddress, "Địa chỉ đang cập nhật");

        var logoHtml = string.IsNullOrWhiteSpace(document.HeaderLogoUrl)
            ? string.Empty
            : $"""<img class="header-logo" src="{H(document.HeaderLogoUrl)}" alt="Logo">""";

        if (!string.IsNullOrWhiteSpace(document.BranchPhoneNumber))
        {
            branchInfo += $" | {document.BranchPhoneNumber}";
        }

        return $$"""
        <section class="header">
            <div class="header-main">
                <div class="brand">{{H(document.BranchName)}}</div>
                <div class="branch">{{H(branchInfo)}}</div>
            </div>
            {{logoHtml}}
        </section>
""";
    }

    private static string PaymentSection(EnrollmentConfirmationPdfDocument document)
    {
        var hasQr = !string.IsNullOrWhiteSpace(document.PaymentQrUrl);
        var layoutClass = hasQr ? "payment-layout" : "payment-layout no-qr";
        var qrHtml = hasQr
            ? $$"""
            <div class="payment-qr">
                <img src="{{H(document.PaymentQrUrl!)}}" alt="Payment QR">
                <div>QR chuyen khoan</div>
            </div>
"""
            : string.Empty;

        return $$"""
        <section class="section">
            <div class="section-title">Thông tin thanh toán</div>
            <div class="{{layoutClass}}">
            <table>
                <tr><th>Hình thức</th><td>{{H(DisplayText(document.PaymentMethod, "Tiền mặt / Chuyển khoản"))}}</td></tr>
                <tr><th>Chủ tài khoản</th><td>{{H(DisplayText(document.PaymentAccountName, "....."))}}</td></tr>
                <tr><th>Số tài khoản</th><td>{{H(DisplayText(document.PaymentAccountNumber, "....."))}}</td></tr>
                <tr><th>Ngân hàng</th><td>{{H(DisplayText(document.PaymentBankName, "....."))}}</td></tr>
                <tr><th>Nội dung chuyển khoản</th><td>{{H(DisplayText(document.PaymentTransferContent, $"{document.StudentName} - {document.ClassCode}"))}}</td></tr>
            </table>
            {{qrHtml}}
            </div>
        </section>
""";
    }

    private static string Signature(EnrollmentConfirmationPdfDocument document)
    {
        return $$"""
        <section class="signature">
            <div>
                <strong>Đại diện trung tâm</strong>
                {{H(document.IssuedByName ?? string.Empty)}}
            </div>
            <div>
                <strong>Phụ huynh học viên</strong>
            </div>
        </section>
""";
    }

    private static string Footer(EnrollmentConfirmationPdfDocument document)
    {
        var issuedAt = VietnamTime.ToVietnamDateTime(document.GeneratedAt).ToString("dd/MM/yyyy HH:mm", ViCulture);

        return $$"""
        <section class="footer">
            Ngày xuất phiếu: {{H(issuedAt)}} | Mã đăng ký: {{H(document.RegistrationId.ToString())}} | Mã enrollment: {{H(document.EnrollmentId.ToString())}}
        </section>
""";
    }

    private static async Task<byte[]> GeneratePdfBytesAsync(string htmlContent, CancellationToken cancellationToken)
    {
        await EnsureBrowserReadyAsync(cancellationToken);

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            MarginOptions = new PuppeteerSharp.Media.MarginOptions
            {
                Top = "20px",
                Right = "20px",
                Bottom = "20px",
                Left = "20px"
            },
            PrintBackground = true
        });
    }

    private static async Task EnsureBrowserReadyAsync(CancellationToken cancellationToken)
    {
        if (browserReady)
        {
            return;
        }

        await DownloadLock.WaitAsync(cancellationToken);
        try
        {
            if (browserReady)
            {
                return;
            }

            try
            {
                using var testBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });
                await testBrowser.CloseAsync();
                browserReady = true;
                return;
            }
            catch
            {
            }

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            browserReady = true;
        }
        finally
        {
            DownloadLock.Release();
        }
    }

    private static string BuildFileName(EnrollmentConfirmationPdfDocument document)
    {
        var formType = document.FormType == EnrollmentConfirmationPdfFormType.ContinuingStudent
            ? "continuing"
            : "new";
        var safeStudentName = Slugify(document.StudentName);
        var date = document.EnrollDate.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        return $"enrollment-confirmation-{formType}-{safeStudentName}-{date}-{document.EnrollmentId:N}.pdf";
    }

    private static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (var character in value.Trim().ToLowerInvariant())
        {
            builder.Append(char.IsLetterOrDigit(character) ? character : '-');
        }

        var slug = builder.ToString().Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "student" : slug;
    }

    private static string DisplayDate(DateOnly value)
        => value.ToString("dd/MM/yyyy", ViCulture);

    private static string DisplayDate(DateOnly? value)
        => value.HasValue ? DisplayDate(value.Value) : "Chưa cập nhật";

    private static string DisplayNumber(int value)
        => value > 0 ? value.ToString("N0", ViCulture) : "0";

    private static string DisplayText(string? value, string fallback = "Chưa cập nhật")
        => string.IsNullOrWhiteSpace(value) ? fallback : value;

    private static string FormatAmount(decimal amount, string currency)
        => $"{amount.ToString("N0", ViCulture)} {currency}";

    private static string RenderPolicyItems(IEnumerable<string> lines)
    {
        var items = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => $"<li>{H(line)}</li>")
            .ToList();

        return items.Count == 0 ? "<li>Đang cập nhật.</li>" : string.Join(Environment.NewLine, items);
    }

    private static string H(string value)
        => WebUtility.HtmlEncode(value);
}
