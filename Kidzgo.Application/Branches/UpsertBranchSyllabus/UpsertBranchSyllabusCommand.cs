using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Branches.UpsertBranchSyllabus;

public sealed class UpsertBranchSyllabusCommand : ICommand<UpsertBranchSyllabusResponse>
{
    public Guid BranchId { get; init; }
    public Guid SyllabusId { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
}

