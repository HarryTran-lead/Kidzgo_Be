using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.CreateMultipleChoiceHomework;

public sealed class CreateMultipleChoiceHomeworkCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMultipleChoiceHomeworkCommand, CreateMultipleChoiceHomeworkResponse>
{
    public async Task<Result<CreateMultipleChoiceHomeworkResponse>> Handle(
        CreateMultipleChoiceHomeworkCommand command,
        CancellationToken cancellationToken)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidTitle);
        }

        // Validate questions
        if (command.Questions == null || command.Questions.Count == 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.NoQuestionsProvided);
        }

        var normalizedCorrectAnswers = new List<string>(command.Questions.Count);

        // Validate each question
        for (int i = 0; i < command.Questions.Count; i++)
        {
            var question = command.Questions[i];

            if (string.IsNullOrWhiteSpace(question.QuestionText))
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.InvalidQuestionText(i + 1));
            }

            if (question.QuestionType == HomeworkQuestionType.MultipleChoice)
            {
                if (question.Options == null || question.Options.Count < 2)
                {
                    return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                        HomeworkErrors.InsufficientOptions(i + 1));
                }

                var normalizedCorrectAnswer = QuizOptionUtils.NormalizeCorrectAnswerForStorage(
                    question.Options,
                    question.CorrectAnswer);

                if (string.IsNullOrWhiteSpace(normalizedCorrectAnswer))
                {
                    return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                        HomeworkErrors.InvalidCorrectAnswer(i + 1));
                }

                normalizedCorrectAnswers.Add(normalizedCorrectAnswer);
            }
            else
            {
                normalizedCorrectAnswers.Add(question.CorrectAnswer.Trim());
            }

            if (question.Points <= 0)
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.InvalidPoints(i + 1));
            }
        }

        // Validate class exists
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ClassNotFound);
        }

        // Validate session if provided
        if (command.SessionId.HasValue)
        {
            var session = await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == command.SessionId.Value, cancellationToken);

            if (session is null || session.ClassId != command.ClassId)
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.SessionNotFound(command.SessionId));
            }
        }

        // Validate mission if provided
        if (command.MissionId.HasValue)
        {
            var mission = await context.Missions
                .FirstOrDefaultAsync(m => m.Id == command.MissionId.Value, cancellationToken);

            if (mission is null)
            {
                return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                    HomeworkErrors.MissionNotFound(command.MissionId));
            }
        }

        var startDateUtc = VietnamTime.NormalizeToUtc(command.StartDate);

        // Validate due date
        if (command.DueAt.HasValue && VietnamTime.NormalizeToUtc(command.DueAt.Value) <= VietnamTime.UtcNow())
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidDueDate);
        }

        if (command.TimeLimitMinutes.HasValue && command.TimeLimitMinutes.Value <= 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidTimeLimitMinutes);
        }

        if (command.MaxAttempts <= 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidMaxAttempts);
        }

        var dueAtUtc = VietnamTime.NormalizeToUtc(command.DueAt);
        if (startDateUtc.HasValue && dueAtUtc.HasValue && startDateUtc.Value > dueAtUtc.Value)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(Error.Validation(
                "Homework.InvalidStartDate",
                "Start date must be before or equal to due date"));
        }

        // Get current user ID from context
        var currentUserId = userContext.UserId;

        var now = VietnamTime.UtcNow();
        var normalizedSkills = HomeworkDeliveryMetadata.NormalizeSkills(command.Skills, command.AttachmentUrl);

        // Calculate max score from questions
        var maxScore = command.Questions.Sum(q => q.Points);

        var homework = new HomeworkAssignment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            SessionId = command.SessionId,
            Title = command.Title,
            Description = command.Description,
            StartDate = startDateUtc,
            DueAt = dueAtUtc,
            Skills = normalizedSkills,
            Topic = command.Topic,
            GrammarTags = StringListJson.Serialize(command.GrammarTags),
            VocabularyTags = StringListJson.Serialize(command.VocabularyTags),
            SubmissionType = SubmissionType.Quiz,
            MaxScore = maxScore,
            RewardStars = command.RewardStars,
            TimeLimitMinutes = command.TimeLimitMinutes,
            MaxAttempts = command.MaxAttempts,
            AiHintEnabled = command.AiHintEnabled ?? false,
            AiRecommendEnabled = command.AiRecommendEnabled ?? false,
            MissionId = command.MissionId,
            Instructions = command.Instructions,
            AttachmentUrl = command.AttachmentUrl,
            CreatedBy = currentUserId,
            CreatedAt = now
        };

        context.HomeworkAssignments.Add(homework);

        // Create questions
        var questionDtos = new List<HomeworkQuestionDto>();
        for (int i = 0; i < command.Questions.Count; i++)
        {
            var q = command.Questions[i];
            var question = new HomeworkQuestion
            {
                Id = Guid.NewGuid(),
                HomeworkAssignmentId = homework.Id,
                OrderIndex = i,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = JsonSerializer.Serialize(q.Options),
                CorrectAnswer = normalizedCorrectAnswers[i],
                Points = q.Points,
                Explanation = q.Explanation
            };

            context.HomeworkQuestions.Add(question);

            questionDtos.Add(new HomeworkQuestionDto
            {
                Id = question.Id,
                OrderIndex = i,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType.ToString(),
                Options = q.Options,
                Points = q.Points
            });
        }

        // Auto-assign to all active students in the class
        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.ClassId == command.ClassId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ClassHasNoActiveStudents(command.ClassId));
        }

        var homeworkStudents = activeEnrollments.Select(studentId => new HomeworkStudent
        {
            Id = Guid.NewGuid(),
            AssignmentId = homework.Id,
            StudentProfileId = studentId,
            Status = HomeworkStatus.Assigned
        }).ToList();

        context.HomeworkStudents.AddRange(homeworkStudents);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateMultipleChoiceHomeworkResponse
        {
            Id = homework.Id,
            ClassId = homework.ClassId,
            SessionId = homework.SessionId,
            Title = homework.Title,
            Description = homework.Description,
            StartDate = homework.StartDate,
            DueAt = homework.DueAt,
            Skills = homework.Skills,
            Topic = homework.Topic,
            GrammarTags = StringListJson.Deserialize(homework.GrammarTags),
            VocabularyTags = StringListJson.Deserialize(homework.VocabularyTags),
            RewardStars = homework.RewardStars,
            TimeLimitMinutes = homework.TimeLimitMinutes,
            AllowResubmit = homework.MaxAttempts > 1,
            MaxAttempts = homework.MaxAttempts,
            AiHintEnabled = homework.AiHintEnabled,
            AiRecommendEnabled = homework.AiRecommendEnabled,
            Instructions = homework.Instructions,
            AttachmentUrl = homework.AttachmentUrl,
            CreatedAt = homework.CreatedAt,
            AssignedStudentsCount = homeworkStudents.Count,
            Questions = questionDtos
        };
    }
}

