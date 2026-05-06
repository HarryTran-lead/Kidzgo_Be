using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.GetProgramProgressionAssessmentById;

public sealed class GetProgramProgressionAssessmentByIdQueryHandler(
    IDbContext context)
    : IQueryHandler<GetProgramProgressionAssessmentByIdQuery, ProgramProgressionAssessmentDto>
{
    public async Task<Result<ProgramProgressionAssessmentDto>> Handle(
        GetProgramProgressionAssessmentByIdQuery query,
        CancellationToken cancellationToken)
    {
        var assessment = await ProgramProgressionAssessmentReadQuery.Build(context)
            .FirstOrDefaultAsync(a => a.Id == query.Id, cancellationToken);

        if (assessment is null)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(
                ProgramProgressionErrors.AssessmentNotFound(query.Id));
        }

        return Result.Success(assessment.ToDto());
    }
}
