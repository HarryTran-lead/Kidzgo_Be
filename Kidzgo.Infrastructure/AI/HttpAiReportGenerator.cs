using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Domain.Reports;
using Kidzgo.Infrastructure.AI.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace Kidzgo.Infrastructure.AI;

/// <summary>
/// Implementation of IAiReportGenerator that calls A6 API (Python FastAPI)
/// </summary>
public sealed class HttpAiReportGenerator : IAiReportGenerator
{
    private readonly HttpClient _httpClient;
    private readonly IDbContext _context;
    private readonly string _baseUrl;

    public HttpAiReportGenerator(
        HttpClient httpClient,
        IDbContext context,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _context = context;
        _baseUrl = configuration["AiService:BaseUrl"] 
            ?? throw new InvalidOperationException("AiService:BaseUrl not configured");
    }

    public async Task<string> GenerateDraftAsync(
        string dataJson,
        Guid studentProfileId,
        Guid? classId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            throw new ArgumentException("Aggregated data JSON cannot be null or empty", nameof(dataJson));
        }

        // Parse aggregated data JSON (case-insensitive to handle CamelCase from reconstructed data)
        JsonSerializerOptions parseOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        Dictionary<string, JsonElement>? aggregatedData;
        try
        {
            aggregatedData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(dataJson, parseOptions);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Invalid JSON format in aggregated data: {ex.Message}", nameof(dataJson), ex);
        }

        if (aggregatedData is null)
        {
            throw new ArgumentException("Aggregated data JSON is empty", nameof(dataJson));
        }

        // Extract session feedbacks from notes data (case-insensitive)
        var sessionFeedbacks = new List<A6SessionFeedback>();
        
