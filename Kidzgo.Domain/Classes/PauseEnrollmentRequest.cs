using Kidzgo.Domain.Common;
using Kidzgo.Domain.Users;

namespace Kidzgo.Domain.Classes;

public class PauseEnrollmentRequest : Entity
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public DateOnly PauseFrom { get; set; }
    public DateOnly PauseTo { get; set; }
    public string? Reason { get; set; }
    public PauseEnrollmentRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public PauseEnrollmentOutcome? Outcome { get; set; }
    public string? OutcomeNote { get; set; }
    public Guid? OutcomeBy { get; set; }
    public DateTime? OutcomeAt { get; set; }
    public Guid? ReassignedClassId { get; set; }
    public Guid? ReassignedEnrollmentId { get; set; }
    public Guid? OutcomeCompletedBy { get; set; }
    public DateTime? OutcomeCompletedAt { get; set; }
    public int ReservedSessionCount { get; set; }
    public DateOnly? ReservationExpiresOn { get; set; }
    public DateTime? ReservationSnapshotAt { get; set; }

    public Profile StudentProfile { get; set; } = null!;
    public Class? Class { get; set; }
    public Class? ReassignedClass { get; set; }
    public ClassEnrollment? ReassignedEnrollment { get; set; }
    public User? ApprovedByUser { get; set; }
    public User? CancelledByUser { get; set; }
    public User? OutcomeByUser { get; set; }
    public User? OutcomeCompletedByUser { get; set; }
}
