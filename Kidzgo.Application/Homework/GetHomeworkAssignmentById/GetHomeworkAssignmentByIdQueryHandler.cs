using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkAssignmentById;

public sealed class GetHomeworkAssignmentByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetHomeworkAssignmentByIdQuery, GetHomeworkAssignmentByIdResponse>
{
    public async Task<Result<GetHomeworkAssignmentByIdResponse>> Handle(
        GetHomeworkAssignmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var homework = await context.HomeworkAssignments
            .Include(h => h.Class)
            .Include(h => h.Session)
            .Include(h => h.HomeworkStudents)
                .ThenInclude(hs => hs.StudentProfile)
            .Include(h => h.HomeworkStudents)
                .ThenInclude(hs => hs.SubmissionAttempts)
            .FirstOrDefaultAsync(h => h.Id == query.Id, cancellationToken);

        if (homework is null)
        {
            return Result.Failure<GetHomeworkAssignmentByIdResponse>(
                HomeworkErrors.NotFound(query.Id));
        }

        // Load questions nếu là Quiz/MultipleChoice
        var questions = new List<HomeworkQuestionDetailDto>();
        if (homework.SubmissionType == SubmissionType.Quiz)
        {
            var homeworkQuestions = await context.HomeworkQuestions
                .Where(q => q.HomeworkAssignmentId == homework.Id)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync(cancellationToken);

            questions = homeworkQuestions.Select(q => new HomeworkQuestionDetailDto
            {
                Id = q.Id,
                OrderIndex = q.OrderIndex,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType.ToString(),
                Options = StringListJson.Deserialize(q.Options),
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points,
                Explanation = q.Explanation
            }).ToList();
        }

        var response = new GetHomeworkAssignmentByIdResponse
        {
            Id = homework.Id,
            ClassId = homework.ClassId,
            ClassCode = homework.Class.Code,
            ClassTitle = homework.Class.Title,
            SessionId = homework.SessionId,
            SessionTitle = homework.Session != null 
                ? $"Session {VietnamTime.FormatInVietnam(homework.Session.PlannedDatetime, "dd/MM/yyyy HH:mm")}" 
                : null,
            Title = homework.Title,
            Description = homework.Description,
            StartDate = homework.StartDate,
            DueAt = homework.DueAt,
            Book = homework.Book,
            Pages = homework.Pages,
            Skills = homework.Skills,
            Topic = homework.Topic,
            GrammarTags = StringListJson.Deserialize(homework.GrammarTags),
            VocabularyTags = StringListJson.Deserialize(homework.VocabularyTags),
            SubmissionType = SubmissionTypeMapper.ToApiString(homework.SubmissionType),
            MaxScore = homework.MaxScore,
            RewardStars = homework.RewardStars,
            TimeLimitMinutes = homework.TimeLimitMinutes,
            AllowResubmit = homework.MaxAttempts > 1,
            MaxAttempts = homework.MaxAttempts,
            AiHintEnabled = homework.AiHintEnabled,
            AiRecommendEnabled = homework.AiRecommendEnabled,
            Instructions = homework.Instructions,
            ExpectedAnswer = homework.ExpectedAnswer,
            Rubric = homework.Rubric,
            SpeakingMode = homework.SpeakingMode,
            TargetWords = StringListJson.Deserialize(homework.TargetWords),
            SpeakingExpectedText = homework.SpeakingExpectedText,
            AttachmentUrl = homework.AttachmentUrl,
            CreatedAt = homework.CreatedAt,
            Students = homework.HomeworkStudents.Select(hs => new HomeworkStudentDto
            {
                Id = hs.Id,
                StudentProfileId = hs.StudentProfileId,
                StudentName = hs.StudentProfile.DisplayName,
                Status = hs.Status.ToString(),
                SubmittedAt = hs.SubmittedAt,
                GradedAt = hs.GradedAt,
                Score = hs.Score,
                TeacherFeedback = hs.TeacherFeedback,
                AttemptCount = hs.SubmissionAttempts.Count
            }).ToList(),
            Questions = questions
        };

        return response;
    }
}