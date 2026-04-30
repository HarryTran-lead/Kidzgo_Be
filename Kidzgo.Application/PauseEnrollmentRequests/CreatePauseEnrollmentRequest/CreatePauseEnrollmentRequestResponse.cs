using Kidzgo.Application.PauseEnrollmentRequests;

namespace Kidzgo.Application.PauseEnrollmentRequests.CreatePauseEnrollmentRequest;

public sealed class CreatePauseEnrollmentRequestResponse
{
    public Guid Id { get; set; }
    public Guid StudentProfileId { get; set; }
    public Guid? ClassId { get; set; }
    public DateOnly PauseFrom { get; set; }
    public DateOnly PauseTo { get; set; }
    public string? Reason { get; set; }
    public string Scope { get; set; } = PauseEnrollmentRequestScopeHelper.AllEligible;
    public string Status { get; set; } = null!;
    public DateTime RequestedAt { get; set; }
    public List<PauseEnrollmentClassDto> Classes { get; set; } = new();
}
