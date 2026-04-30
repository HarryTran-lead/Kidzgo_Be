using Kidzgo.API.Extensions;
using Kidzgo.API.Infrastructure;
using Kidzgo.API.Requests;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Classes.GetStudentClasses;
using Kidzgo.Application.Homework.AnalyzeSpeakingPractice;
using Kidzgo.Application.Homework.GetHomeworkHint;
using Kidzgo.Application.Homework.GetHomeworkRecommendations;
using Kidzgo.Application.Homework.GetHomeworkSpeakingAnalysis;
using Kidzgo.Application.Homework.GetStudentHomeworkFeedback;
using Kidzgo.Application.Homework.GetStudentHomeworks;
using Kidzgo.Application.Homework.GetStudentHomeworkSubmission;
using Kidzgo.Application.Homework.SubmitHomework;
using Kidzgo.Application.Homework.SubmitMultipleChoiceHomework;
using Kidzgo.Application.LearningHistory.GetLearningHistory;
using Kidzgo.Application.Sessions.GetStudentTimetable;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Media;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.API.Controllers;

[Route("api/students")]
[ApiController]
[Authorize]
public class StudentController : ControllerBase
{
    private readonly IDbContext _context;
    private readonly ISender _mediator;
    private readonly IUserContext _userContext;

    public StudentController(ISender mediator, IDbContext context, IUserContext userContext)
    {
        _mediator = mediator;
        _context = context;
        _userContext = userContext;
    }

