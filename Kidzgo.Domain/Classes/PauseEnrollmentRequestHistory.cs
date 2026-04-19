using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Classes;

public class PauseEnrollmentRequestHistory : Entity
{
    public Guid Id { get; set; }
    public Guid PauseEnrollmentRequestId { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid ClassId { get; set; }
    public Guid? EnrollmentId { get; set; }
    public EnrollmentStatus PreviousStatus { get; set; }
    public EnrollmentStatus NewStatus { get; set; }
    public DateOnly PauseFrom { get; set; }
    public DateOnly PauseTo { get; set; }
    public int ReservedSessionCount { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid? ChangedBy { get; set; }

    public PauseEnrollmentRequest PauseEnrollmentRequest { get; set; } = null!;
    public Profile StudentProfile { get; set; } = null!;
    public Class Class { get; set; } = null!;
    public ClassEnrollment? Enrollment { get; set; }
    public User? ChangedByUser { get; set; }
}
