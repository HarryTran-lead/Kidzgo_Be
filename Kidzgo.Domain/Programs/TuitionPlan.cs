using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;
using Kidzgo.Domain.Classes;

namespace Kidzgo.Domain.Programs;

public class TuitionPlan : Entity
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string Name { get; set; } = null!;
    public int TotalSessions { get; set; }
    public decimal TuitionAmount { get; set; }
    public decimal UnitPriceSession { get; set; }
    public string Currency { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Program Program { get; set; } = null!;
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
}
