namespace Kidzgo.Application.Registrations.TransferRegistrationBranch;

public sealed class TransferRegistrationBranchResponse
{
    public Guid RegistrationId { get; init; }
    public Guid OldBranchId { get; init; }
    public string OldBranchName { get; init; } = null!;
    public Guid NewBranchId { get; init; }
    public string NewBranchName { get; init; } = null!;
    public Guid? OldClassId { get; init; }
    public string? OldClassName { get; init; }
    public Guid? NewClassId { get; init; }
    public string? NewClassName { get; init; }
    public DateTime EffectiveDate { get; init; }
    public string Status { get; init; } = null!;
    public string EntryType { get; init; } = null!;
    public string? WarningMessage { get; init; }
}
