using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkAssignments;

public sealed class GetHomeworkAssignmentsQueryHandler(
    IDbContext context
) : IQueryHandler<GetHomeworkAssignmentsQuery, GetHomeworkAssignmentsResponse>
{
    public async Task<Result<GetHomeworkAssignmentsResponse>> Handle(
        GetHomeworkAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        var homeworkQuery = context.HomeworkAssignments
            .Include(h => h.Class)
                .ThenInclude(c => c.Branch)
            .Include(h => h.HomeworkStudents)
            .AsQueryable();

        // Filter by class
        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.ClassId == query.ClassId.Value);
        }

        // Filter by session
        if (query.SessionId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.SessionId == query.SessionId.Value);
        }

        // Filter by skill
        if (!string.IsNullOrWhiteSpace(query.Skill))
        {
            homeworkQuery = homeworkQuery.Where(h => h.Skills != null && h.Skills.Contains(query.Skill));
        }

        // Filter by submission type
        if (query.SubmissionType.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.SubmissionType == query.SubmissionType.Value);
        }

        // Filter by branch
        if (query.BranchId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.Class.BranchId == query.BranchId.Value);
        }

        // Filter by date range (created_at)
        if (query.FromDate.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.CreatedAt <= query.ToDate.Value);
        }

        // Get total count
        int totalCount = await homeworkQuery.CountAsync(cancellationToken);

        // Apply pagination and select
        var homeworkAssignments = await homeworkQuery
            .OrderByDescending(h => h.CreatedAt)
            .ThenByDescending(h => h.DueAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(h => new HomeworkAssignmentDto
            {
                Id = h.Id,
                ClassId = h.ClassId,
                ClassCode = h.Class.Code,
                ClassTitle = h.Class.Title,
                SessionId = h.SessionId,
                Title = h.Title,
                Description = h.Description,
                StartDate = h.StartDate,
                DueAt = h.DueAt,
                Book = h.Book,
                Pages = h.Pages,
                Skills = h.Skills,
                Topic = h.Topic,
                AttachmentUrl = h.AttachmentUrl,
                SubmissionType = SubmissionTypeMapper.ToApiString(h.SubmissionType),
                MaxScore = h.MaxScore,
                RewardStars = h.RewardStars,
                TimeLimitMinutes = h.TimeLimitMinutes,
                AllowResubmit = h.MaxAttempts > 1,
                MaxAttempts = h.MaxAttempts,
                AiHintEnabled = h.AiHintEnabled,
                AiRecommendEnabled = h.AiRecommendEnabled,
                SpeakingMode = h.SpeakingMode,
                CreatedAt = h.CreatedAt,
                TotalStudents = h.HomeworkStudents.Count,
                SubmittedCount = h.HomeworkStudents.Count(hs => hs.Status == HomeworkStatus.Submitted || hs.Status == HomeworkStatus.Graded),
                GradedCount = h.HomeworkStudents.Count(hs => hs.Status == HomeworkStatus.Graded),
                LateCount = h.HomeworkStudents.Count(hs => hs.Status == HomeworkStatus.Late),
                MissingCount = h.HomeworkStudents.Count(hs => hs.Status == HomeworkStatus.Missing)
            })
            .ToListAsync(cancellationToken);

        var page = new Page<HomeworkAssignmentDto>(
            homeworkAssignments,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetHomeworkAssignmentsResponse
        {
            HomeworkAssignments = page
        };
    }
}

