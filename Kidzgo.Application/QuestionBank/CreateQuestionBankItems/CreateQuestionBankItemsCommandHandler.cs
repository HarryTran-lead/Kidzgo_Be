using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.QuestionBank;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.CreateQuestionBankItems;

public sealed class CreateQuestionBankItemsCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<CreateQuestionBankItemsCommand, CreateQuestionBankItemsResponse>
{
    public async Task<Result<CreateQuestionBankItemsResponse>> Handle(
        CreateQuestionBankItemsCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Items == null || command.Items.Count == 0)
        {
            return Result.Failure<CreateQuestionBankItemsResponse>(HomeworkErrors.NoQuestionsProvided);
        }

        var programExists = await context.Programs
            .AnyAsync(p => p.Id == command.ProgramId && !p.IsDeleted, cancellationToken);

        if (!programExists)
        {
            return Result.Failure<CreateQuestionBankItemsResponse>(
                HomeworkErrors.ProgramNotFound(command.ProgramId));
        }

        var now = VietnamTime.UtcNow();
        var createdBy = userContext.UserId;

        var items = new List<QuestionBankItem>();
        var dtoItems = new List<QuestionBankItemDto>();
        var normalizedCorrectAnswers = new List<string>(command.Items.Count);

        for (int i = 0; i < command.Items.Count; i++)
        {
            var item = command.Items[i];

            if (string.IsNullOrWhiteSpace(item.QuestionText))
            {
                return Result.Failure<CreateQuestionBankItemsResponse>(
                    HomeworkErrors.InvalidQuestionText(i + 1));
            }

            if (item.QuestionType == HomeworkQuestionType.MultipleChoice)
            {
                if (item.Options == null || item.Options.Count < 2)
                {
                    return Result.Failure<CreateQuestionBankItemsResponse>(
                        HomeworkErrors.InsufficientOptions(i + 1));
                }

                var normalizedCorrectAnswer = QuizOptionUtils.NormalizeCorrectAnswerForStorage(
                    item.Options,
                    item.CorrectAnswer);

                if (string.IsNullOrWhiteSpace(normalizedCorrectAnswer))
                {
                    return Result.Failure<CreateQuestionBankItemsResponse>(
                        HomeworkErrors.InvalidCorrectAnswer(i + 1));
                }

                normalizedCorrectAnswers.Add(normalizedCorrectAnswer);
            }
            else
            {
                normalizedCorrectAnswers.Add(item.CorrectAnswer.Trim());
            }

            if (item.Points <= 0)
            {
                return Result.Failure<CreateQuestionBankItemsResponse>(
                    HomeworkErrors.InvalidPoints(i + 1));
            }

            var entity = new QuestionBankItem
            {
                Id = Guid.NewGuid(),
                ProgramId = command.ProgramId,
                QuestionText = item.QuestionText,
                QuestionType = item.QuestionType,
                Options = item.QuestionType == HomeworkQuestionType.MultipleChoice
                    ? JsonSerializer.Serialize(item.Options)
                    : null,
                CorrectAnswer = normalizedCorrectAnswers[i],
                Points = item.Points,
                Explanation = item.Explanation,
                ImageUrls = StringListJson.Serialize(item.ImageUrls),
                VideoUrls = StringListJson.Serialize(item.VideoUrls),
                AudioUrls = StringListJson.Serialize(item.AudioUrls),
                Topic = item.Topic,
                Skill = item.Skill,
                GrammarTags = StringListJson.Serialize(item.GrammarTags),
                VocabularyTags = StringListJson.Serialize(item.VocabularyTags),
                Level = item.Level,
                CreatedBy = createdBy,
                CreatedAt = now
            };

            items.Add(entity);
        }

        context.QuestionBankItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        foreach (var entity in items)
        {
            dtoItems.Add(entity.ToDto());
        }

        return new CreateQuestionBankItemsResponse
        {
            Items = dtoItems
        };
    }
}
