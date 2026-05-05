using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.UpdateHomeworkAssignment;

public sealed class UpdateHomeworkAssignmentCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateHomeworkAssignmentCommand, UpdateHomeworkAssignmentResponse>
{
    public async Task<Result<UpdateHomeworkAssignmentResponse>> Handle(
        UpdateHomeworkAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        var homework = await context.HomeworkAssignments
            .Include(h => h.HomeworkStudents)
            .FirstOrDefaultAsync(h => h.Id == command.Id, cancellationToken);

        if (homework is null)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.NotFound(command.Id));
        }

        // Check if there are any submitted or graded submissions
        bool hasSubmittedOrGraded = homework.HomeworkStudents
            .Any(hs => hs.Status == HomeworkStatus.Submitted || hs.Status == HomeworkStatus.Graded);

        if (hasSubmittedOrGraded)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.CannotUpdate);
        }

        // Validate mission if provided
        if (command.MissionId.HasValue)
        {
            var mission = await context.Missions
                .FirstOrDefaultAsync(m => m.Id == command.MissionId.Value, cancellationToken);

            if (mission is null)
            {
                return Result.Failure<UpdateHomeworkAssignmentResponse>(
                    HomeworkErrors.MissionNotFound(command.MissionId));
            }
        }

        // Validate due date
        if (command.DueAt.HasValue && VietnamTime.NormalizeToUtc(command.DueAt.Value) <= VietnamTime.UtcNow())
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidDueDate);
        }

        var dueAtUtc = VietnamTime.NormalizeToUtc(command.DueAt);

        // Validate Title if provided
        if (command.Title != null && string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTitle);
        }

        // Validate MaxScore if provided
        if (command.MaxScore.HasValue && command.MaxScore.Value <= 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidMaxScore);
        }

        // Validate RewardStars if provided
        if (command.RewardStars.HasValue && command.RewardStars.Value < 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidRewardStars);
        }

        if (command.TimeLimitMinutes.HasValue && command.TimeLimitMinutes.Value <= 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTimeLimitMinutes);
        }

        if (command.MaxAttempts.HasValue && command.MaxAttempts.Value <= 0)
        {
            return Result.Failure<UpdateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidMaxAttempts);
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(command.Title))
        {
            homework.Title = command.Title;
        }

        if (command.Description != null)
        {
            homework.Description = command.Description;
        }

        if (command.DueAt.HasValue)
        {
            homework.DueAt = dueAtUtc;
        }

        if (command.Book != null)
        {
            homework.Book = command.Book;
        }

        if (command.Pages != null)
        {
            homework.Pages = command.Pages;
        }

        if (command.Skills != null)
        {
            homework.Skills = command.Skills;
        }

        if (command.Topic != null)
        {
            homework.Topic = command.Topic;
        }

        if (command.GrammarTags != null)
        {
            homework.GrammarTags = StringListJson.Serialize(command.GrammarTags);
        }

        if (command.VocabularyTags != null)
        {
            homework.VocabularyTags = StringListJson.Serialize(command.VocabularyTags);
        }

        if (command.SubmissionType.HasValue)
        {
            homework.SubmissionType = command.SubmissionType.Value;
        }

        if (command.MaxScore.HasValue)
        {
            homework.MaxScore = command.MaxScore.Value;
        }

        if (command.RewardStars.HasValue)
        {
            homework.RewardStars = command.RewardStars;
        }

        if (command.TimeLimitMinutes.HasValue)
        {
            homework.TimeLimitMinutes = command.TimeLimitMinutes;
        }

        if (command.MaxAttempts.HasValue)
        {
            homework.MaxAttempts = command.MaxAttempts.Value;
        }

        if (command.AiHintEnabled.HasValue)
        {
            homework.AiHintEnabled = command.AiHintEnabled.Value;
        }

        if (command.AiRecommendEnabled.HasValue)
        {
            homework.AiRecommendEnabled = command.AiRecommendEnabled.Value;
        }

        if (command.MissionId.HasValue)
        {
            homework.MissionId = command.MissionId;
        }

        if (command.Instructions != null)
        {
            homework.Instructions = command.Instructions;
        }

        if (command.ExpectedAnswer != null)
        {
            homework.ExpectedAnswer = command.ExpectedAnswer;
        }

        if (command.Rubric != null)
        {
            homework.Rubric = command.Rubric;
        }

        if (command.SpeakingMode != null)
        {
            homework.SpeakingMode = command.SpeakingMode;
        }

        if (command.TargetWords != null)
        {
            homework.TargetWords = StringListJson.Serialize(command.TargetWords);
        }

        if (command.SpeakingExpectedText != null)
        {
            homework.SpeakingExpectedText = command.SpeakingExpectedText;
        }

        if (command.AttachmentUrl != null)
        {
            homework.AttachmentUrl = command.AttachmentUrl;
        }

        // Update questions if provided
        if (command.Questions != null)
        {
            var existingQuestions = await context.HomeworkQuestions
                .Where(q => q.HomeworkAssignmentId == homework.Id)
                .ToListAsync(cancellationToken);

            var incomingIds = command.Questions
                .Where(q => q.Id.HasValue)
                .Select(q => q.Id!.Value)
                .ToHashSet();

            // Remove questions not in the new list
            var toRemove = existingQuestions.Where(q => !incomingIds.Contains(q.Id)).ToList();
            context.HomeworkQuestions.RemoveRange(toRemove);

            var existingById = existingQuestions.ToDictionary(q => q.Id);

            for (var i = 0; i < command.Questions.Count; i++)
            {
                var dto = command.Questions[i];

                string? serializedOptions = dto.Options != null
                    ? JsonSerializer.Serialize(dto.Options)
                    : null;

                string? normalizedCorrectAnswer = null;
                if (dto.QuestionType == HomeworkQuestionType.MultipleChoice && dto.Options != null)
                {
                    normalizedCorrectAnswer = QuizOptionUtils.NormalizeCorrectAnswerForStorage(
                        dto.Options, dto.CorrectAnswer);
                }
                else
                {
                    normalizedCorrectAnswer = dto.CorrectAnswer;
                }

                if (dto.Id.HasValue && existingById.TryGetValue(dto.Id.Value, out var existing))
                {
                    existing.OrderIndex = i;
                    existing.QuestionText = dto.QuestionText;
                    existing.QuestionType = dto.QuestionType;
                    existing.Options = serializedOptions;
                    existing.CorrectAnswer = normalizedCorrectAnswer;
                    existing.Points = dto.Points;
                    existing.Explanation = dto.Explanation;
                }
                else
                {
                    context.HomeworkQuestions.Add(new HomeworkQuestion
                    {
                        Id = Guid.NewGuid(),
                        HomeworkAssignmentId = homework.Id,
                        OrderIndex = i,
                        QuestionText = dto.QuestionText,
                        QuestionType = dto.QuestionType,
                        Options = serializedOptions,
                        CorrectAnswer = normalizedCorrectAnswer,
                        Points = dto.Points,
                        Explanation = dto.Explanation
                    });
                }
            }
        }

        // Update LATE status for submissions if due date changed
        if (command.DueAt.HasValue && command.DueAt.Value < homework.DueAt)
        {
            var now = VietnamTime.UtcNow();
            var lateSubmissions = homework.HomeworkStudents
                .Where(hs => hs.Status == HomeworkStatus.Assigned && 
                            homework.DueAt.HasValue && 
                            now > homework.DueAt.Value)
                .ToList();

            foreach (var submission in lateSubmissions)
            {
                submission.Status = HomeworkStatus.Late;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateHomeworkAssignmentResponse
        {
            Id = homework.Id,
            Title = homework.Title,
            DueAt = homework.DueAt
        };
    }
}

