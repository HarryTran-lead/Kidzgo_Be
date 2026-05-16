using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.Assessments.GetAssessmentsByStudent;

public sealed class GetAssessmentsByStudentQueryHandler(IDbContext context)
    : IQueryHandler<GetAssessmentsByStudentQuery, GetAssessmentsByStudentResponse>
{
    public async Task<Result<GetAssessmentsByStudentResponse>> Handle(GetAssessmentsByStudentQuery query, CancellationToken cancellationToken)
    {
        var items = await context.Assessments
            .AsNoTracking()
            .Include(x => x.Module)
            .Where(x => x.StudentProfileId == query.StudentProfileId)
            .OrderByDescending(x => x.AssessedAt)
            .Select(x => new AssessmentDto
            {
                Id = x.Id,
                StudentProfileId = x.StudentProfileId,
                ModuleId = x.ModuleId,
                ModuleCode = x.Module.Code,
                Type = x.Type,
                Score = x.Score,
                Result = x.Result.ToString(),
                TeacherComment = x.TeacherComment,
                AssessedBy = x.AssessedBy,
                AssessedAt = x.AssessedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetAssessmentsByStudentResponse { Items = items });
    }
}
