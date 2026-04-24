using System.Text.Json;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.QuestionBank;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.UpdateQuestionBankItem;

public sealed class UpdateQuestionBankItemCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateQuestionBankItemCommand, QuestionBankItemDto>
{
    public async Task<Result<QuestionBankItemDto>> Handle(
        UpdateQuestionBankItemCommand command,
        CancellationToken cancellationToken)
    {
        var item = await context.QuestionBankItems
            .FirstOrDefaultAsync(q => q.Id == command.Id && !q.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Result.Failure<QuestionBankItemDto>(
                HomeworkErrors.QuestionBankItemNotFound(command.Id));
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<QuestionBankItemDto>(
                HomeworkErrors.ProgramNotFound(command.ProgramId));
        }

        if (string.IsNullOrWhiteSpace(command.QuestionText))
        {
            return Result.Failure<QuestionBankItemDto>(
                HomeworkErrors.InvalidQuestionText(1));
        }

        string? normalizedCorrectAnswer;
        if (command.QuestionType == HomeworkQuestionType.MultipleChoice)
        {
            if (command.Options == null || command.Options.Count < 2)
            {
                return Result.Failure<QuestionBankItemDto>(
                    HomeworkErrors.InsufficientOptions(1));
            }

            normalizedCorrectAnswer = QuizOptionUtils.NormalizeCorrectAnswerForStorage(
                command.Options,
                command.CorrectAnswer);

            if (string.IsNullOrWhiteSpace(normalizedCorrectAnswer))
            {
                return Result.Failure<QuestionBankItemDto>(
                    HomeworkErrors.InvalidCorrectAnswer(1));
            }
        }
        else
        {
            normalizedCorrectAnswer = command.CorrectAnswer?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedCorrectAnswer))
            {
                return Result.Failure<QuestionBankItemDto>(
                    HomeworkErrors.InvalidCorrectAnswer(1));
            }
        }

        if (command.Points <= 0)
        {
            return Result.Failure<QuestionBankItemDto>(
                HomeworkErrors.InvalidPoints(1));
        }

        item.ProgramId = command.ProgramId;
        item.QuestionText = command.QuestionText.Trim();
        item.QuestionType = command.QuestionType;
        item.Options = command.QuestionType == HomeworkQuestionType.MultipleChoice
            ? JsonSerializer.Serialize(command.Options)
            : null;
        item.CorrectAnswer = normalizedCorrectAnswer;
        item.Points = command.Points;
        item.Explanation = string.IsNullOrWhiteSpace(command.Explanation) ? null : command.Explanation.Trim();
        item.ImageUrls = StringListJson.Serialize(command.ImageUrls);
        item.VideoUrls = StringListJson.Serialize(command.VideoUrls);
        item.AudioUrls = StringListJson.Serialize(command.AudioUrls);
        item.Topic = string.IsNullOrWhiteSpace(command.Topic) ? null : command.Topic.Trim();
        item.Skill = string.IsNullOrWhiteSpace(command.Skill) ? null : command.Skill.Trim();
        item.GrammarTags = StringListJson.Serialize(command.GrammarTags);
        item.VocabularyTags = StringListJson.Serialize(command.VocabularyTags);
        item.Level = command.Level;
        item.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return item.ToDto();
    }
}
