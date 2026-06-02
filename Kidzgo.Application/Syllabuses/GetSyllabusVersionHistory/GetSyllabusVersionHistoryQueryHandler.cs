using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetSyllabusVersionHistory;

public sealed class GetSyllabusVersionHistoryQueryHandler(IDbContext context)
    : IQueryHandler<GetSyllabusVersionHistoryQuery, GetSyllabusVersionHistoryResponse>
{
    public async Task<Result<GetSyllabusVersionHistoryResponse>> Handle(
        GetSyllabusVersionHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Include(x => x.Program)
            .Include(x => x.Level)
            .FirstOrDefaultAsync(x => x.Id == query.SyllabusId && !x.IsDeleted, cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<GetSyllabusVersionHistoryResponse>(SyllabusErrors.NotFound(query.SyllabusId));
        }

        var versions = await context.Syllabuses
            .AsNoTracking()
            .Where(x => !x.IsDeleted &&
                        x.ProgramId == syllabus.ProgramId &&
                        x.LevelId == syllabus.LevelId &&
                        x.Code == syllabus.Code)
            .OrderByDescending(x => x.IsActive)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => new SyllabusVersionHistoryItemDto
            {
                SyllabusId = x.Id,
                Version = x.Version,
                Title = x.Title,
                Edition = x.Edition,
                DocumentStatus = x.DocumentStatus,
                IsActive = x.IsActive,
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetSyllabusVersionHistoryResponse
        {
            SyllabusId = syllabus.Id,
            Code = syllabus.Code,
            ProgramId = syllabus.ProgramId,
            ProgramName = syllabus.Program.Name,
            LevelId = syllabus.LevelId,
            LevelName = syllabus.Level.Name,
            Versions = versions
        };
    }
}
