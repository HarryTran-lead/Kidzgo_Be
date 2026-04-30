namespace Kidzgo.Application.PauseEnrollmentRequests;

public sealed class PauseEnrollmentRequestResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public string Scope { get; set; } = PauseEnrollmentRequestScopeHelper.AllEligible;
    public DateOnly PauseFrom { get; set; }
    public DateOnly PauseTo { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? Outcome { get; set; }
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
    public List<PauseEnrollmentClassDto> Classes { get; set; } = new();
}
