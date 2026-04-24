using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.QuestionBank;

internal static class QuestionBankItemMappings
{
    public static QuestionBankItemDto ToDto(this QuestionBankItem entity)
    {
        return new QuestionBankItemDto
        {
            Id = entity.Id,
            ProgramId = entity.ProgramId,
            QuestionText = entity.QuestionText,
            QuestionType = entity.QuestionType.ToString(),
            Options = QuizOptionUtils.ParseOptions(entity.Options),
            CorrectAnswer = entity.CorrectAnswer,
            Points = entity.Points,
            Explanation = entity.Explanation,
            ImageUrls = StringListJson.Deserialize(entity.ImageUrls),
            VideoUrls = StringListJson.Deserialize(entity.VideoUrls),
            AudioUrls = StringListJson.Deserialize(entity.AudioUrls),
            Topic = entity.Topic,
            Skill = entity.Skill,
            GrammarTags = StringListJson.Deserialize(entity.GrammarTags),
            VocabularyTags = StringListJson.Deserialize(entity.VocabularyTags),
            Level = entity.Level,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
