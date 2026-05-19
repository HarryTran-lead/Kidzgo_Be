using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Domain.Exams;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Kidzgo.Application.Shared;

namespace Kidzgo.Infrastructure.Services;

/// <summary>
/// UC-175: Gom dữ liệu cho Monthly Report
/// Implementation of IMonthlyReportDataAggregator
/// </summary>
public sealed class MonthlyReportDataAggregator(
    IDbContext context
) : IMonthlyReportDataAggregator
{
    public async Task<string> AggregateDataAsync(
        Guid studentProfileId,
        Guid? classId,
        int month,
        int year,
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Aggregate all data sequentially (DbContext is not thread-safe)
        var attendance = await AggregateAttendanceDataAsync(studentProfileId, classId, startDate, endDate, cancellationToken);
        var homework = await AggregateHomeworkDataAsync(studentProfileId, classId, startDate, endDate, cancellationToken);
        var test = await AggregateTestDataAsync(studentProfileId, classId, startDate, endDate, cancellationToken);
        var mission = await AggregateMissionDataAsync(studentProfileId, classId, startDate, endDate, cancellationToken);
        var notes = await AggregateSessionReportsDataAsync(studentProfileId, classId, startDate, endDate, cancellationToken);
        var topics = await AggregateTopicsCoveredAsync(studentProfileId, classId, startDate, endDate, cancellationToken);

        var aggregatedData = new
        {
            attendance,
            homework,
            test,
            mission,
            notes,
            topics
        };

        return JsonSerializer.Serialize(aggregatedData, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private async Task<object> AggregateAttendanceDataAsync(
        Guid studentProfileId,
        Guid? classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var attendancesQuery = context.Attendances
            .Include(a => a.Session)
            .Where(a => a.StudentProfileId == studentProfileId &&
                       a.Session.PlannedDatetime >= startDate &&
                       a.Session.PlannedDatetime <= endDate);

        if (classId.HasValue)
        {
            attendancesQuery = attendancesQuery.Where(a => a.Session.ClassId == classId.Value);
        }

        var attendances = await attendancesQuery
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var total = attendances.Count;
        var present = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Present);
        var absent = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Absent);
        var makeup = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.Makeup);
        var notMarked = attendances.Count(a => a.AttendanceStatus == AttendanceStatus.NotMarked);
        var percentage = total > 0 ? (decimal)(present + makeup) / total * 100 : 0;

        return new
        {
            total,
            present,
            absent,
            makeup,
            notMarked,
            percentage = Math.Round(percentage, 2)
        };
    }

    private async Task<object> AggregateHomeworkDataAsync(
        Guid studentProfileId,
        Guid? classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var homeworkQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
            .Where(hs => hs.StudentProfileId == studentProfileId &&
                        hs.Assignment.CreatedAt >= startDate &&
                        hs.Assignment.CreatedAt <= endDate);

        if (classId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Assignment.ClassId == classId.Value);
        }

        var homeworkSubmissions = await homeworkQuery
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var total = homeworkSubmissions.Count;
        var completed = homeworkSubmissions.Count(hs => hs.Status == HomeworkStatus.Graded);
        var submitted = homeworkSubmissions.Count(hs => hs.Status == HomeworkStatus.Submitted);
        var pending = homeworkSubmissions.Count(hs => hs.Status == HomeworkStatus.Assigned);
        var late = homeworkSubmissions.Count(hs => hs.Status == HomeworkStatus.Late);
        var missing = homeworkSubmissions.Count(hs => hs.Status == HomeworkStatus.Missing);

        var gradedSubmissions = homeworkSubmissions
            .Where(hs => hs.Score.HasValue)
            .ToList();

        var average = gradedSubmissions.Any()
            ? Math.Round((decimal)gradedSubmissions.Average(hs => hs.Score!.Value), 2)
            : 0;

        var completionRate = total > 0 ? Math.Round((decimal)(completed + submitted) / total * 100, 2) : 0;

        var topics = homeworkSubmissions
            .Select(hs => hs.Assignment.Topic)
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Select(static value => value!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value)
            .ToList();

        var skills = homeworkSubmissions
            .SelectMany(hs => ParseFlexibleTags(hs.Assignment.Skills))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value)
            .ToList();

        var grammarTags = homeworkSubmissions
            .SelectMany(hs => ParseFlexibleTags(hs.Assignment.GrammarTags))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value)
            .ToList();

        var vocabularyTags = homeworkSubmissions
            .SelectMany(hs => ParseFlexibleTags(hs.Assignment.VocabularyTags))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(static value => value)
            .ToList();

        var speakingAssignments = homeworkSubmissions.Count(hs => !string.IsNullOrWhiteSpace(hs.Assignment.SpeakingMode));
        var aiSupportedAssignments = homeworkSubmissions.Count(hs => hs.Assignment.AiHintEnabled || hs.Assignment.AiRecommendEnabled);

        return new
        {
            total,
            completed,
            submitted,
            pending,
            late,
            missing,
            average,
            completionRate,
            topics,
            skills,
            grammarTags,
            vocabularyTags,
            speakingAssignments,
            aiSupportedAssignments
        };
    }

    private async Task<object> AggregateTestDataAsync(
        Guid studentProfileId,
        Guid? classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var examResultsQuery = context.ExamResults
            .Include(er => er.Exam)
            .Where(er => er.StudentProfileId == studentProfileId &&
                        er.Exam.Date >= DateOnly.FromDateTime(startDate) &&
                        er.Exam.Date <= DateOnly.FromDateTime(endDate));

        if (classId.HasValue)
        {
            examResultsQuery = examResultsQuery.Where(er => er.Exam.ClassId == classId.Value);
        }

        var examResults = await examResultsQuery
            .OrderBy(er => er.Exam.Date)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var tests = examResults.Select(er => new
        {
            examId = er.ExamId,
            type = er.Exam.ExamType.ToString(),
            score = er.Score,
            maxScore = er.Exam.MaxScore,
            date = er.Exam.Date.ToString("yyyy-MM-dd"),
            comment = er.Comment
        }).ToList();

        return new
        {
            total = tests.Count,
            tests
        };
    }

    private async Task<object> AggregateMissionDataAsync(
        Guid studentProfileId,
        Guid? classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var missionProgressesQuery = context.MissionProgresses
            .Include(mp => mp.Mission)
            .Where(mp => mp.StudentProfileId == studentProfileId &&
                        mp.Mission.StartAt >= startDate &&
                        mp.Mission.StartAt <= endDate);

        if (classId.HasValue)
        {
            // Monthly report is class-scoped, so mission counts/stars/xp should only
            // reflect missions targeted to the current class. Student level/xp stays global.
            missionProgressesQuery = missionProgressesQuery.Where(mp =>
                mp.Mission.Scope == MissionScope.Class &&
                mp.Mission.TargetClassId == classId.Value);
        }

        var missionProgresses = await missionProgressesQuery
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var completed = missionProgresses.Count(mp => mp.Status == MissionProgressStatus.Completed);
        var total = missionProgresses.Count;
        var inProgress = missionProgresses.Count(mp => mp.Status == MissionProgressStatus.InProgress);

        // Get stars and XP from completed missions
        var completedMissions = missionProgresses
            .Where(mp => mp.Status == MissionProgressStatus.Completed && mp.CompletedAt.HasValue)
            .ToList();

        var stars = completedMissions
            .Where(mp => mp.Mission.RewardStars.HasValue)
            .Sum(mp => mp.Mission.RewardStars!.Value);

        var xp = completedMissions
            .Where(mp => mp.Mission.RewardExp.HasValue)
            .Sum(mp => mp.Mission.RewardExp!.Value);

        // Get student level - use separate query with AsNoTracking
        var studentLevel = await context.StudentLevels
            .AsNoTracking()
            .FirstOrDefaultAsync(sl => sl.StudentProfileId == studentProfileId, cancellationToken);

        return new
        {
            completed,
            total,
            inProgress,
            stars,
            xp,
            currentLevel = studentLevel?.CurrentLevel ?? "0",
            currentXp = studentLevel?.CurrentXp ?? 0
        };
    }

    private async Task<object> AggregateSessionReportsDataAsync(
        Guid studentProfileId,
        Guid? classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        var sessionReportsQuery = context.SessionReports
            .Include(sr => sr.Session)
            .Include(sr => sr.TeacherUser)
            .Where(sr => sr.StudentProfileId == studentProfileId &&
                        sr.ReportDate >= DateOnly.FromDateTime(startDate) &&
                        sr.ReportDate <= DateOnly.FromDateTime(endDate));

        if (classId.HasValue)
        {
            sessionReportsQuery = sessionReportsQuery.Where(sr => sr.Session.ClassId == classId.Value);
        }

        var sessionReports = await sessionReportsQuery
            .OrderBy(sr => sr.ReportDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var reports = sessionReports.Select(sr => new
        {
            sessionId = sr.SessionId,
            sessionDate = sr.Session.PlannedDatetime.ToString("yyyy-MM-dd"),
            reportDate = sr.ReportDate.ToString("yyyy-MM-dd"),
            teacherName = sr.TeacherUser?.Name ?? "Unknown",
            feedback = sr.Feedback,
            aiGeneratedSummary = sr.AiGeneratedSummary
        }).ToList();

        return new
        {
            total = reports.Count,
            sessionReports = reports
        };
    }

    private async Task<object> AggregateTopicsCoveredAsync(
        Guid studentProfileId,
        Guid? classId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken)
    {
        // Get sessions with lesson plans for the student in the given period
        var sessionsQuery = context.Sessions
            .Include(s => s.LessonPlan)
            .ThenInclude(lp => lp!.Template)
            .ThenInclude(t => t!.Module)
            .Where(s => s.Attendances.Any(a => a.StudentProfileId == studentProfileId) &&
                       s.PlannedDatetime >= startDate &&
                       s.PlannedDatetime <= endDate &&
                       s.LessonPlan != null &&
                       s.LessonPlan.Template != null);

        if (classId.HasValue)
        {
            sessionsQuery = sessionsQuery.Where(s => s.ClassId == classId.Value);
        }

        var sessionsWithLessonPlans = await sessionsQuery
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // Extract unique topics from lesson plan templates
        var topics = sessionsWithLessonPlans
            .Where(s => !string.IsNullOrWhiteSpace(s.LessonPlan?.Template?.Module?.Name))
            .Select(s => s.LessonPlan!.Template!.Module!.Name)
            .Distinct()
            .ToList();

        // Get all lesson plan contents (planned/actual content)
        var lessonContents = sessionsWithLessonPlans
            .Where(s => !string.IsNullOrWhiteSpace(s.LessonPlan?.PlannedContent))
            .Select(s => s.LessonPlan!.PlannedContent!)
            .ToList();

        return new
        {
            total = topics.Count,
            topics,
            lessonContents
        };
    }

    private static IEnumerable<string> ParseFlexibleTags(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Array.Empty<string>();
        }

        var deserialized = StringListJson.Deserialize(raw);
        if (deserialized.Count > 0)
        {
            return deserialized;
        }

        return StringListJson.ParseTags(raw);
    }
}

