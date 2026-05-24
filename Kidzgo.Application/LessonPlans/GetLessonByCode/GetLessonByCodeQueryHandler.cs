using System.Text.RegularExpressions;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlans.GetLessonByCode;

public sealed class GetLessonByCodeQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetLessonByCodeQuery, GetLessonByCodeResponse>
{
    public async Task<Result<GetLessonByCodeResponse>> Handle(
        GetLessonByCodeQuery query,
        CancellationToken cancellationToken)
    {
        var normalizedRequestedCode = NormalizeCode(query.LessonCode);

        var templates = await context.LessonPlanTemplates
            .AsNoTracking()
            .Include(x => x.Module)
                .ThenInclude(x => x.Level)
                    .ThenInclude(x => x.Program)
            .Include(x => x.LessonPlanUnit)
            .Include(x => x.SessionTemplate)
                .ThenInclude(x => x!.Syllabus)
            .Include(x => x.Materials)
            .Include(x => x.Activities)
            .Include(x => x.HomeworkTemplates)
            .Where(x => !x.IsDeleted && x.IsActive)
            .ToListAsync(cancellationToken);

        var template = templates.FirstOrDefault(x => NormalizeCode(BuildLessonCode(x)) == normalizedRequestedCode);
        if (template is null)
        {
            return Result.Failure<GetLessonByCodeResponse>(
                Error.NotFound("Lesson.NotFound", $"Lesson with code '{query.LessonCode}' was not found"));
        }

        var currentUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (currentUser is null)
        {
            return Result.Failure<GetLessonByCodeResponse>(LessonPlanTemplateErrors.Unauthorized);
        }

        if (currentUser.Role == UserRole.Teacher)
        {
            var canAccessTemplate = await context.Sessions
                .AnyAsync(s =>
                        (s.PlannedTeacherId == currentUser.Id ||
                         s.ActualTeacherId == currentUser.Id ||
                         s.Class.MainTeacherId == currentUser.Id ||
                         s.Class.AssistantTeacherId == currentUser.Id) &&
                        (s.LessonPlanTemplateId == template.Id ||
                         (s.LessonPlan != null && s.LessonPlan.TemplateId == template.Id) ||
                         (s.TeachingLog != null &&
                          (s.TeachingLog.PlannedLessonPlanTemplateId == template.Id ||
                           s.TeachingLog.ActualLessonPlanTemplateId == template.Id)) ||
                         s.SessionLessons.Any(sl => sl.LessonPlanTemplateId == template.Id)),
                    cancellationToken);

            if (!canAccessTemplate)
            {
                return Result.Failure<GetLessonByCodeResponse>(LessonPlanTemplateErrors.Unauthorized);
            }
        }

        var unitCode = BuildUnitCode(template.LessonPlanUnit?.Name ?? template.Title);
        var lessonNo = Math.Max(1, template.OrderIndexInUnit + 1);
        var type = unitCode.StartsWith("revision_", StringComparison.OrdinalIgnoreCase) ? "revision" : "lesson";
        var teacherMaterials = template.Materials
            .Where(x => string.Equals(x.MaterialType, "teacher", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        var studentMaterials = template.Materials
            .Where(x => string.Equals(x.MaterialType, "student", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.OrderIndex)
            .Select(x => x.Name)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        return Result.Success(new GetLessonByCodeResponse
        {
            CourseCode = BuildCourseCode(template),
            UnitCode = unitCode,
            LessonCode = BuildLessonCode(template),
            Type = type,
            Title = template.Title ?? template.LessonPlanUnit?.Name ?? query.LessonCode,
            LessonNo = lessonNo,
            Objectives = SplitTextItems(template.Objectives),
            LanguageContent = new LessonLanguageContentDto
            {
                Vocabulary = SplitTextItems(template.Vocabulary),
                Grammar = SplitTextItems(template.Grammar)
            },
            Materials = new LessonMaterialsDto
            {
                Teacher = teacherMaterials.Count > 0 ? teacherMaterials : SplitTextItems(template.TeacherMaterials),
                Students = studentMaterials.Count > 0 ? studentMaterials : SplitTextItems(template.StudentMaterials)
            },
            Procedure = BuildProcedure(template),
            Homework = template.HomeworkTemplates.Count > 0
                ? template.HomeworkTemplates.OrderBy(x => x.OrderIndex).Select(x => x.Instructions ?? x.Title).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()!
                : [],
            Evaluation = template.Evaluation?.Trim() ?? string.Empty,
            SourceFileUrl = template.AttachmentUrl
        });
    }

    private static string BuildLessonCode(LessonPlanTemplate template)
    {
        return $"{BuildUnitCode(template.LessonPlanUnit?.Name ?? template.Title)}_lesson_{Math.Max(1, template.OrderIndexInUnit + 1)}";
    }

    private static string BuildUnitCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "lesson";
        }

        var value = raw.Trim();
        if (Regex.IsMatch(value, @"\bREVISION\s*0*(\d+)\b", RegexOptions.IgnoreCase))
        {
            var number = Regex.Match(value, @"\bREVISION\s*0*(\d+)\b", RegexOptions.IgnoreCase).Groups[1].Value;
            return $"revision_{int.Parse(number)}";
        }

        if (value.Contains("starter", StringComparison.OrdinalIgnoreCase))
        {
            return "unit_starter";
        }

        var match = Regex.Match(value, @"\bUNIT\s*0*(\d+)\b", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return $"unit_{int.Parse(match.Groups[1].Value)}";
        }

        return NormalizeCode(value);
    }

    private static string BuildCourseCode(LessonPlanTemplate template)
    {
        var source = template.SessionTemplate?.Syllabus?.Code
                     ?? template.SessionTemplate?.Syllabus?.Title
                     ?? template.Module.Level.Program.Name;
        return NormalizeCode(source);
    }

    private static string NormalizeCode(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9]+", "_");
        return Regex.Replace(slug, @"_+", "_").Trim('_');
    }

    private static IReadOnlyList<string> SplitTextItems(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(line => Regex.Replace(line, @"^\s*[-+*•\d\.\)]*\s*", string.Empty).Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
    }

    private static IReadOnlyList<LessonProcedureStageDto> BuildProcedure(LessonPlanTemplate template)
    {
        if (template.Activities.Count > 0)
        {
            return template.Activities
                .OrderBy(x => x.OrderIndex)
                .Select((activity, index) => new LessonProcedureStageDto
                {
                    StageNo = index + 1,
                    Stage = string.IsNullOrWhiteSpace(activity.Title) ? $"Stage {index + 1}" : activity.Title!,
                    Details = SplitTextItems(activity.TeacherActivity)
                })
                .ToList();
        }

        var details = SplitTextItems(template.Procedure);
        if (details.Count == 0)
        {
            return [];
        }

        return
        [
            new LessonProcedureStageDto
            {
                StageNo = 1,
                Stage = "Procedure",
                Details = details
            }
        ];
    }
}
