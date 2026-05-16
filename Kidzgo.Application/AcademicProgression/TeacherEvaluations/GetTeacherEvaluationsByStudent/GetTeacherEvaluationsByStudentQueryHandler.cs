using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.TeacherEvaluations.GetTeacherEvaluationsByStudent;

public sealed class GetTeacherEvaluationsByStudentQueryHandler(IDbContext context)
    : IQueryHandler<GetTeacherEvaluationsByStudentQuery, GetTeacherEvaluationsByStudentResponse>
{
    public async Task<Result<GetTeacherEvaluationsByStudentResponse>> Handle(GetTeacherEvaluationsByStudentQuery query, CancellationToken cancellationToken)
    {
        var items = await context.TeacherEvaluations
            .AsNoTracking()
            .Include(x => x.Module)
            .Where(x => x.StudentProfileId == query.StudentProfileId)
            .OrderByDescending(x => x.EvaluatedAt)
            .Select(x => new TeacherEvaluationDto
            {
                Id = x.Id,
                StudentProfileId = x.StudentProfileId,
                ModuleId = x.ModuleId,
                ModuleCode = x.Module.Code,
                Speaking = x.Speaking,
                Listening = x.Listening,
                Reading = x.Reading,
                Writing = x.Writing,
                Participation = x.Participation,
                Confidence = x.Confidence,
                Behavior = x.Behavior,
                Notes = x.Notes,
                EvaluatedBy = x.EvaluatedBy,
                EvaluatedAt = x.EvaluatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new GetTeacherEvaluationsByStudentResponse { Items = items });
    }
}
