using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;

namespace Kidzgo.Application.ProgramProgressions.BulkApproveProgramProgressionAssessments;

public sealed class BulkApproveProgramProgressionAssessmentsCommandHandler(
    IUserContext userContext,
    ProgramProgressionApprovalService approvalService)
    : ICommandHandler<BulkApproveProgramProgressionAssessmentsCommand, BulkApproveProgramProgressionAssessmentsResponse>
{
    public async Task<Result<BulkApproveProgramProgressionAssessmentsResponse>> Handle(
        BulkApproveProgramProgressionAssessmentsCommand command,
        CancellationToken cancellationToken)
    {
        var response = new BulkApproveProgramProgressionAssessmentsResponse();
        var approvedCount = 0;
        var skippedCount = 0;

        foreach (var item in command.Items)
        {
            var result = await approvalService.ApproveAsync(
                item.AssessmentId,
                item.TuitionPlanId,
                item.ApprovalNote,
                userContext.UserId,
                cancellationToken);

            if (result.IsFailure)
            {
                skippedCount++;
                response.Errors.Add(new BulkApproveProgramProgressionAssessmentError
                {
                    AssessmentId = item.AssessmentId,
                    ErrorCode = result.Error.Code,
                    ErrorDescription = result.Error.Description
                });
                continue;
            }

            approvedCount++;
            response.Results.Add(new BulkApproveProgramProgressionAssessmentResult
            {
                AssessmentId = item.AssessmentId,
                GeneratedRegistrationId = result.Value.GeneratedRegistrationId
            });
        }

        return Result.Success(new BulkApproveProgramProgressionAssessmentsResponse
        {
            ApprovedCount = approvedCount,
            SkippedCount = skippedCount,
            Results = response.Results,
            Errors = response.Errors
        });
    }
}
