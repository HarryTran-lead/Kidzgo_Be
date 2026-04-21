namespace Kidzgo.Application.Registrations.ImportActiveRegistration;

public sealed class ImportActiveRegistrationResponse
{
    public Guid Id { get; init; }
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public string ProgramName { get; init; } = null!;
    public Guid TuitionPlanId { get; init; }
    public string TuitionPlanName { get; init; } = null!;
    public DateTime RegistrationDate { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public DateTime ActualStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public string Status { get; init; } = null!;
    public int TotalSessions { get; init; }
    public int UsedSessions { get; init; }
    public int RemainingSessions { get; init; }
    public DateTime CreatedAt { get; init; }
}
