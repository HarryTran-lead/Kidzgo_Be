using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Domain.Classes;

public class ClassModuleProgress : Entity
{
    public Guid Id { get; set; }
    public Guid ClassId { get; set; }
    public Guid ModuleId { get; set; }
    public int OrderIndex { get; set; }
    public int RequiredSessions { get; set; }
    public int CompletedSessions { get; set; }
    public ClassModuleProgressStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Class Class { get; set; } = null!;
    public Module Module { get; set; } = null!;
}
