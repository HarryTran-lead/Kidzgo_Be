using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Gamification;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.CreateHomeworkAssignment;

public sealed class CreateHomeworkAssignmentCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateHomeworkAssignmentCommand, CreateHomeworkAssignmentResponse>
{
    public async Task<Result<CreateHomeworkAssignmentResponse>> Handle(
        CreateHomeworkAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(command.Title))
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTitle);
        }

        // Validate MaxScore
        if (command.MaxScore.HasValue && command.MaxScore.Value <= 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidMaxScore);
        }

        // Validate RewardStars
        if (command.RewardStars.HasValue && command.RewardStars.Value < 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidRewardStars);
        }

        if (command.TimeLimitMinutes.HasValue && command.TimeLimitMinutes.Value <= 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidTimeLimitMinutes);
        }

        if (command.MaxAttempts <= 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidMaxAttempts);
        }

        // Validate class exists
        var classEntity = await context.Classes
            .FirstOrDefaultAsync(c => c.Id == command.ClassId, cancellationToken);

        if (classEntity is null)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.ClassNotFound);
        }

        // Validate session if provided
        if (command.SessionId.HasValue)
        {
            var session = await context.Sessions
                .FirstOrDefaultAsync(s => s.Id == command.SessionId.Value, cancellationToken);

            if (session is null)
            {
                return Result.Failure<CreateHomeworkAssignmentResponse>(
                    HomeworkErrors.SessionNotFound(command.SessionId));
            }

            // Validate session belongs to the class
            if (session.ClassId != command.ClassId)
            {
                return Result.Failure<CreateHomeworkAssignmentResponse>(
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
                return Result.Failure<CreateHomeworkAssignmentResponse>(
                    HomeworkErrors.MissionNotFound(command.MissionId));
            }
        }

        var startDateUtc = VietnamTime.NormalizeToUtc(command.StartDate);

        // Validate due date
        if (command.DueAt.HasValue && VietnamTime.NormalizeToUtc(command.DueAt.Value) <= VietnamTime.UtcNow())
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.InvalidDueDate);
        }

        var dueAtUtc = VietnamTime.NormalizeToUtc(command.DueAt);
        if (startDateUtc.HasValue && dueAtUtc.HasValue && startDateUtc.Value > dueAtUtc.Value)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(Error.Validation(
                "Homework.InvalidStartDate",
                "Start date must be before or equal to due date"));
        }

        // Get current user ID from context
        var currentUserId = userContext.UserId;

        var now = VietnamTime.UtcNow();
        var homework = new HomeworkAssignment
        {
            Id = Guid.NewGuid(),
            ClassId = command.ClassId,
            SessionId = command.SessionId,
            Title = command.Title,
            Description = command.Description,
            StartDate = startDateUtc,
            DueAt = dueAtUtc,
            Book = command.Book,
            Pages = command.Pages,
            Skills = command.Skills,
            Topic = command.Topic,
            GrammarTags = StringListJson.Serialize(command.GrammarTags),
            VocabularyTags = StringListJson.Serialize(command.VocabularyTags),
            SubmissionType = command.SubmissionType,
            MaxScore = command.MaxScore ?? 10,
            RewardStars = command.RewardStars,
            TimeLimitMinutes = command.TimeLimitMinutes,
            MaxAttempts = command.MaxAttempts,
            AiHintEnabled = command.AiHintEnabled ?? false,
            AiRecommendEnabled = command.AiRecommendEnabled ?? false,
            MissionId = command.MissionId,
            Instructions = command.Instructions,
            ExpectedAnswer = command.ExpectedAnswer,
            Rubric = command.Rubric,
            SpeakingMode = command.SpeakingMode,
            TargetWords = StringListJson.Serialize(command.TargetWords),
            SpeakingExpectedText = command.SpeakingExpectedText,
            AttachmentUrl = command.AttachmentUrl,
            CreatedBy = currentUserId,
            CreatedAt = now
        };

        context.HomeworkAssignments.Add(homework);

        // Auto-assign to all active students in the class
        var activeEnrollments = await context.ClassEnrollments
            .Where(e => e.ClassId == command.ClassId && e.Status == EnrollmentStatus.Active)
            .Select(e => e.StudentProfileId)
            .ToListAsync(cancellationToken);

        if (activeEnrollments.Count == 0)
        {
            return Result.Failure<CreateHomeworkAssignmentResponse>(
                HomeworkErrors.ClassHasNoActiveStudents(command.ClassId));
        }

        // If session is specified, it's still assigned to all active students in the class
        // (students who were absent still need to do homework)
        var studentIdsToAssign = activeEnrollments;

        var homeworkStudents = studentIdsToAssign.Select(studentId => new HomeworkStudent
        {
            Id = Guid.NewGuid(),
            AssignmentId = homework.Id,
            StudentProfileId = studentId,
            Status = HomeworkStatus.Assigned
        }).ToList();

        context.HomeworkStudents.AddRange(homeworkStudents);

        await context.SaveChangesAsync(cancellationToken);

        return new CreateHomeworkAssignmentResponse
        {
            Id = homework.Id,
            ClassId = homework.ClassId,
            SessionId = homework.SessionId,
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
            AssignedStudentsCount = homeworkStudents.Count
        };
    }
}

