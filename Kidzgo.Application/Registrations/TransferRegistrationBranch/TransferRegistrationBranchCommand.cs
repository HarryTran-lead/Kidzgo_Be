using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Services;

namespace Kidzgo.Application.Registrations.TransferRegistrationBranch;

public sealed class TransferRegistrationBranchCommand : ICommand<TransferRegistrationBranchResponse>
{
    public Guid RegistrationId { get; init; }
    public Guid NewBranchId { get; init; }
    public Guid? NewClassId { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyCollection<WeeklyPatternEntry>? WeeklyPattern { get; init; }
}
