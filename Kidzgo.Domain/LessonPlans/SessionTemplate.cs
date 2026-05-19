using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.LessonPlans;

public class SessionTemplate : Entity
{
    public Guid Id { get; set; }
    public Guid SyllabusId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid? ModuleId { get; set; }
    public Guid? LessonPlanTemplateId { get; set; }
    public int SessionIndex { get; set; }
    public int? SessionIndexInModule { get; set; }
    public int? LessonNumber { get; set; }
    public string? Title { get; set; }
    public string? Topic { get; set; }
    public string? ObjectiveSummary { get; set; }
    public string? VocabularySummary { get; set; }
    public string? GrammarSummary { get; set; }
    public string? ContentSummary { get; set; }
    public string? TeacherNotes { get; set; }
    public int OrderIndex { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Syllabus Syllabus { get; set; } = null!;
    public Program Program { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public Module? Module { get; set; }
    public LessonPlanTemplate? LessonPlanTemplate { get; set; }
    public ICollection<Sessions.ClassSessionLesson> ClassSessionLessons { get; set; } = new List<Sessions.ClassSessionLesson>();
    public ICollection<Sessions.TeachingLogLesson> TeachingLogLessons { get; set; } = new List<Sessions.TeachingLogLesson>();
}