        // Try both lowercase and CamelCase for "notes" key
        if (!aggregatedData.TryGetValue("notes", out var notesElement) &&
            !aggregatedData.TryGetValue("Notes", out notesElement))
        {
            Console.WriteLine("[DEBUG] No 'notes' or 'Notes' key found in aggregated data");
        }
        else
        {
            if (TryGetProperty(notesElement, "sessionReports", "SessionReports", out var sessionReportsElement) &&
                sessionReportsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var report in sessionReportsElement.EnumerateArray())
                {
                    var feedback = TryGetProperty(report, "feedback", "Feedback", out var feedbackElement)
                        ? ReadFlexibleString(feedbackElement)
                        : null;

                    if (!string.IsNullOrWhiteSpace(feedback))
                    {
                        var reportDate =
                            TryGetProperty(report, "reportDate", "ReportDate", out var dateElement)
                                ? ReadFlexibleString(dateElement)
                                : null;

                        reportDate ??=
                            TryGetProperty(report, "sessionDate", "SessionDate", out var sessionDateElement)
                                ? ReadFlexibleString(sessionDateElement)
                                : null;

                        sessionFeedbacks.Add(new A6SessionFeedback
                        {
                            Date = reportDate ?? VietnamTime.TodayDateOnly().ToString("yyyy-MM-dd"),
                            Text = feedback
                        });
                    }
                }
            }
        }

        // Extract attendance data
        A6AttendanceData? attendanceData = null;
        if (aggregatedData.TryGetValue("attendance", out var attElement) ||
            aggregatedData.TryGetValue("Attendance", out attElement))
        {
            attendanceData = new A6AttendanceData
            {
                Total = attElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Present = attElement.TryGetProperty("present", out var p) ? p.GetInt32() : 0,
                Absent = attElement.TryGetProperty("absent", out var a) ? a.GetInt32() : 0,
                Makeup = attElement.TryGetProperty("makeup", out var m) ? m.GetInt32() : 0,
                NotMarked = attElement.TryGetProperty("notMarked", out var nm) ? nm.GetInt32() : 0,
                Percentage = attElement.TryGetProperty("percentage", out var pct) ? pct.GetSingle() : 0
            };
        }

        // Extract homework data
        A6HomeworkData? homeworkData = null;
        if (aggregatedData.TryGetValue("homework", out var hwElement) ||
            aggregatedData.TryGetValue("Homework", out hwElement))
        {
            homeworkData = new A6HomeworkData
            {
                Total = hwElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Completed = hwElement.TryGetProperty("completed", out var c) ? c.GetInt32() : 0,
                Submitted = hwElement.TryGetProperty("submitted", out var s) ? s.GetInt32() : 0,
                Pending = hwElement.TryGetProperty("pending", out var p) ? p.GetInt32() : 0,
                Late = hwElement.TryGetProperty("late", out var l) ? l.GetInt32() : 0,
                Missing = hwElement.TryGetProperty("missing", out var m) ? m.GetInt32() : 0,
                Average = hwElement.TryGetProperty("average", out var avg) ? avg.GetSingle() : 0,
                CompletionRate = hwElement.TryGetProperty("completionRate", out var cr) ? cr.GetSingle() : 0,
                Topics = ReadStringList(hwElement, "topics", "Topics"),
                Skills = ReadStringList(hwElement, "skills", "Skills"),
                GrammarTags = ReadStringList(hwElement, "grammarTags", "GrammarTags"),
                VocabularyTags = ReadStringList(hwElement, "vocabularyTags", "VocabularyTags"),
                SpeakingAssignments = hwElement.TryGetProperty("speakingAssignments", out var sa) ? sa.GetInt32() : 0,
                AiSupportedAssignments = hwElement.TryGetProperty("aiSupportedAssignments", out var asa) ? asa.GetInt32() : 0
            };
        }

        // Extract test data
        A6TestData? testData = null;
        if (aggregatedData.TryGetValue("test", out var testElement) ||
            aggregatedData.TryGetValue("Test", out testElement))
        {
            testData = new A6TestData
            {
                Total = testElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Tests = new List<A6TestResult>()
            };

            if (TryGetProperty(testElement, "tests", "Tests", out var testsElement) &&
                testsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var test in testsElement.EnumerateArray())
                {
                    testData.Tests.Add(new A6TestResult
                    {
                        ExamId = test.TryGetProperty("examId", out var eid) ? ReadFlexibleString(eid) ?? "" : "",
                        Type = test.TryGetProperty("type", out var typ) ? ReadFlexibleString(typ) ?? "" : "",
                        Score = test.TryGetProperty("score", out var sc) ? sc.GetSingle() : 0,
                        MaxScore = test.TryGetProperty("maxScore", out var ms) ? ms.GetSingle() : 0,
                        Date = test.TryGetProperty("date", out var dt) ? ReadFlexibleString(dt) ?? "" : "",
                        Comment = test.TryGetProperty("comment", out var cm) ? ReadFlexibleString(cm) : null
                    });
                }
            }
        }

        // Extract mission data
        A6MissionData? missionData = null;
        if (aggregatedData.TryGetValue("mission", out var missionElement) ||
            aggregatedData.TryGetValue("Mission", out missionElement))
        {
            missionData = new A6MissionData
            {
                Completed = missionElement.TryGetProperty("completed", out var c) ? c.GetInt32() : 0,
                Total = missionElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                InProgress = missionElement.TryGetProperty("inProgress", out var ip) ? ip.GetInt32() : 0,
                Stars = missionElement.TryGetProperty("stars", out var s) ? s.GetInt32() : 0,
                Xp = missionElement.TryGetProperty("xp", out var x) ? x.GetInt32() : 0,
                CurrentLevel = missionElement.TryGetProperty("currentLevel", out var cl) ? ReadFlexibleString(cl) ?? "0" : "0",
                CurrentXp = missionElement.TryGetProperty("currentXp", out var cxp) ? cxp.GetInt32() : 0
            };
        }

        // Extract topics data
        A6TopicsData? topicsData = null;
        if (aggregatedData.TryGetValue("topics", out var topicsElement) ||
            aggregatedData.TryGetValue("Topics", out topicsElement))
        {
            topicsData = new A6TopicsData
            {
                Total = topicsElement.TryGetProperty("total", out var t) ? t.GetInt32() : 0,
                Topics = new List<string>(),
                LessonContents = new List<string>()
            };

            if (TryGetProperty(topicsElement, "topics", "Topics", out var topicsList))
            {
                topicsData.Topics = ReadJsonStringList(topicsList);
            }

            if (TryGetProperty(topicsElement, "lessonContents", "LessonContents", out var contentsList))
            {
                topicsData.LessonContents = ReadJsonStringList(contentsList);
            }
        }

        // Get student profile to extract student info
        var studentProfile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentProfileId, cancellationToken);

        if (studentProfile is null)
        {
            throw new InvalidOperationException($"Student profile with ID {studentProfileId} not found");
        }

        var studentId = studentProfile.Id.ToString();
        var studentName = studentProfile.DisplayName ?? "Unknown Student";
        
        Domain.Classes.Class? reportClass = null;

        if (classId.HasValue)
        {
            reportClass = await _context.Classes
                .Include(c => c.Program)
                .FirstOrDefaultAsync(c => c.Id == classId.Value, cancellationToken);
        }

        if (reportClass is null)
        {
            reportClass = await _context.ClassEnrollments
                .Include(e => e.Class)
                    .ThenInclude(c => c.Program)
                .Where(e => e.StudentProfileId == studentProfileId &&
                           e.Status == Domain.Classes.EnrollmentStatus.Active)
                .OrderByDescending(e => e.EnrollDate)
                .Select(e => e.Class)
                .FirstOrDefaultAsync(cancellationToken);
        }

        var programId = reportClass?.ProgramId;
        var programName = reportClass?.Program?.Name;
        var className = reportClass?.Title;

        // Get recent reports (3 months before current month)
        var recentReports = await GetRecentReportsAsync(
            studentProfileId,
            programId,
            month,
            year,
            cancellationToken);

        // Calculate date range
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Build request
        var request = new A6MonthlyReportRequest
        {
            Student = new A6StudentInfo
            {
                StudentId = studentId,
                Name = studentName,
                Program = programName,
                ClassName = className
            },
            Range = new A6ReportRange
            {
                FromDate = startDate.ToString("yyyy-MM-dd"),
                ToDate = endDate.ToString("yyyy-MM-dd")
            },
            Attendance = attendanceData,
            Homework = homeworkData,
            Test = testData,
            Mission = missionData,
            Topics = topicsData,
            SessionFeedbacks = sessionFeedbacks,
            RecentReports = recentReports,
            Language = "vi"
        };

        

        // Call A6 API
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"{_baseUrl}/a6/generate-monthly-report",
                request,
                jsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"A6 API returned {response.StatusCode}: {errorContent}");
            }

            var result = await response.Content.ReadFromJsonAsync<A6MonthlyReportResponse>(
                jsonOptions,
                cancellationToken: cancellationToken);

            if (result is null)
            {
                throw new InvalidOperationException("A6 API returned null response");
            }

            // Serialize the full response to JSON string for storage (keep snake_case for consistency)
            return JsonSerializer.Serialize(result, jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Failed to call A6 API: {ex.Message}. Make sure AI-KidzGo service is running at {_baseUrl}",
                ex);
        }
    }

    private async Task<List<A6RecentMonthlyReport>> GetRecentReportsAsync(
        Guid studentProfileId,
        Guid? programId,
        int currentMonth,
        int currentYear,
        CancellationToken cancellationToken)
    {
        if (studentProfileId == Guid.Empty)
        {
            return new List<A6RecentMonthlyReport>();
        }

        var recentReports = new List<A6RecentMonthlyReport>();

        // Get reports from 3 months before current month
        for (int i = 1; i <= 3; i++)
        {
            var targetMonth = currentMonth - i;
            var targetYear = currentYear;

            if (targetMonth <= 0)
            {
                targetMonth += 12;
                targetYear--;
            }

            var reportsQuery = _context.StudentMonthlyReports
                .Include(r => r.Class)
                .Where(r => r.StudentProfileId == studentProfileId &&
                           r.Month == targetMonth &&
                           r.Year == targetYear &&
                           r.Status == ReportStatus.Published &&
                           r.FinalContent != null);

            if (programId.HasValue)
            {
                reportsQuery = reportsQuery
                    .Where(r => r.ClassId.HasValue && r.Class != null && r.Class.ProgramId == programId.Value);
            }

            var report = await reportsQuery
                .OrderByDescending(r => r.PublishedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (report?.FinalContent != null)
            {
                try
                {
                    var recentReport = new A6RecentMonthlyReport
                    {
                        Month = $"{targetYear}-{targetMonth:D2}"
                    };

                    using var doc = JsonDocument.Parse(report.FinalContent);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.String)
                    {
                        recentReport.Overview = root.GetString();
                    }
                    else if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (TryGetProperty(root, "draft_text", "draftText", out var draftText))
                        {
                            recentReport.Overview = ReadFlexibleString(draftText);
                        }

                        if (TryGetProperty(root, "sections", "Sections", out var sections) &&
                            sections.ValueKind == JsonValueKind.Object)
                        {
                            if (TryGetProperty(sections, "overview", "Overview", out var overview) ||
                                TryGetProperty(sections, "study_attitude", "StudyAttitude", out overview))
                            {
                                recentReport.Overview = ReadFlexibleString(overview) ?? recentReport.Overview;
                            }

                            if (TryGetProperty(sections, "strengths", "Strengths", out var strengths))
                            {
                                recentReport.Strengths = ReadJsonStringList(strengths);
                            }

                            if (TryGetProperty(sections, "improvements", "Improvements", out var improvements))
                            {
                                recentReport.Improvements = ReadJsonStringList(improvements);
                            }

                            if (TryGetProperty(sections, "highlights", "Highlights", out var highlights))
                            {
                                recentReport.Highlights = ReadJsonStringList(highlights);
                            }

                            if (TryGetProperty(sections, "goals_next_month", "GoalsNextMonth", out var goals) ||
                                TryGetProperty(sections, "goalsNextMonth", "GoalsNextMonth", out goals))
                            {
                                recentReport.GoalsNextMonth = ReadJsonStringList(goals);
                            }
                        }
                    }

                    if (HasRecentReportContent(recentReport))
                    {
                        recentReports.Add(recentReport);
                    }
                }
                catch (JsonException)
                {
                    // Skip invalid JSON
                    continue;
                }
                catch (InvalidOperationException)
                {
                    // Skip legacy content that does not match the expected JSON shape.
                    continue;
                }
            }
        }

        return recentReports;
    }

    private static bool TryGetProperty(JsonElement element, string primaryProperty, string fallbackProperty, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            (element.TryGetProperty(primaryProperty, out value) ||
             element.TryGetProperty(fallbackProperty, out value)))
        {
            return true;
        }

        value = default;
        return false;
    }

    private static List<string> ReadStringList(JsonElement element, string primaryProperty, string fallbackProperty)
    {
        if (TryGetProperty(element, primaryProperty, fallbackProperty, out var valuesElement))
        {
            return ReadJsonStringList(valuesElement);
        }

        return new List<string>();
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
            .Where(static item => !string.IsNullOrWhiteSpace(item))
            .Select(static item => item!)
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

    private static bool HasRecentReportContent(A6RecentMonthlyReport report)
    {
        return !string.IsNullOrWhiteSpace(report.Overview) ||
               report.Strengths.Count > 0 ||
               report.Improvements.Count > 0 ||
               report.Highlights.Count > 0 ||
               report.GoalsNextMonth.Count > 0;
    }
}
