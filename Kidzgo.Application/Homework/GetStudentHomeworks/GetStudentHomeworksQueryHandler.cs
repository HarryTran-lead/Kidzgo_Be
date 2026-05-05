using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetStudentHomeworks;

public sealed class GetStudentHomeworksQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetStudentHomeworksQuery, GetStudentHomeworksResponse>
{
    public async Task<Result<GetStudentHomeworksResponse>> Handle(
        GetStudentHomeworksQuery query,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetStudentHomeworksResponse>(ProfileErrors.StudentNotFound);
        }

        var profile = await context.Profiles
            .FirstOrDefaultAsync(p => p.Id == studentId.Value, cancellationToken);

        if (profile == null || profile.ProfileType != Domain.Users.ProfileType.Student || !profile.IsActive || profile.IsDeleted)
        {
            return Result.Failure<GetStudentHomeworksResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkQuery = context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .Where(hs => hs.StudentProfileId == studentId.Value)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Status == query.Status.Value);
        }

        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(hs => hs.Assignment.ClassId == query.ClassId.Value);
        }

        var totalCount = await homeworkQuery.CountAsync(cancellationToken);
        var now = VietnamTime.UtcNow();

        var homeworkRows = await homeworkQuery
            .OrderByDescending(hs => hs.Assignment.DueAt)
            .ThenByDescending(hs => hs.Assignment.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(hs => new
            {
                hs.Id,
                hs.AssignmentId,
                hs.Assignment.Title,
                hs.Assignment.Description,
                AssignmentAttachmentUrl = hs.Assignment.AttachmentUrl,
                hs.Assignment.ClassId,
                ClassCode = hs.Assignment.Class.Code,
                ClassTitle = hs.Assignment.Class.Title,
                hs.Assignment.StartDate,
                hs.Assignment.DueAt,
                hs.Assignment.Book,
                hs.Assignment.Pages,
                hs.Assignment.Skills,
                hs.Assignment.SubmissionType,
                hs.Assignment.MaxScore,
                hs.Assignment.TimeLimitMinutes,
                hs.Assignment.MaxAttempts,
                hs.Status,
                hs.StartedAt,
                hs.SubmittedAt,
                hs.GradedAt,
                hs.Score,
                hs.TeacherFeedback,
                hs.AiFeedback,
                hs.TextAnswer,
                hs.AttachmentUrl,
                AssignmentCreatedAt = hs.Assignment.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var homeworkStudentIds = homeworkRows.Select(x => x.Id).ToList();
        var attemptsLookup = await context.HomeworkSubmissionAttempts
            .Where(a => homeworkStudentIds.Contains(a.HomeworkStudentId))
            .OrderByDescending(a => a.AttemptNumber)
            .Select(a => new
            {
                a.Id,
                a.HomeworkStudentId,
                a.AttemptNumber,
                a.Status,
                a.StartedAt,
                a.SubmittedAt,
                a.GradedAt,
                a.Score,
                a.TeacherFeedback,
                a.AiFeedback,
                a.TextAnswer,
                a.AttachmentUrl,
                a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var attemptsByHomeworkStudentId = attemptsLookup
            .GroupBy(a => a.HomeworkStudentId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(a => new HomeworkSubmissionAttempt
                {
                    Id = a.Id,
                    HomeworkStudentId = a.HomeworkStudentId,
                    AttemptNumber = a.AttemptNumber,
                    Status = a.Status,
                    StartedAt = a.StartedAt,
                    SubmittedAt = a.SubmittedAt,
                    GradedAt = a.GradedAt,
                    Score = a.Score,
                    TeacherFeedback = a.TeacherFeedback,
                    AiFeedback = a.AiFeedback,
                    TextAnswer = a.TextAnswer,
                    AttachmentUrl = a.AttachmentUrl,
                    CreatedAt = a.CreatedAt
                }).ToList());

        var homeworks = homeworkRows.Select(row =>
        {
            var snapshot = new HomeworkStudent
            {
                Id = row.Id,
                AssignmentId = row.AssignmentId,
                Status = row.Status,
                StartedAt = row.StartedAt,
                SubmittedAt = row.SubmittedAt,
                GradedAt = row.GradedAt,
                Score = row.Score,
                TeacherFeedback = row.TeacherFeedback,
                AiFeedback = row.AiFeedback,
                TextAnswer = row.TextAnswer,
                AttachmentUrl = row.AttachmentUrl
            };

            var attempts = attemptsByHomeworkStudentId.TryGetValue(row.Id, out var persistedAttempts)
                ? HomeworkSubmissionAttemptMapper.BuildDtos(snapshot, persistedAttempts)
                : HomeworkSubmissionAttemptMapper.BuildDtos(snapshot, Array.Empty<HomeworkSubmissionAttempt>());

            return new StudentHomeworkDto
            {
                Id = row.Id,
                AssignmentId = row.AssignmentId,
                AssignmentTitle = row.Title,
                AssignmentDescription = row.Description,
                AssignmentAttachmentUrl = row.AssignmentAttachmentUrl,
                ClassId = row.ClassId,
                ClassCode = row.ClassCode,
                ClassTitle = row.ClassTitle,
                StartDate = row.StartDate,
                DueAt = row.DueAt,
                Book = row.Book,
                Pages = row.Pages,
                Skills = row.Skills,
                SubmissionType = SubmissionTypeMapper.ToApiString(row.SubmissionType),
                MaxScore = row.MaxScore,
                TimeLimitMinutes = row.TimeLimitMinutes,
                Status = row.Status.ToString(),
                SubmittedAt = row.SubmittedAt,
                GradedAt = row.GradedAt,
                Score = row.Score,
                AllowResubmit = row.MaxAttempts > 1,
                MaxAttempts = row.MaxAttempts,
                AttemptCount = attempts.Count,
                Attempts = attempts,
                IsLate = row.Status == HomeworkStatus.Late,
                IsOverdue = row.DueAt.HasValue &&
                            now > row.DueAt.Value &&
                            (row.Status == HomeworkStatus.Assigned || row.Status == HomeworkStatus.Missing)
            };
        }).ToList();

        var page = new Page<StudentHomeworkDto>(
            homeworks,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetStudentHomeworksResponse
        {
            Homeworks = page
        };
    }
}
