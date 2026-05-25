using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.GetSyllabusUnitLessonPlans;

public sealed class GetSyllabusUnitLessonPlansQueryHandler(IDbContext context)
    : IQueryHandler<GetSyllabusUnitLessonPlansQuery, GetSyllabusUnitLessonPlansResponse>
{
    public async Task<Result<GetSyllabusUnitLessonPlansResponse>> Handle(
        GetSyllabusUnitLessonPlansQuery query,
        CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Where(x => x.Id == query.SyllabusId && !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.ProgramId,
                ProgramName = x.Program.Name,
                x.LevelId,
                LevelName = x.Level.Name
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<GetSyllabusUnitLessonPlansResponse>(
                SyllabusErrors.NotFound(query.SyllabusId));
        }

        var templates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(t => !t.IsDeleted &&
                        t.SessionTemplate != null &&
                        t.SessionTemplate.SyllabusId == query.SyllabusId)
            .Select(t => new LessonPlanTemplateProjection
            {
                LessonPlanTemplateId = t.Id,
                LessonPlanUnitId = t.LessonPlanUnitId,
                LessonPlanUnitName = t.LessonPlanUnit != null ? t.LessonPlanUnit.Name : null,
                LessonPlanUnitOrder = t.LessonPlanUnit != null ? t.LessonPlanUnit.OrderIndex : null,
                SessionTemplateId = t.SessionTemplateId,
                Title = t.Title,
                SyllabusMetadata = t.SyllabusMetadata,
                SourceFileName = t.SourceFileName,
                SessionIndex = t.SessionIndex,
                SessionOrder = t.SessionOrder,
                OrderIndexInUnit = t.OrderIndexInUnit,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ModuleId = t.ModuleId,
                ModuleCode = t.Module.Code,
                ModuleName = t.Module.Name,
                ModuleOrder = t.Module.Order,
                SessionIndexInModule = t.SessionTemplate != null ? t.SessionTemplate.SessionIndexInModule : null,
                SessionTitle = t.SessionTemplate != null ? t.SessionTemplate.Title : null,
                SessionTopic = t.SessionTemplate != null ? t.SessionTemplate.Topic : null
            })
            .ToListAsync(cancellationToken);

        var orphanLessons = templates
            .Where(x => !x.LessonPlanUnitId.HasValue)
            .OrderBy(x => x.ModuleOrder)
            .ThenBy(x => x.SessionOrder)
            .ThenBy(x => x.SessionIndex)
            .Select(ToLessonDto)
            .ToList();

        var groups = templates
            .Where(x => x.LessonPlanUnitId.HasValue)
            .GroupBy(x => new
            {
                x.ModuleId,
                x.ModuleCode,
                x.ModuleName,
                x.ModuleOrder
            })
            .OrderBy(g => g.Key.ModuleOrder)
            .Select(moduleGroup =>
            {
                var units = moduleGroup
                    .GroupBy(x => new
                    {
                        UnitId = x.LessonPlanUnitId!.Value,
                        UnitName = x.LessonPlanUnitName ?? "Unmapped",
                        UnitOrder = x.LessonPlanUnitOrder ?? int.MaxValue
                    })
                    .OrderBy(g => g.Key.UnitOrder)
                    .ThenBy(g => g.Key.UnitName)
                    .Select(unitGroup =>
                    {
                        var unitIdentity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(unitGroup.Key.UnitName);
                        var lessons = unitGroup
                            .OrderBy(x => x.OrderIndexInUnit)
                            .ThenBy(x => x.SessionOrder)
                            .ThenBy(x => x.SessionIndex)
                            .ThenBy(x => x.Title)
                            .Select(ToLessonDto)
                            .ToList();

                        return new SyllabusLessonPlanUnitDto
                        {
                            UnitId = unitGroup.Key.UnitId,
                            UnitName = unitGroup.Key.UnitName,
                            OrderIndex = unitGroup.Key.UnitOrder == int.MaxValue ? 0 : unitGroup.Key.UnitOrder,
                            UnitOrderIndex = unitGroup.Key.UnitOrder == int.MaxValue ? 0 : unitGroup.Key.UnitOrder,
                            UnitNumber = unitIdentity?.UnitNumber,
                            UnitTitle = unitIdentity?.UnitTitle,
                            LessonPlanCount = lessons.Count,
                            Lessons = lessons
                        };
                    })
                    .ToList();

                return new SyllabusModuleUnitLessonPlanGroupDto
                {
                    ModuleId = moduleGroup.Key.ModuleId,
                    ModuleCode = moduleGroup.Key.ModuleCode,
                    ModuleName = moduleGroup.Key.ModuleName,
                    ModuleOrder = moduleGroup.Key.ModuleOrder,
                    ModuleOrderIndex = moduleGroup.Key.ModuleOrder,
                    UnitCount = units.Count,
                    LessonPlanCount = units.Sum(x => x.LessonPlanCount),
                    Units = units
                };
            })
            .ToList();

        return new GetSyllabusUnitLessonPlansResponse
        {
            SyllabusId = syllabus.Id,
            ProgramId = syllabus.ProgramId,
            ProgramName = syllabus.ProgramName,
            LevelId = syllabus.LevelId,
            LevelName = syllabus.LevelName,
            TotalModules = groups.Count,
            TotalUnits = groups.Sum(x => x.UnitCount),
            TotalGroups = groups.Count,
            TotalLessonPlans = groups.Sum(x => x.LessonPlanCount) + orphanLessons.Count,
            Groups = groups,
            OrphanLessons = orphanLessons
        };
    }

    private static SyllabusUnitLessonPlanDto ToLessonDto(LessonPlanTemplateProjection template)
    {
        var unitIdentity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(template.LessonPlanUnitName);

        return new SyllabusUnitLessonPlanDto
        {
            LessonPlanTemplateId = template.LessonPlanTemplateId,
            ModuleId = template.ModuleId,
            ModuleOrderIndex = template.ModuleOrder,
            LessonPlanUnitId = template.LessonPlanUnitId,
            UnitId = template.LessonPlanUnitId,
            UnitOrderIndex = template.LessonPlanUnitOrder,
            UnitNumber = unitIdentity?.UnitNumber,
            UnitTitle = unitIdentity?.UnitTitle,
            SessionTemplateId = template.SessionTemplateId,
            Title = template.Title,
            LessonNumber = LessonPlanUnitNameNormalizer.ExtractLessonNumber(template.SourceFileName, template.Title),
            SessionIndex = template.SessionIndex,
            SessionOrder = template.SessionOrder,
            SessionIndexInModule = template.SessionIndexInModule,
            SessionTitle = template.SessionTitle,
            SessionTopic = template.SessionTopic,
            SourceFileName = template.SourceFileName,
            OrderIndexInUnit = template.OrderIndexInUnit,
            LessonOrderIndexInUnit = template.OrderIndexInUnit,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    private sealed class LessonPlanTemplateProjection
    {
        public Guid LessonPlanTemplateId { get; init; }
        public Guid? LessonPlanUnitId { get; init; }
        public string? LessonPlanUnitName { get; init; }
        public int? LessonPlanUnitOrder { get; init; }
        public Guid? SessionTemplateId { get; init; }
        public string? Title { get; init; }
        public string? SyllabusMetadata { get; init; }
        public string? SourceFileName { get; init; }
        public int SessionIndex { get; init; }
        public int SessionOrder { get; init; }
        public int OrderIndexInUnit { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public Guid ModuleId { get; init; }
        public string ModuleCode { get; init; } = null!;
        public string ModuleName { get; init; } = null!;
        public int ModuleOrder { get; init; }
        public int? SessionIndexInModule { get; init; }
        public string? SessionTitle { get; init; }
        public string? SessionTopic { get; init; }
    }
}
