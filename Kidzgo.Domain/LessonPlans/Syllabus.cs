using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.LessonPlans;

public class Syllabus : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public string Code { get; set; } = null!;
    public int Version { get; set; }
    public string Title { get; set; } = null!;
    public string? Edition { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? PacingSchemeJson { get; set; }
    public string? Overview { get; set; }
    public string? OverallObjectives { get; set; }
    public string? SpecificObjectives { get; set; }
    public string? EthicsAndAttitudes { get; set; }
    public string? BookOverview { get; set; }
    public int? TotalPeriods { get; set; }
    public int? MinutesPerPeriod { get; set; }
    public int? TotalLessons { get; set; }
    public string? SourceFileName { get; set; }
    public string? AttachmentUrl { get; set; }
    public string? RawContentJson { get; set; }
    public string DocumentStatus { get; set; } = null!;
    public string SourceType { get; set; } = null!;
    public string? ParserVersion { get; set; }
    public int DocumentVersion { get; set; }
    public string? SectionsJson { get; set; }
    public string? WarningsJson { get; set; }
    public string? ArchiveReason { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Program Program { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public User? CreatedByUser { get; set; }
    public ICollection<SyllabusUnit> Units { get; set; } = new List<SyllabusUnit>();
    public ICollection<SyllabusLesson> Lessons { get; set; } = new List<SyllabusLesson>();
    public ICollection<SyllabusResource> Resources { get; set; } = new List<SyllabusResource>();
    public ICollection<SessionTemplate> SessionTemplates { get; set; } = new List<SessionTemplate>();
    public ICollection<Classes.Class> Classes { get; set; } = new List<Classes.Class>();
}
