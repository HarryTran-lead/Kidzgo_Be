using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.LearningTickets;

namespace Kidzgo.Domain.Programs;

public class TuitionPlan : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid? ModuleId { get; set; }
    public string Name { get; set; } = null!;
    public int TotalSessions { get; set; }
    public decimal TuitionAmount { get; set; }
    public decimal UnitPriceSession { get; set; }
    public string Currency { get; set; } = null!;
    public Guid? LearningTicketTypeId { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Program Program { get; set; } = null!;
    public Level Level { get; set; } = null!;
    public Module? Module { get; set; }
    public LearningTicketType? LearningTicketType { get; set; }
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<PackageCurriculumMapping> CurriculumMappings { get; set; } = new List<PackageCurriculumMapping>();
}
