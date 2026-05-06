using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessmentById;

public sealed class GetProgramProgressionAssessmentByIdQuery : IQuery<ProgramProgressionAssessmentDto>
{
    public Guid Id { get; init; }
}
