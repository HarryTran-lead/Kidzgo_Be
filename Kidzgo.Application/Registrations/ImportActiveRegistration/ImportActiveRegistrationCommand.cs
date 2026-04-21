using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.ImportActiveRegistration;

public sealed class ImportActiveRegistrationCommand : ICommand<ImportActiveRegistrationResponse>
{
    public Guid StudentProfileId { get; init; }
    public Guid BranchId { get; init; }
    public Guid ProgramId { get; init; }
    public Guid TuitionPlanId { get; init; }
    public DateTime? ExpectedStartDate { get; init; }
    public DateTime ActualStartDate { get; init; }
    public string? PreferredSchedule { get; init; }
    public string? Note { get; init; }
    public int UsedSessions { get; init; }
    public int RemainingSessions { get; init; }
}
