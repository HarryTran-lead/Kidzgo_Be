using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using PuppeteerSharp;
using System.Collections.Concurrent;

namespace Kidzgo.Infrastructure.Reports;

/// <summary>
/// Implementation of IPdfReportGenerator using QuestPDF
/// Converts HTML content to PDF and uploads to Cloudinary
/// </summary>
public sealed class QuestPdfReportGenerator : IPdfReportGenerator
{
    private readonly IFileStorageService _fileStorage;
    private readonly ILogger<QuestPdfReportGenerator> _logger;
    private static readonly SemaphoreSlim _downloadLock = new(1, 1);
    private static bool _browserDownloaded = false;

    public QuestPdfReportGenerator(
        IFileStorageService fileStorage,
        ILogger<QuestPdfReportGenerator> logger)
    {
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<string> GeneratePdfAsync(
        string htmlContent,
        string studentName,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Parse HTML content (could be JSON with sections or plain HTML)
            var pdfContent = ConvertToPdfHtml(htmlContent, studentName, month, year);

            // Generate PDF bytes from HTML
            var pdfBytes = await GeneratePdfBytesAsync(pdfContent, cancellationToken);

            // Upload to Cloudinary
            var fileName = $"monthly-report-{studentName.Replace(" ", "-")}-{year}-{month:D2}.pdf";
            using var pdfStream = new MemoryStream(pdfBytes);
            
            var pdfUrl = await _fileStorage.UploadFileAsync(
                pdfStream,
                fileName,
                "monthly-reports",
                "raw", // PDF is raw file type
                cancellationToken);

            _logger.LogInformation("PDF generated successfully for {StudentName}, Month: {Month}, Year: {Year}, URL: {Url}",
                studentName, month, year, pdfUrl);

            return pdfUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate PDF for {StudentName}, Month: {Month}, Year: {Year}",
                studentName, month, year);
            throw new InvalidOperationException($"Failed to generate PDF: {ex.Message}", ex);
        }
    }

    private string ConvertToPdfHtml(string content, string studentName, int month, int year)
    {
        // Try to parse as JSON (from AI response) or use as plain HTML
        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.String)
            {
                return BuildHtmlFromDraftText(root.GetString() ?? "", studentName, month, year);
            }

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("draft_text", out var draftText))
            {
                return BuildHtmlFromDraftText(ReadFlexibleString(draftText) ?? "", studentName, month, year);
            }

            if (root.ValueKind == JsonValueKind.Object &&
                root.TryGetProperty("sections", out var sections) &&
                sections.ValueKind == JsonValueKind.Object)
            {
                return BuildHtmlFromSections(sections, studentName, month, year);
            }
        }
        catch (JsonException)
        {
            // Not JSON, treat as plain HTML or text
        }
        catch (InvalidOperationException)
        {
            // Legacy JSON content may be a string root or another non-object shape.
        }

        // If content is plain text or HTML, wrap it
        return BuildHtmlFromPlainText(content, studentName, month, year);
    }

    private string BuildHtmlFromDraftText(string draftText, string studentName, int month, int year)
    {
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy", 
            new System.Globalization.CultureInfo("vi-VN"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Báo cáo tháng - {studentName}</title>
    <style>
        body {{ font-family: 'Arial', sans-serif; margin: 40px; line-height: 1.6; }}
        h1 {{ color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }}
        h2 {{ color: #34495e; margin-top: 30px; }}
        .section {{ margin: 20px 0; }}
        ul {{ margin: 10px 0; padding-left: 20px; }}
        li {{ margin: 5px 0; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #7f8c8d; font-size: 12px; }}
        pre {{ white-space: pre-wrap; font-family: inherit; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Báo Cáo Tháng {monthName}</h1>
        <h2>Học sinh: {studentName}</h2>
    </div>
    <div class=""section"">
        <pre>{System.Net.WebUtility.HtmlEncode(draftText)}</pre>
    </div>
    <div class=""footer"">
        <p>Rex English Center - KidzGo</p>
        <p>Được tạo vào: {VietnamTime.NowInVietnam():dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
    }

    private string BuildHtmlFromSections(JsonElement sections, string studentName, int month, int year)
    {
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy",
            new System.Globalization.CultureInfo("vi-VN"));

        var overview =
            sections.TryGetProperty("overview", out var ov)
                ? ReadFlexibleString(ov)
                : sections.TryGetProperty("study_attitude", out var studyAttitude)
                    ? ReadFlexibleString(studyAttitude)
                    : "";
        var strengths = sections.TryGetProperty("strengths", out var st)
            ? ReadJsonStringList(st)
            : new List<string>();
        var improvements = sections.TryGetProperty("improvements", out var imp)
            ? ReadJsonStringList(imp)
            : new List<string>();
        var highlights = sections.TryGetProperty("highlights", out var hl)
            ? ReadJsonStringList(hl)
            : sections.TryGetProperty("progress_topics", out var progressTopics)
                ? ReadJsonStringList(progressTopics)
                : new List<string>();
        var goals = sections.TryGetProperty("goals_next_month", out var gl)
            ? ReadJsonStringList(gl)
            : sections.TryGetProperty("parent_support", out var parentSupport)
                ? ReadJsonStringList(parentSupport)
                : new List<string>();

        var strengthsHtml = string.Join("", strengths.Select(s => $"<li>{System.Net.WebUtility.HtmlEncode(s)}</li>"));
        var improvementsHtml = string.Join("", improvements.Select(s => $"<li>{System.Net.WebUtility.HtmlEncode(s)}</li>"));
        var highlightsHtml = string.Join("", highlights.Select(s => $"<li>{System.Net.WebUtility.HtmlEncode(s)}</li>"));
        var goalsHtml = string.Join("", goals.Select(s => $"<li>{System.Net.WebUtility.HtmlEncode(s)}</li>"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Báo cáo tháng - {studentName}</title>
    <style>
        body {{ font-family: 'Arial', sans-serif; margin: 40px; line-height: 1.6; }}
        h1 {{ color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }}
        h2 {{ color: #34495e; margin-top: 30px; }}
        .section {{ margin: 20px 0; }}
        ul {{ margin: 10px 0; padding-left: 20px; }}
        li {{ margin: 5px 0; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #7f8c8d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Báo Cáo Tháng {monthName}</h1>
        <h2>Học sinh: {studentName}</h2>
    </div>
    <div class=""section"">
        <h2>1. Tổng quan</h2>
        <p>{System.Net.WebUtility.HtmlEncode(overview)}</p>
    </div>
    <div class=""section"">
        <h2>2. Điểm mạnh</h2>
        <ul>{strengthsHtml}</ul>
    </div>
    <div class=""section"">
        <h2>3. Cần cải thiện</h2>
        <ul>{improvementsHtml}</ul>
    </div>
    <div class=""section"">
        <h2>4. Nhận xét tiêu biểu</h2>
        <ul>{highlightsHtml}</ul>
    </div>
    <div class=""section"">
        <h2>5. Mục tiêu tháng tới</h2>
        <ul>{goalsHtml}</ul>
    </div>
    <div class=""footer"">
        <p>Rex English Center - KidzGo</p>
        <p>Được tạo vào: {VietnamTime.NowInVietnam():dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
    }

    private static List<string> ReadJsonStringList(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.String)
        {
            var singleValue = element.GetString();
            return string.IsNullOrWhiteSpace(singleValue)
                ? new List<string>()
                : new List<string> { singleValue };
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            return new List<string>();
        }

        return element.EnumerateArray()
            .Select(ReadFlexibleString)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value!)
            .ToList();
    }

    private static string? ReadFlexibleString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Object => element.GetRawText(),
            JsonValueKind.Array => element.GetRawText(),
            _ => element.ToString()
        };
    }

    private string BuildHtmlFromPlainText(string content, string studentName, int month, int year)
    {
        var monthName = new DateTime(year, month, 1).ToString("MMMM yyyy",
            new System.Globalization.CultureInfo("vi-VN"));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <title>Báo cáo tháng - {studentName}</title>
    <style>
        body {{ font-family: 'Arial', sans-serif; margin: 40px; line-height: 1.6; }}
        h1 {{ color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .footer {{ margin-top: 40px; text-align: center; color: #7f8c8d; font-size: 12px; }}
        pre {{ white-space: pre-wrap; font-family: inherit; }}
    </style>
</head>
<body>
    <div class=""header"">
        <h1>Báo Cáo Tháng {monthName}</h1>
        <h2>Học sinh: {studentName}</h2>
    </div>
    <div>
        <pre>{System.Net.WebUtility.HtmlEncode(content)}</pre>
    </div>
    <div class=""footer"">
        <p>Rex English Center - KidzGo</p>
        <p>Được tạo vào: {VietnamTime.NowInVietnam():dd/MM/yyyy HH:mm}</p>
    </div>
</body>
</html>";
    }

    private async Task<byte[]> GeneratePdfBytesAsync(string htmlContent, CancellationToken cancellationToken)
    {
        try
        {
            // Download Chromium if not already downloaded (with lock to prevent concurrent downloads)
            await EnsureBrowserDownloadedAsync(cancellationToken);

            // Launch browser
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            // Create new page
            using var page = await browser.NewPageAsync();

            // Set content and wait for it to load
            await page.SetContentAsync(htmlContent, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            // Generate PDF
            var pdfBytes = await page.PdfDataAsync(new PdfOptions
            {
                Format = PuppeteerSharp.Media.PaperFormat.A4,
                MarginOptions = new PuppeteerSharp.Media.MarginOptions
                {
                    Top = "20px",
                    Right = "20px",
                    Bottom = "20px",
                    Left = "20px"
                },
                PrintBackground = true,
                DisplayHeaderFooter = true,
                HeaderTemplate = "<div style='font-size: 9px; text-align: right; width: 100%; padding-right: 20px;'><span class='pageNumber'></span> / <span class='totalPages'></span></div>",
                FooterTemplate = "<div style='font-size: 9px; text-align: center; width: 100%;'><span>Rex English Center - KidzGo</span></div>"
            });

            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert HTML to PDF bytes using PuppeteerSharp");
            throw new InvalidOperationException($"Failed to generate PDF bytes: {ex.Message}", ex);
        }
    }

    private async Task EnsureBrowserDownloadedAsync(CancellationToken cancellationToken)
    {
        // Fast path: if already downloaded, skip
        if (_browserDownloaded)
        {
            return;
        }

        // Use semaphore to ensure only one thread downloads at a time
        await _downloadLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_browserDownloaded)
            {
                return;
            }

            var browserFetcher = new BrowserFetcher();
            
            // Try to launch browser first - if it works, browser is already downloaded
            try
            {
                using var testBrowser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });
                await testBrowser.CloseAsync();
                _browserDownloaded = true;
                _logger.LogInformation("Browser already available");
                return;
            }
            catch
            {
                // Browser not available, need to download
            }

            // Download browser with retry logic
            _logger.LogInformation("Downloading browser for PDF generation...");
            var maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    await browserFetcher.DownloadAsync();
                    _browserDownloaded = true;
                    _logger.LogInformation("Browser downloaded successfully");
                    return;
                }
                catch (IOException ex) when (ex.Message.Contains("being used by another process") && attempt < maxRetries)
                {
                    _logger.LogWarning("Browser download failed (file locked), retrying in {Delay}ms (attempt {Attempt}/{MaxRetries})...", 
                        1000 * attempt, attempt, maxRetries);
                    await Task.Delay(1000 * attempt, cancellationToken);
                }
            }

            throw new InvalidOperationException("Failed to download browser after multiple retries");
        }
        finally
        {
            _downloadLock.Release();
        }
    }
}

