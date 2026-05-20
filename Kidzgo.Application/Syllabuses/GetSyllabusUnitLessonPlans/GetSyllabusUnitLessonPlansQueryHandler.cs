using System.Text.RegularExpressions;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
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
                SessionTemplateId = t.SessionTemplateId,
                Title = t.Title,
                SyllabusMetadata = t.SyllabusMetadata,
                SourceFileName = t.SourceFileName,
                SessionIndex = t.SessionIndex,
                SessionOrder = t.SessionOrder,
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

        var groups = templates
            .Select(x => new
            {
                Template = x,
                Group = ResolveGroup(x.SourceFileName, x.Title, x.SyllabusMetadata)
            })
            .GroupBy(x => new
            {
                x.Group.GroupKey,
                x.Group.GroupType,
                x.Group.UnitNumber,
                x.Group.RevisionNumber,
                x.Group.DisplayName,
                x.Template.ModuleId,
                x.Template.ModuleCode,
                x.Template.ModuleName,
                x.Template.ModuleOrder
            })
            .OrderBy(g => GetGroupOrder(g.Key.GroupType, g.Key.UnitNumber, g.Key.RevisionNumber))
            .ThenBy(g => g.Key.ModuleOrder)
            .Select(g => new SyllabusUnitLessonPlanGroupDto
            {
                GroupKey = g.Key.GroupKey,
                GroupType = g.Key.GroupType,
                UnitNumber = g.Key.UnitNumber,
                RevisionNumber = g.Key.RevisionNumber,
                DisplayName = g.Key.DisplayName,
                ModuleId = g.Key.ModuleId,
                ModuleCode = g.Key.ModuleCode,
                ModuleName = g.Key.ModuleName,
                ModuleOrder = g.Key.ModuleOrder,
                LessonPlanCount = g.Count(),
                LessonPlans = g
                    .Select(x => new
                    {
                        x.Template,
                        LessonNumber = ExtractLessonNumber(x.Template.SourceFileName, x.Template.Title)
                    })
                    .OrderBy(x => x.LessonNumber ?? int.MaxValue)
                    .ThenBy(x => x.Template.SessionIndex)
                    .ThenBy(x => x.Template.Title)
                    .Select(x => new SyllabusUnitLessonPlanDto
                    {
                        LessonPlanTemplateId = x.Template.LessonPlanTemplateId,
                        SessionTemplateId = x.Template.SessionTemplateId,
                        Title = x.Template.Title,
                        LessonNumber = x.LessonNumber,
                        SessionIndex = x.Template.SessionIndex,
                        SessionOrder = x.Template.SessionOrder,
                        SessionIndexInModule = x.Template.SessionIndexInModule,
                        SessionTitle = x.Template.SessionTitle,
                        SessionTopic = x.Template.SessionTopic,
                        SourceFileName = x.Template.SourceFileName,
                        IsActive = x.Template.IsActive,
                        CreatedAt = x.Template.CreatedAt,
                        UpdatedAt = x.Template.UpdatedAt
                    })
                    .ToList()
            })
            .ToList();

        return new GetSyllabusUnitLessonPlansResponse
        {
            SyllabusId = syllabus.Id,
            ProgramId = syllabus.ProgramId,
            ProgramName = syllabus.ProgramName,
            LevelId = syllabus.LevelId,
            LevelName = syllabus.LevelName,
            TotalGroups = groups.Count,
            TotalLessonPlans = groups.Sum(x => x.LessonPlanCount),
            Groups = groups
        };
    }

    private static UnitLessonPlanGroup ResolveGroup(params string?[] hints)
    {
        var texts = hints
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(Normalize)
            .ToList();

        foreach (var text in texts)
        {
            if (Regex.IsMatch(text, @"\bUNIT\s+STARTER\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                return new UnitLessonPlanGroup("unit-starter", "Unit", 0, null, "Unit Starter");
            }

            var unitMatch = Regex.Match(
                text,
                @"\bUNIT\s*0*(\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (unitMatch.Success && int.TryParse(unitMatch.Groups[1].Value, out var unitNumber))
            {
                return new UnitLessonPlanGroup(
                    $"unit-{unitNumber}",
                    "Unit",
                    unitNumber,
                    null,
                    $"Unit {unitNumber}");
            }

            var revisionMatch = Regex.Match(
                text,
                @"\bREVISION\s*0*(\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            if (revisionMatch.Success && int.TryParse(revisionMatch.Groups[1].Value, out var revisionNumber))
            {
                return new UnitLessonPlanGroup(
                    $"revision-{revisionNumber}",
                    "Revision",
                    null,
                    revisionNumber,
                    $"Revision {revisionNumber}");
            }
        }

        return new UnitLessonPlanGroup("unmapped", "Unmapped", null, null, "Unmapped");
    }

    private static int? ExtractLessonNumber(params string?[] hints)
    {
        foreach (var text in hints.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Normalize))
        {
            var match = Regex.Match(
                text,
                @"\bLESSON\s*0*(\d+)\b",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            if (match.Success && int.TryParse(match.Groups[1].Value, out var lessonNumber))
            {
                return lessonNumber;
            }
        }

        return null;
    }

    private static int GetGroupOrder(string groupType, int? unitNumber, int? revisionNumber)
    {
        return groupType switch
        {
            "Unit" => unitNumber ?? 0,
            "Revision" => 10_000 + (revisionNumber ?? 0),
            _ => int.MaxValue
        };
    }

    private static string Normalize(string? value)
    {
        return Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
    }

    private sealed record UnitLessonPlanGroup(
        string GroupKey,
        string GroupType,
        int? UnitNumber,
        int? RevisionNumber,
        string DisplayName);

    private sealed class LessonPlanTemplateProjection
    {
        public Guid LessonPlanTemplateId { get; init; }
        public Guid? SessionTemplateId { get; init; }
        public string? Title { get; init; }
        public string? SyllabusMetadata { get; init; }
        public string? SourceFileName { get; init; }
        public int SessionIndex { get; init; }
        public int SessionOrder { get; init; }
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
