using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.Homework.GetHomeworkAssignments;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetMyCreatedHomeworkAssignments;

public sealed class GetMyCreatedHomeworkAssignmentsQueryHandler(
    IDbContext context,
    IUserContext userContext
) : IQueryHandler<GetMyCreatedHomeworkAssignmentsQuery, GetHomeworkAssignmentsResponse>
{
    public async Task<Result<GetHomeworkAssignmentsResponse>> Handle(
        GetMyCreatedHomeworkAssignmentsQuery query,
        CancellationToken cancellationToken)
    {
        var currentUserId = userContext.UserId;

        var homeworkQuery = context.HomeworkAssignments
            .AsNoTracking()
            .Include(h => h.Class)
                .ThenInclude(c => c.Branch)
            .Include(h => h.HomeworkStudents)
            .Where(h => h.CreatedBy == currentUserId)
            .AsQueryable();

        if (query.ClassId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.ClassId == query.ClassId.Value);
        }

        if (query.SessionId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.SessionId == query.SessionId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Skill))
        {
            homeworkQuery = homeworkQuery.Where(h => h.Skills != null && h.Skills.Contains(query.Skill));
        }

        if (query.SubmissionType.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.SubmissionType == query.SubmissionType.Value);
        }

        if (query.BranchId.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.Class.BranchId == query.BranchId.Value);
        }

        if (query.FromDate.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.CreatedAt >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            homeworkQuery = homeworkQuery.Where(h => h.CreatedAt <= query.ToDate.Value);
        }

        int totalCount = await homeworkQuery.CountAsync(cancellationToken);

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

        return Result.Success(new GetHomeworkAssignmentsResponse
        {
            HomeworkAssignments = page
        });
    }
}
