using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.Homework;

public class QuestionBankItem : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string QuestionText { get; set; } = null!;
    public HomeworkQuestionType QuestionType { get; set; }
    public string? Options { get; set; } // JSON array for multiple choice options
    public string? CorrectAnswer { get; set; } // Correct answer (for multiple choice: option text, legacy data may store option index)
    public int Points { get; set; }
    public string? Explanation { get; set; }
    public string? ImageUrls { get; set; }
    public string? VideoUrls { get; set; }
    public string? AudioUrls { get; set; }
    public string? Topic { get; set; }
    public string? Skill { get; set; }
    public string? GrammarTags { get; set; }
    public string? VocabularyTags { get; set; }
    public QuestionLevel Level { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Program Program { get; set; } = null!;
}

