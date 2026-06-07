using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetCurriculumTree;

public sealed class GetProgramCurriculumTreeQueryHandler(IDbContext context)
    : IQueryHandler<GetProgramCurriculumTreeQuery, GetProgramCurriculumTreeResponse>
{
    private const string UnmappedUnitKey = "UNMAPPED";
    private const string UnmappedUnitName = "Unmapped";

    public async Task<Result<GetProgramCurriculumTreeResponse>> Handle(
        GetProgramCurriculumTreeQuery query,
        CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .AsNoTracking()
            .Where(x => x.Id == query.ProgramId && !x.IsDeleted)
            .Select(x => new ProgramProjection
            {
                ProgramId = x.Id,
                ProgramName = x.Name,
                ProgramCode = x.Code,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (program is null)
        {
            return Result.Failure<GetProgramCurriculumTreeResponse>(ProgramErrors.NotFound(query.ProgramId));
        }

        var levels = await context.Levels
            .AsNoTracking()
            .Where(x => x.ProgramId == query.ProgramId)
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Select(x => new LevelProjection
            {
                LevelId = x.Id,
                LevelCode = x.Code,
                LevelName = x.Name,
                LevelOrderIndex = x.Order,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var modules = await context.Modules
            .AsNoTracking()
            .Where(x => x.Level.ProgramId == query.ProgramId)
            .OrderBy(x => x.Level.Order)
            .ThenBy(x => x.Order)
            .ThenBy(x => x.Name)
            .Select(x => new ModuleProjection
            {
                ModuleId = x.Id,
                LevelId = x.LevelId,
                ModuleCode = x.Code,
                ModuleName = x.Name,
                ModuleOrderIndex = x.Order,
                ModuleType = x.Type,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var syllabuses = await context.Syllabuses
            .AsNoTracking()
            .Where(x => x.ProgramId == query.ProgramId && !x.IsDeleted)
            .OrderBy(x => x.Level.Order)
            .ThenByDescending(x => x.IsActive)
            .ThenByDescending(x => x.Version)
            .ThenBy(x => x.Title)
            .Select(x => new SyllabusProjection
            {
                SyllabusId = x.Id,
                LevelId = x.LevelId,
                SyllabusCode = x.Code,
                Version = x.Version,
                SyllabusTitle = x.Title,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        var syllabusUnits = await context.SyllabusUnits
            .AsNoTracking()
            .Where(x => x.ModuleId.HasValue &&
                        !x.Syllabus.IsDeleted &&
                        x.Syllabus.ProgramId == query.ProgramId)
            .Select(x => new SyllabusUnitProjection
            {
                SyllabusId = x.SyllabusId,
                ModuleId = x.ModuleId!.Value,
                UnitName = x.Name,
                UnitOrderIndex = x.OrderIndex
            })
            .ToListAsync(cancellationToken);

        var lessonTemplates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Syllabus.ProgramId == query.ProgramId)
            .Select(x => new LessonTemplateProjection
            {
                LessonTemplateId = x.Id,
                SyllabusId = x.SyllabusId,
                ModuleId = x.ModuleId,
                LessonPlanUnitId = x.LessonPlanUnitId,
                LessonPlanUnitName = x.LessonPlanUnit != null ? x.LessonPlanUnit.Name : null,
                LessonPlanUnitOrderIndex = x.LessonPlanUnit != null ? x.LessonPlanUnit.OrderIndex : null,
                SessionTemplateId = x.SessionTemplateId,
                Title = x.Title,
                SessionTitle = x.SessionTemplate != null ? x.SessionTemplate.Title : null,
                SyllabusMetadata = x.SyllabusMetadata,
                SourceFileName = x.SourceFileName,
                SessionIndex = x.SessionIndex,
                SessionOrder = x.SessionOrder,
                OrderIndexInUnit = x.OrderIndexInUnit,
                SessionIndexInModule = x.SessionTemplate != null ? x.SessionTemplate.SessionIndexInModule : null,
                IsActive = x.IsActive,
                ModuleType = x.Module.Type
            })
            .ToListAsync(cancellationToken);

        var syllabusesByLevel = syllabuses
            .GroupBy(x => x.LevelId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var syllabusUnitsByModule = syllabusUnits
            .GroupBy(x => x.ModuleId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var lessonTemplatesByModule = lessonTemplates
            .GroupBy(x => x.ModuleId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var modulesByLevel = modules
            .GroupBy(x => x.LevelId)
            .ToDictionary(x => x.Key, x => x.ToList());

        var levelDtos = levels
            .Select(level =>
            {
                syllabusesByLevel.TryGetValue(level.LevelId, out var levelSyllabuses);
                modulesByLevel.TryGetValue(level.LevelId, out var levelModules);

                var moduleDtos = (levelModules ?? [])
                    .Select(module =>
                    {
                        syllabusUnitsByModule.TryGetValue(module.ModuleId, out var moduleSyllabusUnits);
                        lessonTemplatesByModule.TryGetValue(module.ModuleId, out var moduleLessonTemplates);

                        return new ProgramCurriculumTreeModuleDto
                        {
                            ModuleId = module.ModuleId,
                            ModuleCode = module.ModuleCode,
                            ModuleName = module.ModuleName,
                            ModuleOrderIndex = module.ModuleOrderIndex,
                            ModuleType = module.ModuleType.ToString(),
                            IsActive = module.IsActive,
                            Units = BuildUnits(
                                levelSyllabuses ?? [],
                                moduleSyllabusUnits ?? [],
                                moduleLessonTemplates ?? [])
                        };
                    })
                    .ToList();

                return new ProgramCurriculumTreeLevelDto
                {
                    LevelId = level.LevelId,
                    LevelCode = level.LevelCode,
                    LevelName = level.LevelName,
                    LevelOrderIndex = level.LevelOrderIndex,
                    IsActive = level.IsActive,
                    Modules = moduleDtos
                };
            })
            .ToList();

        return new GetProgramCurriculumTreeResponse
        {
            ProgramId = program.ProgramId,
            ProgramName = program.ProgramName,
            ProgramCode = program.ProgramCode,
            IsActive = program.IsActive,
            Levels = levelDtos
        };
    }

    private static IReadOnlyList<ProgramCurriculumTreeUnitDto> BuildUnits(
        IReadOnlyList<SyllabusProjection> levelSyllabuses,
        IReadOnlyList<SyllabusUnitProjection> moduleSyllabusUnits,
        IReadOnlyList<LessonTemplateProjection> moduleLessonTemplates)
    {
        var buckets = new Dictionary<string, UnitBucket>(StringComparer.OrdinalIgnoreCase);

        foreach (var syllabusUnit in moduleSyllabusUnits)
        {
            var identity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(syllabusUnit.UnitName);
            var unitKey = ResolveNamedUnitKey(syllabusUnit.UnitName);
            var bucket = GetOrCreateBucket(buckets, unitKey);

            bucket.AbsorbSyllabusUnit(
                syllabusUnit.SyllabusId,
                syllabusUnit.UnitName,
                syllabusUnit.UnitOrderIndex,
                identity);
        }

        foreach (var lessonTemplate in moduleLessonTemplates)
        {
            var unitKey = ResolveTemplateUnitKey(lessonTemplate);
            var bucket = GetOrCreateBucket(buckets, unitKey);
            var identity = lessonTemplate.LessonPlanUnitName is null
                ? null
                : LessonPlanUnitNameNormalizer.ExtractUnitIdentity(lessonTemplate.LessonPlanUnitName);

            bucket.AbsorbLessonTemplate(
                lessonTemplate.SyllabusId,
                lessonTemplate.LessonPlanUnitId,
                lessonTemplate.LessonPlanUnitName,
                lessonTemplate.LessonPlanUnitOrderIndex,
                identity);
        }

        return buckets.Values
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.UnitName, StringComparer.OrdinalIgnoreCase)
            .Select(bucket => new ProgramCurriculumTreeUnitDto
            {
                UnitId = bucket.UnitId,
                UnitKey = bucket.UnitKey,
                UnitName = bucket.UnitName,
                UnitNumber = bucket.UnitNumber,
                UnitTitle = bucket.UnitTitle,
                UnitOrderIndex = bucket.OutputOrderIndex,
                IsSynthetic = bucket.IsSynthetic,
                Syllabuses = levelSyllabuses
                    .Where(x => bucket.SyllabusIds.Contains(x.SyllabusId))
                    .OrderByDescending(x => x.IsActive)
                    .ThenByDescending(x => x.Version)
                    .ThenBy(x => x.SyllabusTitle)
                    .Select(syllabus => new ProgramCurriculumTreeSyllabusDto
                    {
                        SyllabusId = syllabus.SyllabusId,
                        SyllabusCode = syllabus.SyllabusCode,
                        Version = syllabus.Version,
                        SyllabusTitle = syllabus.SyllabusTitle,
                        IsActive = syllabus.IsActive,
                        LessonTemplates = moduleLessonTemplates
                            .Where(x => x.SyllabusId == syllabus.SyllabusId &&
                                        ResolveTemplateUnitKey(x) == bucket.UnitKey)
                            .OrderBy(x => x.OrderIndexInUnit)
                            .ThenBy(x => x.SessionOrder)
                            .ThenBy(x => x.SessionIndex)
                            .ThenBy(x => x.Title)
                            .Select(x => new ProgramCurriculumTreeLessonTemplateDto
                            {
                                LessonTemplateId = x.LessonTemplateId,
                                SessionTemplateId = x.SessionTemplateId,
                                Title = x.Title ?? x.SessionTitle ?? x.SourceFileName,
                                LessonType = InferLessonType(x),
                                SessionIndex = x.SessionIndex,
                                SessionOrder = x.SessionOrder,
                                SessionIndexInModule = x.SessionIndexInModule,
                                OrderIndex = x.OrderIndexInUnit,
                                IsActive = x.IsActive
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();
    }

    private static UnitBucket GetOrCreateBucket(IDictionary<string, UnitBucket> buckets, string unitKey)
    {
        if (buckets.TryGetValue(unitKey, out var existing))
        {
            return existing;
        }

        var created = new UnitBucket(unitKey);
        buckets[unitKey] = created;
        return created;
    }

    private static string ResolveTemplateUnitKey(LessonTemplateProjection template)
    {
        if (!template.LessonPlanUnitId.HasValue || string.IsNullOrWhiteSpace(template.LessonPlanUnitName))
        {
            return UnmappedUnitKey;
        }

        return ResolveNamedUnitKey(template.LessonPlanUnitName);
    }

    private static string ResolveNamedUnitKey(string? unitName)
    {
        var key = LessonPlanUnitNameNormalizer.Normalize(unitName);
        return string.IsNullOrWhiteSpace(key)
            ? UnmappedUnitKey
            : key;
    }

    private static string InferLessonType(LessonTemplateProjection template)
    {
        var candidates = new[]
        {
            template.Title,
            template.SessionTitle,
            template.SyllabusMetadata,
            template.SourceFileName
        };

        if (ContainsKeyword(candidates, "assessment"))
        {
            return "Assessment";
        }

        if (template.ModuleType == ModuleType.Test)
        {
            return "Assessment";
        }

        if (ContainsKeyword(candidates, "revision"))
        {
            return "Revision";
        }

        if (template.ModuleType == ModuleType.Revision)
        {
            return "Revision";
        }

        if (LessonPlanUnitNameNormalizer.ExtractLessonNumber(candidates) is not null ||
            ContainsKeyword(candidates, "lesson"))
        {
            return "Lesson";
        }

        return "Other";
    }

    private static bool ContainsKeyword(IEnumerable<string?> values, string keyword)
    {
        return values.Any(value =>
            !string.IsNullOrWhiteSpace(value) &&
            value.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class ProgramProjection
    {
        public Guid ProgramId { get; init; }
        public string ProgramName { get; init; } = null!;
        public string ProgramCode { get; init; } = null!;
        public bool IsActive { get; init; }
    }

    private sealed class LevelProjection
    {
        public Guid LevelId { get; init; }
        public string LevelCode { get; init; } = null!;
        public string LevelName { get; init; } = null!;
        public int LevelOrderIndex { get; init; }
        public bool IsActive { get; init; }
    }

    private sealed class ModuleProjection
    {
        public Guid ModuleId { get; init; }
        public Guid LevelId { get; init; }
        public string ModuleCode { get; init; } = null!;
        public string ModuleName { get; init; } = null!;
        public int ModuleOrderIndex { get; init; }
        public ModuleType ModuleType { get; init; }
        public bool IsActive { get; init; }
    }

    private sealed class SyllabusProjection
    {
        public Guid SyllabusId { get; init; }
        public Guid LevelId { get; init; }
        public string SyllabusCode { get; init; } = null!;
        public int Version { get; init; }
        public string SyllabusTitle { get; init; } = null!;
        public bool IsActive { get; init; }
    }

    private sealed class SyllabusUnitProjection
    {
        public Guid SyllabusId { get; init; }
        public Guid ModuleId { get; init; }
        public string UnitName { get; init; } = null!;
        public int UnitOrderIndex { get; init; }
    }

    private sealed class LessonTemplateProjection
    {
        public Guid LessonTemplateId { get; init; }
        public Guid SyllabusId { get; init; }
        public Guid ModuleId { get; init; }
        public Guid? LessonPlanUnitId { get; init; }
        public string? LessonPlanUnitName { get; init; }
        public int? LessonPlanUnitOrderIndex { get; init; }
        public Guid? SessionTemplateId { get; init; }
        public string? Title { get; init; }
        public string? SessionTitle { get; init; }
        public string? SyllabusMetadata { get; init; }
        public string? SourceFileName { get; init; }
        public int SessionIndex { get; init; }
        public int SessionOrder { get; init; }
        public int OrderIndexInUnit { get; init; }
        public int? SessionIndexInModule { get; init; }
        public bool IsActive { get; init; }
        public ModuleType ModuleType { get; init; }
    }

    private sealed class UnitBucket(string unitKey)
    {
        private int? _orderIndex;

        public string UnitKey { get; } = unitKey;
        public Guid? UnitId { get; private set; }
        public string UnitName { get; private set; } = unitKey == UnmappedUnitKey ? UnmappedUnitName : unitKey;
        public int? UnitNumber { get; private set; }
        public string? UnitTitle { get; private set; }
        public HashSet<Guid> SyllabusIds { get; } = [];
        public bool IsSynthetic => UnitKey == UnmappedUnitKey || !UnitId.HasValue;
        public int SortOrder => _orderIndex ?? int.MaxValue;
        public int OutputOrderIndex => _orderIndex ?? 0;

        public void AbsorbSyllabusUnit(
            Guid syllabusId,
            string unitName,
            int orderIndex,
            LessonPlanUnitIdentity? identity)
        {
            SyllabusIds.Add(syllabusId);
            SetDisplay(unitName, identity);
            SetOrderIndex(orderIndex);
        }

        public void AbsorbLessonTemplate(
            Guid syllabusId,
            Guid? unitId,
            string? unitName,
            int? orderIndex,
            LessonPlanUnitIdentity? identity)
        {
            SyllabusIds.Add(syllabusId);

            if (unitId.HasValue)
            {
                UnitId ??= unitId;
            }

            if (!string.IsNullOrWhiteSpace(unitName))
            {
                SetDisplay(unitName!, identity);
            }

            if (orderIndex.HasValue)
            {
                SetOrderIndex(orderIndex.Value);
            }
        }

        private void SetDisplay(string unitName, LessonPlanUnitIdentity? identity)
        {
            if (UnitKey == UnmappedUnitKey)
            {
                UnitName = UnmappedUnitName;
                UnitNumber = null;
                UnitTitle = null;
                return;
            }

            if (identity is not null)
            {
                UnitName = identity.CanonicalDisplayName;
                UnitNumber ??= identity.UnitNumber;
                UnitTitle ??= identity.UnitTitle;
                return;
            }

            if (UnitName == UnitKey || string.IsNullOrWhiteSpace(UnitName))
            {
                UnitName = unitName;
            }
        }

        private void SetOrderIndex(int orderIndex)
        {
            _orderIndex = _orderIndex.HasValue
                ? Math.Min(_orderIndex.Value, orderIndex)
                : orderIndex;
        }
    }
}
