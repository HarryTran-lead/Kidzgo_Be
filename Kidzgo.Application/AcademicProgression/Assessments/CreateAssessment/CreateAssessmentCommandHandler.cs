using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Assessments.CreateAssessment;

public sealed class CreateAssessmentCommandHandler(
    IDbContext context,
    IUserContext userContext,
    AssessmentService assessmentService)
    : ICommandHandler<CreateAssessmentCommand, AssessmentDto>
{
    public async Task<Result<AssessmentDto>> Handle(CreateAssessmentCommand command, CancellationToken cancellationToken)
    {
        var result = await assessmentService.CreateAssessmentAsync(
            command.StudentProfileId,
            command.ModuleId,
            command.SessionId,
            command.Type,
            command.Score,
            command.TeacherComment,
            userContext.UserId,
            command.AssessedAt,
            cancellationToken);
        if (result.IsFailure)
        {
            return Result.Failure<AssessmentDto>(result.Error);
        }

        var assessment = await context.Assessments
            .AsNoTracking()
            .Include(x => x.Module)
            .FirstAsync(x => x.Id == result.Value.Id, cancellationToken);

        return Result.Success(new AssessmentDto
        {
            Id = assessment.Id,
            StudentProfileId = assessment.StudentProfileId,
            ModuleId = assessment.ModuleId,
            ModuleCode = assessment.Module.Code,
            Type = assessment.Type,
            Score = assessment.Score,
            Result = assessment.Result.ToString(),
            TeacherComment = assessment.TeacherComment,
            AssessedBy = assessment.AssessedBy,
            AssessedAt = assessment.AssessedAt
        });
    }
}
