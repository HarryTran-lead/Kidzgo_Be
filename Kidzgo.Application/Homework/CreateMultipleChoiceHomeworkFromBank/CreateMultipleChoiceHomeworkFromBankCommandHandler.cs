using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.CreateMultipleChoiceHomework;
using Kidzgo.Application.QuestionBank.Shared;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Microsoft.EntityFrameworkCore;
using QuizOptionUtils = Kidzgo.Application.Homework.Shared.QuizOptionUtils;
namespace Kidzgo.Application.Homework.CreateMultipleChoiceHomeworkFromBank;

public sealed class CreateMultipleChoiceHomeworkFromBankCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateMultipleChoiceHomeworkFromBankCommand, CreateMultipleChoiceHomeworkResponse>
{
    public async Task<Result<CreateMultipleChoiceHomeworkResponse>> Handle(
        CreateMultipleChoiceHomeworkFromBankCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidTitle);
        }

        if (command.Distribution == null || command.Distribution.Count == 0 ||
            command.Distribution.Any(d => d.Count <= 0))
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InvalidQuestionDistribution);
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ProgramNotFound(command.ProgramId));
        }

        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.ClassNotFound);
        }

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

        var distribution = command.Distribution
            .GroupBy(d => d.Level)
            .Select(g => new { Level = g.Key, Count = g.Sum(x => x.Count) })
            .ToList();
        var selection = await QuestionBankMatrixSelector.SelectAsync(
            context,
            new QuestionBankMatrixSelectionRequest
            {
                ProgramId = command.ProgramId,
                QuestionType = HomeworkQuestionType.MultipleChoice,
                Skill = command.Skills,
                Topic = command.Topic,
                ShuffleQuestions = true,
                Distribution = distribution
                    .Select(x => new QuestionBankMatrixLevelCount
                    {
                        Level = x.Level,
                        Count = x.Count
                    })
                    .ToList()
            },
            cancellationToken);

        if (selection.InsufficientLevel.HasValue)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(
                HomeworkErrors.InsufficientQuestionsInBank(
                    selection.InsufficientLevel.Value,
                    selection.RequiredCount,
                    selection.AvailableCount));
        }

        var selected = selection.SelectedItems;

        var dueAtUtc = VietnamTime.NormalizeToUtc(command.DueAt);
        if (startDateUtc.HasValue && dueAtUtc.HasValue && startDateUtc.Value > dueAtUtc.Value)
        {
            return Result.Failure<CreateMultipleChoiceHomeworkResponse>(Error.Validation(
                "Homework.InvalidStartDate",
                "Start date must be before or equal to due date"));
        }

        var now = VietnamTime.UtcNow();
        var normalizedSkills = HomeworkDeliveryMetadata.NormalizeSkills(command.Skills, command.AttachmentUrl);

        var maxScore = selected.Sum(q => q.Points);

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
            CreatedBy = userContext.UserId,
            CreatedAt = now
        };

        context.HomeworkAssignments.Add(homework);

        var questionDtos = new List<Kidzgo.Application.Homework.CreateMultipleChoiceHomework.HomeworkQuestionDto>();
        for (int i = 0; i < selected.Count; i++)
        {
            var bank = selected[i];
            var options = bank.Options == null
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(bank.Options) ?? new List<string>();

            var question = new HomeworkQuestion
            {
                Id = Guid.NewGuid(),
                HomeworkAssignmentId = homework.Id,
                OrderIndex = i,
                QuestionText = bank.QuestionText,
                QuestionType = bank.QuestionType,
                Options = bank.Options,
                CorrectAnswer = bank.QuestionType == HomeworkQuestionType.MultipleChoice
                    ? QuizOptionUtils.NormalizeCorrectAnswerForStorage(options, bank.CorrectAnswer) ?? bank.CorrectAnswer
                    : bank.CorrectAnswer?.Trim(),
                Points = bank.Points,
                Explanation = bank.Explanation
            };

            context.HomeworkQuestions.Add(question);

            questionDtos.Add(new Kidzgo.Application.Homework.CreateMultipleChoiceHomework.HomeworkQuestionDto
            {
                Id = question.Id,
                OrderIndex = i,
                QuestionText = question.QuestionText,
                QuestionType = question.QuestionType.ToString(),
                Options = options,
                Points = question.Points
            });
        }

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
