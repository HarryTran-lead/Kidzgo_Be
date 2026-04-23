using Kidzgo.Domain.Homework;

namespace Kidzgo.Application.QuestionBank;

public sealed class QuestionBankItemDto
{
    public Guid Id { get; init; }
    public Guid ProgramId { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
    public List<string> Options { get; init; } = new();
    public string? CorrectAnswer { get; init; }
    public int Points { get; init; }
    public string? Explanation { get; init; }
    public string? Topic { get; init; }
    public string? Skill { get; init; }
    public List<string> GrammarTags { get; init; } = new();
    public List<string> VocabularyTags { get; init; } = new();
    public QuestionLevel Level { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
