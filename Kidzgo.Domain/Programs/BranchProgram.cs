using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Programs;

public class BranchProgram : Entity
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public bool IsActive { get; set; }
    public Guid? DefaultMakeupClassId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Branch Branch { get; set; } = null!;
    public Program Program { get; set; } = null!;
    public Class? DefaultMakeupClass { get; set; }
}
