namespace Kidzgo.Application.Students.Shared;

public sealed class StudentBranchStateDto
{
    public Guid StudentProfileId { get; init; }
    public Guid HomeBranchId { get; init; }
    public string HomeBranchName { get; init; } = null!;
    public Guid ActiveBranchId { get; init; }
    public string ActiveBranchName { get; init; } = null!;
    public bool AllowCrossBranchEnrollment { get; init; }
    public DateTime? LastTransferredAt { get; init; }
    public IReadOnlyList<StudentBranchTransferDto> Transfers { get; init; } = Array.Empty<StudentBranchTransferDto>();
}

public sealed class StudentBranchTransferDto
{
    public Guid Id { get; init; }
    public Guid FromBranchId { get; init; }
    public string FromBranchName { get; init; } = null!;
    public Guid ToBranchId { get; init; }
    public string ToBranchName { get; init; } = null!;
    public DateOnly EffectiveDate { get; init; }
    public string? Reason { get; init; }
    public bool KeepCurrentClass { get; init; }
    public bool AllowCrossBranchEnrollment { get; init; }
    public DateTime CreatedAt { get; init; }
}