    [HttpGet("classes")]
    public async Task<IResult> GetClasses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentClassesQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("timetable")]
    public async Task<IResult> GetTimetable(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentTimetableQuery
        {
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("learning-history")]
    public async Task<IResult> GetLearningHistory(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int sessionPageNumber = 1,
        [FromQuery] int sessionPageSize = 20,
        [FromQuery] int missionPageNumber = 1,
        [FromQuery] int missionPageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLearningHistoryQuery
        {
            From = from,
            To = to,
            SessionPageNumber = sessionPageNumber,
            SessionPageSize = sessionPageSize,
            MissionPageNumber = missionPageNumber,
            MissionPageSize = missionPageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("homework/submit")]
    public async Task<IResult> SubmitHomework(
        [FromBody] SubmitHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SubmitHomeworkCommand
        {
            HomeworkStudentId = request.HomeworkStudentId,
            TextAnswer = request.TextAnswer,
            AttachmentUrls = request.AttachmentUrls,
            LinkUrl = request.LinkUrl,
            Links = request.Links
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("homework/multiple-choice/submit")]
    public async Task<IResult> SubmitMultipleChoiceHomework(
        [FromBody] SubmitMultipleChoiceHomeworkRequest request,
        CancellationToken cancellationToken)
    {
        var answers = request.Answers.Select(a => new SubmitAnswerDto
        {
            QuestionId = a.QuestionId,
            SelectedOptionId = a.SelectedOptionId
        }).ToList();

        var command = new SubmitMultipleChoiceHomeworkCommand
        {
            HomeworkStudentId = request.HomeworkStudentId,
            Answers = answers
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("homework/my")]
    public async Task<IResult> GetHomeworks(
        [FromQuery] string? status,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        HomeworkStatus? parsedStatus = null;
        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<HomeworkStatus>(status, ignoreCase: true, out var tmpStatus))
        {
            parsedStatus = tmpStatus;
        }

        var query = new GetStudentHomeworksQuery
        {
            Status = parsedStatus,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("homework/submitted")]
    public async Task<IResult> GetSubmittedHomeworks(
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentHomeworksQuery
        {
            Status = null,
            ClassId = classId,
            PageNumber = 1,
            PageSize = 1000
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.MatchOk();
        }

        var submittedAndGraded = result.Value.Homeworks.Items
            .Where(h => h.Status == HomeworkStatus.Submitted.ToString() ||
                        h.Status == HomeworkStatus.Graded.ToString())
            .ToList();

        var totalCount = submittedAndGraded.Count;
        var pagedItems = submittedAndGraded
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var filteredPage = new Page<StudentHomeworkDto>(
            pagedItems,
            totalCount,
            pageNumber,
            pageSize);

        var filteredResponse = new GetStudentHomeworksResponse
        {
            Homeworks = filteredPage
        };

        return Results.Ok(ApiResult<GetStudentHomeworksResponse>.Success(filteredResponse));
    }

    [HttpGet("homework/{homeworkStudentId:guid}")]
    public async Task<IResult> GetHomeworkSubmission(
        Guid homeworkStudentId,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentHomeworkSubmissionQuery
        {
            HomeworkStudentId = homeworkStudentId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("homework/{homeworkStudentId:guid}/attempts/{attemptNumber:int}")]
    public async Task<IResult> GetHomeworkSubmissionAttempt(
        Guid homeworkStudentId,
        int attemptNumber,
        CancellationToken cancellationToken)
    {
        var query = new GetStudentHomeworkSubmissionQuery
        {
            HomeworkStudentId = homeworkStudentId,
            AttemptNumber = attemptNumber
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("homework/{homeworkStudentId:guid}/hint")]
    public async Task<IResult> GetHomeworkHint(
        Guid homeworkStudentId,
        [FromBody] GetHomeworkHintRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetHomeworkHintQuery
        {
            HomeworkStudentId = homeworkStudentId,
            CurrentAnswerText = request.CurrentAnswerText,
            Language = request.Language
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("homework/{homeworkStudentId:guid}/recommendations")]
    public async Task<IResult> GetHomeworkRecommendations(
        Guid homeworkStudentId,
        [FromBody] GetHomeworkRecommendationsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetHomeworkRecommendationsQuery
        {
            HomeworkStudentId = homeworkStudentId,
            CurrentAnswerText = request.CurrentAnswerText,
            Language = request.Language,
            MaxItems = request.MaxItems
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("homework/feedback/my")]
    public async Task<IResult> GetMyHomeworkFeedback(
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStudentHomeworkFeedbackQuery
        {
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("homework/{homeworkStudentId:guid}/speaking-analysis")]
    public async Task<IResult> GetHomeworkSpeakingAnalysis(
        Guid homeworkStudentId,
        [FromBody] GetHomeworkSpeakingAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        var query = new GetHomeworkSpeakingAnalysisQuery
        {
            HomeworkStudentId = homeworkStudentId,
            CurrentTranscript = request.CurrentTranscript,
            Language = request.Language
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("ai-speaking/analyze")]
    [RequestSizeLimit(100_000_000)]
    [Consumes("multipart/form-data")]
    public async Task<IResult> AnalyzeSpeakingPractice(
        [FromForm] AnalyzeSpeakingPracticeRequest request,
        CancellationToken cancellationToken)
    {
        byte[] fileBytes = Array.Empty<byte>();
        var fileName = request.File?.FileName ?? "speaking-practice";
        var contentType = string.IsNullOrWhiteSpace(request.File?.ContentType)
            ? "application/octet-stream"
            : request.File!.ContentType;

        if (request.File is not null)
        {
            await using var inputStream = request.File.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await inputStream.CopyToAsync(memoryStream, cancellationToken);
            fileBytes = memoryStream.ToArray();
        }

        var query = new AnalyzeSpeakingPracticeQuery
        {
            HomeworkStudentId = request.HomeworkStudentId,
            FileBytes = fileBytes,
            FileName = fileName,
            ContentType = contentType,
            Language = request.Language,
            Mode = request.Mode,
            Topic = request.Topic,
            ExpectedText = request.ExpectedText,
            TargetWords = request.TargetWords,
            ConversationHistory = request.ConversationHistory,
            Instructions = request.Instructions
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("/api/student/dashboard")]
    public async Task<IResult> GetStudentDashboard(CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not found");
        }

        var now = VietnamTime.UtcNow();
        var today = VietnamTime.ToVietnamDateOnly(now);
        var todayFromUtc = VietnamTime.TodayStartUtc();
        var todayToUtc = VietnamTime.EndOfVietnamDayUtc(todayFromUtc);
        var classIds = await GetActiveClassIdsForStudentAsync(student.Id, cancellationToken);

        var notices = await _context.Notifications
            .AsNoTracking()
            .Where(n =>
                (n.RecipientUserId == _userContext.UserId || n.RecipientProfileId == student.Id) &&
                n.CreatedAt >= now.AddDays(-14))
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .Select(n => new
            {
                n.Id,
                n.Title,
                n.Content,
                n.Kind,
                n.Priority,
                n.CreatedAt,
                isRead = n.ReadAt != null
            })
            .ToListAsync(cancellationToken);

        var todaySession = await _context.Sessions
            .AsNoTracking()
            .Where(s =>
                classIds.Contains(s.ClassId) &&
                s.PlannedDatetime >= todayFromUtc &&
                s.PlannedDatetime <= todayToUtc)
            .OrderBy(s => s.PlannedDatetime)
            .Select(s => new
            {
                sessionId = s.Id,
                classId = s.ClassId,
                className = s.Class.Title,
                s.PlannedDatetime,
                s.DurationMinutes,
                teacherName = s.PlannedTeacher != null ? s.PlannedTeacher.Name : null,
                roomName = s.PlannedRoom != null ? s.PlannedRoom.Name : null,
                status = s.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);

        var todayClass = todaySession is null
            ? null
            : new
            {
                todaySession.sessionId,
                todaySession.classId,
                todaySession.className,
                plannedDate = VietnamTime.ToVietnamDateOnly(todaySession.PlannedDatetime),
                startTime = todaySession.PlannedDatetime,
                endTime = todaySession.PlannedDatetime.AddMinutes(todaySession.DurationMinutes),
                todaySession.teacherName,
                todaySession.roomName,
                todaySession.status
            };

        var pendingTasks = await _context.HomeworkStudents
            .AsNoTracking()
            .Where(hs =>
                hs.StudentProfileId == student.Id &&
                (hs.Status == HomeworkStatus.Assigned ||
                 hs.Status == HomeworkStatus.Missing ||
                 hs.Status == HomeworkStatus.Late))
            .OrderBy(hs => hs.Assignment.DueAt)
            .Take(10)
            .Select(hs => new
            {
                id = hs.Id,
                title = hs.Assignment.Title,
                dueDate = hs.Assignment.DueAt,
                status = hs.Status.ToString(),
                className = hs.Assignment.Class.Title
            })
            .ToListAsync(cancellationToken);

        var attendanceCount = await _context.Attendances
            .AsNoTracking()
            .CountAsync(a => a.StudentProfileId == student.Id, cancellationToken);
        var presentCount = attendanceCount == 0
            ? 0
            : await _context.Attendances
                .AsNoTracking()
                .CountAsync(a => a.StudentProfileId == student.Id && a.AttendanceStatus == AttendanceStatus.Present, cancellationToken);

        var levelInfo = await _context.StudentLevels
            .AsNoTracking()
            .Where(sl => sl.StudentProfileId == student.Id)
            .Select(sl => new { sl.CurrentLevel, sl.CurrentXp })
            .FirstOrDefaultAsync(cancellationToken);

        var stars = await _context.StarTransactions
            .AsNoTracking()
            .Where(st => st.StudentProfileId == student.Id)
            .SumAsync(st => (int?)st.Amount, cancellationToken) ?? 0;

        var teacherNote = await _context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => r.StudentProfileId == student.Id && r.Status == ReportStatus.Published && r.FinalContent != null)
            .OrderByDescending(r => r.PublishedAt ?? r.UpdatedAt)
            .Select(r => r.FinalContent)
            .FirstOrDefaultAsync(cancellationToken);

        return OkData(new
        {
            displayName = student.DisplayName,
            stats = new
            {
                activeClasses = classIds.Count,
                attendancePercent = attendanceCount == 0 ? 0 : Math.Round((decimal)presentCount * 100 / attendanceCount, 2),
                xp = levelInfo?.CurrentXp ?? 0,
                level = int.TryParse(levelInfo?.CurrentLevel, out var level) ? level : (int?)null,
                stars
            },
            notices,
            todayClass,
            teacherNote,
            pendingTasks
        });
    }

    [HttpGet("/api/student/profile")]
    public async Task<IResult> GetStudentProfile(CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not found");
        }

        return OkData(await BuildStudentProfileAsync(student, cancellationToken));
    }

    [HttpGet("/api/student/media")]
    public async Task<IResult> GetStudentMedia(
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not found");
        }

        var mediaQuery = _context.MediaAssets
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.StudentProfileId == student.Id);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<MediaType>(type, true, out var parsedType))
        {
            mediaQuery = mediaQuery.Where(m => m.Type == parsedType);
        }

        var media = await mediaQuery
            .OrderByDescending(m => m.CreatedAt)
            .Take(100)
            .Select(m => new
            {
                id = m.Id,
                albumId = m.MonthTag ?? "general",
                title = m.Caption ?? m.OriginalFileName ?? m.Url,
                type = m.Type.ToString(),
                contentType = m.ContentType.ToString(),
                date = m.CreatedAt,
                coverUrl = m.Url,
                count = 1,
                url = m.Url
            })
            .ToListAsync(cancellationToken);

        return OkData(new
        {
            albums = media
                .GroupBy(m => new { m.albumId, month = m.date.ToString("yyyy-MM"), m.type })
                .Select(g => new
                {
                    albumId = g.Key.albumId,
                    title = g.Key.month,
                    type = g.Key.type,
                    date = g.Max(x => x.date),
                    coverUrl = g.First().coverUrl,
                    count = g.Count()
                })
                .OrderByDescending(a => a.date)
                .ToList(),
            items = media
        });
    }

    [HttpGet("/api/student/tests")]
    public async Task<IResult> GetStudentTests(CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not found");
        }

        var items = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.StudentProfileId == student.Id)
            .OrderByDescending(er => er.Exam.Date)
            .Select(er => new
            {
                id = er.Id,
                title = er.Exam.Description ?? er.Exam.ExamType.ToString(),
                type = er.Exam.ExamType.ToString(),
                subject = er.Exam.Class.Program.Name,
                className = er.Exam.Class.Title,
                testDate = er.Exam.Date,
                duration = er.Exam.TimeLimitMinutes,
                status = er.GradedAt != null ? "Graded" : "Pending",
                score = er.Score,
                maxScore = er.Exam.MaxScore,
                percentage = (er.Exam.MaxScore ?? 0) == 0 ? 0 : Math.Round((er.Score ?? 0) * 100 / (er.Exam.MaxScore ?? 1), 2),
                averageScore = _context.ExamResults.Where(x => x.ExamId == er.ExamId).Average(x => (decimal?)x.Score),
                rank = _context.ExamResults.Count(x => x.ExamId == er.ExamId && x.Score > er.Score) + 1,
                totalStudents = _context.ExamResults.Count(x => x.ExamId == er.ExamId)
            })
            .ToListAsync(cancellationToken);

        return OkData(items);
    }

    [HttpGet("/api/student/tests/{id:guid}")]
    public async Task<IResult> GetStudentTestDetail(Guid id, CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not found");
        }

        var examResult = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.Id == id && er.StudentProfileId == student.Id)
            .Select(er => new
            {
                er.Id,
                title = er.Exam.Description ?? er.Exam.ExamType.ToString(),
                type = er.Exam.ExamType.ToString(),
                subject = er.Exam.Class.Program.Name,
                className = er.Exam.Class.Title,
                testDate = er.Exam.Date,
                duration = er.Exam.TimeLimitMinutes,
                status = er.GradedAt != null ? "Graded" : "Pending",
                score = er.Score,
                maxScore = er.Exam.MaxScore,
                percentage = (er.Exam.MaxScore ?? 0) == 0 ? 0 : Math.Round((er.Score ?? 0) * 100 / (er.Exam.MaxScore ?? 1), 2),
                averageScore = _context.ExamResults.Where(x => x.ExamId == er.ExamId).Average(x => (decimal?)x.Score),
                rank = _context.ExamResults.Count(x => x.ExamId == er.ExamId && x.Score > er.Score) + 1,
                totalStudents = _context.ExamResults.Count(x => x.ExamId == er.ExamId),
                feedback = er.Comment
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (examResult is null)
        {
            return NotFoundProblem("ExamResult", "Exam result not found");
        }

        return OkData(new
        {
            examResult,
            skillBreakdown = Array.Empty<object>(),
            sections = Array.Empty<object>(),
            feedback = examResult.feedback,
            ranking = new { examResult.rank, examResult.totalStudents },
            answerSheet = Array.Empty<object>(),
            improvement = (string?)null
        });
    }

    [HttpGet("/api/student/reports")]
    public async Task<IResult> GetStudentReports(
        [FromQuery] string? type,
        CancellationToken cancellationToken)
    {
        var student = await ResolveStudentProfileAsync(cancellationToken);
        if (student is null)
        {
            return NotFoundProblem("StudentProfile", "Student profile not found");
        }

        var lessonReports = await _context.SessionReports
            .AsNoTracking()
            .Where(r => r.StudentProfileId == student.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .Select(r => new
            {
                id = r.Id,
                sessionId = r.SessionId,
                className = r.Session.Class.Title,
                reportDate = r.CreatedAt,
                status = r.Status,
                feedback = r.Feedback
            })
            .ToListAsync(cancellationToken);

        var monthlySummaries = await _context.StudentMonthlyReports
            .AsNoTracking()
            .Where(r => r.StudentProfileId == student.Id)
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Month)
            .Take(12)
            .Select(r => new
            {
                id = r.Id,
                r.Month,
                r.Year,
                status = r.Status.ToString(),
                summary = r.FinalContent ?? r.DraftContent
            })
            .ToListAsync(cancellationToken);

        if (string.Equals(type, "lesson", StringComparison.OrdinalIgnoreCase))
        {
            return OkData(new { lessonReports });
        }

        if (string.Equals(type, "monthly", StringComparison.OrdinalIgnoreCase))
        {
            return OkData(new { monthlySummaries });
        }

        return OkData(new
        {
            lessonReports,
            progressSummary = new
            {
                totalLessonReports = lessonReports.Count,
                publishedMonthlyReports = monthlySummaries.Count(x => x.status == ReportStatus.Published.ToString())
            },
            monthlySummaries
        });
    }

    private async Task<Profile?> ResolveStudentProfileAsync(CancellationToken cancellationToken)
    {
        IQueryable<Profile> query = _context.Profiles
            .AsNoTracking()
            .Where(p => p.ProfileType == ProfileType.Student && !p.IsDeleted);

        if (_userContext.StudentId.HasValue)
        {
            query = query.Where(p => p.Id == _userContext.StudentId.Value);
        }
        else
        {
            query = query.Where(p => p.UserId == _userContext.UserId);
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<Guid>> GetActiveClassIdsForStudentAsync(Guid studentProfileId, CancellationToken cancellationToken)
    {
        return await _context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => ce.StudentProfileId == studentProfileId && ce.Status == Domain.Classes.EnrollmentStatus.Active)
            .Select(ce => ce.ClassId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private async Task<object> BuildStudentProfileAsync(Profile student, CancellationToken cancellationToken)
    {
        var attendanceTotal = await _context.Attendances
            .AsNoTracking()
            .CountAsync(a => a.StudentProfileId == student.Id, cancellationToken);
        var attendancePresent = attendanceTotal == 0
            ? 0
            : await _context.Attendances
                .AsNoTracking()
                .CountAsync(a => a.StudentProfileId == student.Id && a.AttendanceStatus == AttendanceStatus.Present, cancellationToken);

        var scores = await _context.ExamResults
            .AsNoTracking()
            .Where(er => er.StudentProfileId == student.Id)
            .OrderByDescending(er => er.CreatedAt)
            .Take(20)
            .Select(er => new
            {
                er.Id,
                examId = er.ExamId,
                title = er.Exam.Description ?? er.Exam.ExamType.ToString(),
                score = er.Score,
                maxScore = er.Exam.MaxScore,
                gradedAt = er.GradedAt
            })
            .ToListAsync(cancellationToken);

        var courseHistory = await _context.ClassEnrollments
            .AsNoTracking()
            .Where(ce => ce.StudentProfileId == student.Id)
            .OrderByDescending(ce => ce.CreatedAt)
            .Take(20)
            .Select(ce => new
            {
                classId = ce.ClassId,
                className = ce.Class.Title,
                programName = ce.Class.Program.Name,
                status = ce.Status.ToString(),
                createdAt = ce.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new
        {
            studentInfo = new
            {
                student.Id,
                student.DisplayName,
                student.Name,
                student.DateOfBirth,
                student.AvatarUrl,
                email = student.User.Email,
                phone = student.User.PhoneNumber
            },
            attendancePercent = attendanceTotal == 0 ? 0 : Math.Round((decimal)attendancePresent * 100 / attendanceTotal, 2),
            scores,
            courseHistory,
            certificates = Array.Empty<object>()
        };
    }

    private static IResult OkData<T>(T data)
    {
        return Results.Ok(ApiResult<T>.Success(data));
    }

    private static IResult NotFoundProblem(string title, string detail)
    {
        return Results.Problem(title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);
    }
}
