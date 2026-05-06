using FluentValidation;

namespace Kidzgo.Application.ProgramProgressions.BulkApproveProgramProgressionAssessments;

public sealed class BulkApproveProgramProgressionAssessmentsCommandValidator
    : AbstractValidator<BulkApproveProgramProgressionAssessmentsCommand>
{
    public BulkApproveProgramProgressionAssessmentsCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.AssessmentId).NotEmpty();
            });
    }
}
