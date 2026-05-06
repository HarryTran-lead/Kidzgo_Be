using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.ApproveProgramProgressionAssessment;

public sealed class ApproveProgramProgressionAssessmentCommandHandler(
    IDbContext context,
    IUserContext userContext,
    ProgramProgressionApprovalService approvalService)
    : ICommandHandler<ApproveProgramProgressionAssessmentCommand, ProgramProgressionAssessmentDto>
{
    public async Task<Result<ProgramProgressionAssessmentDto>> Handle(
        ApproveProgramProgressionAssessmentCommand command,
        CancellationToken cancellationToken)
    {
        var approvalResult = await approvalService.ApproveAsync(
            command.AssessmentId,
            command.TuitionPlanId,
            command.ApprovalNote,
            userContext.UserId,
            cancellationToken);

        if (approvalResult.IsFailure)
        {
            return Result.Failure<ProgramProgressionAssessmentDto>(approvalResult.Error);
        }

        var assessment = await ProgramProgressionAssessmentReadQuery.Build(context)
            .FirstAsync(a => a.Id == command.AssessmentId, cancellationToken);

        return Result.Success(assessment.ToDto());
    }
}
