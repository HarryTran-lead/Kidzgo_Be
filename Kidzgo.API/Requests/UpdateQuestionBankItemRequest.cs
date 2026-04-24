namespace Kidzgo.API.Requests;

public sealed class UpdateQuestionBankItemRequest
{
    public Guid ProgramId { get; init; }
    public string QuestionText { get; init; } = null!;
    public string QuestionType { get; init; } = null!;
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
    public string Level { get; init; } = null!;
}
