using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetSyllabusDocument;

public sealed class GetSyllabusDocumentQueryHandler(IDbContext context)
    : IQueryHandler<GetSyllabusDocumentQuery, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(
        GetSyllabusDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted, cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.NotFound(query.Id));
        }

        var units = await context.SyllabusUnits
            .AsNoTracking()
            .Where(x => x.SyllabusId == query.Id)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);

        var lessons = await context.SyllabusLessons
            .AsNoTracking()
            .Where(x => x.SyllabusId == query.Id)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);

        var resources = await context.SyllabusResources
            .AsNoTracking()
            .Where(x => x.SyllabusId == query.Id)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(cancellationToken);

        var sections = SyllabusDocumentMapper.ReadSections(syllabus, units, lessons, resources);
        var warnings = SyllabusDocumentMapper.ReadWarnings(syllabus);

        return SyllabusDocumentMapper.ToResponseFromSections(
            syllabus,
            sections,
            warnings,
            fallbackTotalUnits: units.Count,
            fallbackTotalLessons: syllabus.TotalLessons ?? lessons.Count,
            fallbackTotalPeriods: syllabus.TotalPeriods ?? lessons.Select(x => x.PeriodTo ?? x.PeriodFrom ?? 0).DefaultIfEmpty(0).Max());
    }
}
