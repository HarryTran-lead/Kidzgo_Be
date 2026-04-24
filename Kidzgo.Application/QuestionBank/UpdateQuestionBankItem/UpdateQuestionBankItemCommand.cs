using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.QuestionBank;
using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.QuestionBank.UpdateQuestionBankItem;

public sealed class UpdateQuestionBankItemCommand : ICommand<QuestionBankItemDto>
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string QuestionText { get; init; } = null!;
    public HomeworkQuestionType QuestionType { get; init; } = HomeworkQuestionType.MultipleChoice;
    public List<string> Options { get; init; } = new();
    public string CorrectAnswer { get; init; } = null!;
    public int Points { get; init; } = 1;
    public string? Explanation { get; init; }
    public List<string>? ImageUrls { get; init; }
    public List<string>? VideoUrls { get; init; }
    public List<string>? AudioUrls { get; init; }
    public string? Topic { get; init; }
    public string? Skill { get; init; }
    public List<string>? GrammarTags { get; init; }
    public List<string>? VocabularyTags { get; init; }
    public QuestionLevel Level { get; init; }
}
