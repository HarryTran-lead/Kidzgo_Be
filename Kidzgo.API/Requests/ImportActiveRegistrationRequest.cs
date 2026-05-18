namespace Kidzgo.API.Requests;

public sealed class ImportActiveRegistrationRequest
{
    public Guid StudentProfileId { get; set; }
    public Guid BranchId { get; set; }
    public Guid ProgramId { get; set; }
    public Guid LevelId { get; set; }
    public Guid TuitionPlanId { get; set; }
    public DateTime? ExpectedStartDate { get; set; }
    public DateTime ActualStartDate { get; set; }
    public string? PreferredSchedule { get; set; }
    public string? Note { get; set; }
    public int UsedSessions { get; set; }
    public int RemainingSessions { get; set; }
}
