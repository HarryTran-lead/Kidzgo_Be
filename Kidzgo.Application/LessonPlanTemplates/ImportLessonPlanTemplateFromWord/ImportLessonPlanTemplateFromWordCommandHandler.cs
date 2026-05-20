using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;

public sealed class ImportLessonPlanTemplateFromWordCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<ImportLessonPlanTemplateFromWordCommand, ImportLessonPlanTemplateFromWordResponse>
{
    public async Task<Result<ImportLessonPlanTemplateFromWordResponse>> Handle(
        ImportLessonPlanTemplateFromWordCommand command,
        CancellationToken cancellationToken)
    {
        LessonPlanUnit? requestedUnit = null;
        if (command.LessonPlanUnitId.HasValue)
        {
            requestedUnit = await context.LessonPlanUnits
                .FirstOrDefaultAsync(
                    x => x.Id == command.LessonPlanUnitId.Value && x.IsActive,
                    cancellationToken);

            if (requestedUnit is null)
            {
                return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                    LessonPlanUnitErrors.NotFound(command.LessonPlanUnitId.Value));
            }

            if (command.ModuleId.HasValue && requestedUnit.ModuleId != command.ModuleId.Value)
            {
                return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                    LessonPlanUnitErrors.LessonMustStayInSameModule);
            }
        }

        var moduleId = command.ModuleId ?? requestedUnit?.ModuleId;
        if (!moduleId.HasValue)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(command.ModuleId));
        }

        var module = await context.Modules
            .Where(x => x.Id == moduleId.Value && x.IsActive)
            .Select(x => new { x.Id, x.Name, x.LevelId, x.PlannedSessionCount })
            .FirstOrDefaultAsync(cancellationToken);

        if (module is null)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(moduleId));
        }

        var parsed = CurriculumWordImportParser.ParseLessonPlanDocx(command.FileStream, command.FileName);
        if (parsed.IsFailure)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(parsed.Error);
        }

        var sessionIndex = command.SessionIndexOverride.GetValueOrDefault() > 0
            ? command.SessionIndexOverride!.Value
            : requestedUnit is not null
                ? await ResolveNextSessionIndexInModuleAsync(module.Id, module.PlannedSessionCount, cancellationToken)
                : ResolveSessionIndex(parsed.Value, command.FileName, module.Name, module.PlannedSessionCount);
        if (sessionIndex <= 0 || sessionIndex > module.PlannedSessionCount)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.SessionIndexOutOfRange(sessionIndex, module.PlannedSessionCount));
        }

        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(
                x => x.ModuleId == module.Id &&
                     x.SessionIndex == sessionIndex,
                cancellationToken);

        if (template is not null && !command.OverwriteExisting)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.DuplicateSessionIndex(module.Id, sessionIndex));
        }

        var linkedSessionTemplate = await context.SessionTemplates
            .Where(x => x.ModuleId == module.Id && x.SessionIndexInModule == sessionIndex && x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
        var linkedSessionTemplateId = linkedSessionTemplate?.Id;

        var now = VietnamTime.UtcNow();
        var lessonPlanUnit = requestedUnit;
        if (lessonPlanUnit is null)
        {
            var unitName = LessonPlanUnitNameNormalizer.ExtractUnitName(
                parsed.Value.Title,
                parsed.Value.UnitTitle,
                command.FileName);
            if (!string.IsNullOrWhiteSpace(unitName))
            {
                lessonPlanUnit = await LessonPlanUnitResolver.FindOrCreateAsync(
                    context,
                    module.Id,
                    unitName,
                    now,
                    cancellationToken);
            }
        }

        var created = template is null;
        if (template is null)
        {
            template = new LessonPlanTemplate
            {
                Id = Guid.NewGuid(),
                ModuleId = module.Id,
                CreatedBy = userContext.UserId,
                CreatedAt = now
            };
            context.LessonPlanTemplates.Add(template);
        }
        else
        {
            await context.LessonPlanTemplateActivities
                .Where(x => x.LessonPlanTemplateId == template.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await context.LessonPlanTemplateMaterials
                .Where(x => x.LessonPlanTemplateId == template.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await context.HomeworkTemplates
                .Where(x => x.LessonPlanTemplateId == template.Id)
                .ExecuteDeleteAsync(cancellationToken);
        }

        template.SessionTemplateId = linkedSessionTemplateId;
        template.LessonPlanUnitId = lessonPlanUnit?.Id;
        template.Title = parsed.Value.Title;
        template.SessionIndex = sessionIndex;
        template.SessionOrder = sessionIndex;
        if (lessonPlanUnit is null)
        {
            template.OrderIndexInUnit = 0;
        }
        else
        {
            var lessonNumber = LessonPlanUnitNameNormalizer.ExtractLessonNumber(
                parsed.Value.Title,
                command.FileName,
                parsed.Value.UnitTitle) ?? parsed.Value.LessonNumber;
            template.OrderIndexInUnit = lessonNumber.HasValue
                ? Math.Max(lessonNumber.Value - 1, 0)
                : await LessonPlanUnitResolver.GetNextOrderInUnitAsync(context, lessonPlanUnit.Id, cancellationToken);
        }
        template.SyllabusMetadata = parsed.Value.UnitTitle;
        template.SyllabusContent = parsed.Value.RawText;
        template.Objectives = parsed.Value.Objectives;
        template.LanguageContent = parsed.Value.LanguageContent;
        template.Vocabulary = parsed.Value.Vocabulary;
        template.Grammar = parsed.Value.Grammar;
        template.TeachingMethodology = parsed.Value.TeachingMethodology;
        template.TeacherMaterials = parsed.Value.TeacherMaterials;
        template.StudentMaterials = parsed.Value.StudentMaterials;
        template.Procedure = parsed.Value.Procedure;
        template.Evaluation = parsed.Value.Evaluation;
        template.SourceFileName = command.FileName;
        template.IsActive = true;
        template.IsDeleted = false;
        template.UpdatedAt = now;
        EntityStringLengthTrimmer.TrimToModelLimits(context, template);

        var orderIndex = 1;
        var materials = BuildMaterialTemplates(template.Id, parsed.Value, now, ref orderIndex);
        var homeworks = BuildHomeworkTemplates(template.Id, parsed.Value, now);

        EntityStringLengthTrimmer.TrimToModelLimits(context, materials);
        EntityStringLengthTrimmer.TrimToModelLimits(context, homeworks);

        context.LessonPlanTemplateMaterials.AddRange(materials);
        context.HomeworkTemplates.AddRange(homeworks);

        if (linkedSessionTemplate is not null)
        {
            linkedSessionTemplate.LessonPlanTemplateId = template.Id;
            linkedSessionTemplate.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new ImportLessonPlanTemplateFromWordResponse
        {
            LessonPlanTemplateId = template.Id,
            LessonPlanUnitId = template.LessonPlanUnitId,
            SessionTemplateId = linkedSessionTemplateId,
            SessionIndex = template.SessionIndex,
            SessionOrder = template.SessionOrder,
            OrderIndexInUnit = template.OrderIndexInUnit,
            Created = created,
            Title = template.Title
        };
    }

    private async Task<int> ResolveNextSessionIndexInModuleAsync(
        Guid moduleId,
        int plannedSessionCount,
        CancellationToken cancellationToken)
    {
        var usedSessionIndexes = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => x.ModuleId == moduleId && !x.IsDeleted)
            .Select(x => x.SessionIndex)
            .ToListAsync(cancellationToken);

        for (var index = 1; index <= plannedSessionCount; index++)
        {
            if (!usedSessionIndexes.Contains(index))
            {
                return index;
            }
        }

        return plannedSessionCount + 1;
    }

    private static int ResolveSessionIndex(
        ParsedLessonPlanDocument parsed,
        string fileName,
        string moduleName,
        int plannedSessionCount)
    {
        if (parsed.LessonNumber.HasValue && parsed.LessonNumber.Value > 0)
        {
            return parsed.LessonNumber.Value;
        }

        var combinedText = string.Join(
            " ",
            new[]
            {
                parsed.Title,
                parsed.UnitTitle,
                parsed.ModuleHint,
                fileName,
                moduleName
            }.Where(x => !string.IsNullOrWhiteSpace(x)));

        if (combinedText.Contains("revision", StringComparison.OrdinalIgnoreCase) && plannedSessionCount > 0)
        {
            return 1;
        }

        if (plannedSessionCount == 1)
        {
            return 1;
        }

        return 0;
    }

    private static List<LessonPlanTemplateMaterial> BuildMaterialTemplates(Guid templateId, ParsedLessonPlanDocument parsed, DateTime now, ref int orderIndex)
    {
        var items = new List<LessonPlanTemplateMaterial>();
        if (!string.IsNullOrWhiteSpace(parsed.TeacherMaterials))
        {
            items.Add(new LessonPlanTemplateMaterial
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Name = "Teacher materials",
                MaterialType = "teacher",
                Notes = parsed.TeacherMaterials,
                OrderIndex = orderIndex++,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        if (!string.IsNullOrWhiteSpace(parsed.StudentMaterials))
        {
            items.Add(new LessonPlanTemplateMaterial
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Name = "Student materials",
                MaterialType = "student",
                Notes = parsed.StudentMaterials,
                OrderIndex = orderIndex++,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        return items;
    }

    private static List<HomeworkTemplate> BuildHomeworkTemplates(Guid templateId, ParsedLessonPlanDocument parsed, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(parsed.Homework))
        {
            return [];
        }

        return
        [
            new HomeworkTemplate
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Title = "Homework",
                Instructions = parsed.Homework,
                OrderIndex = 1,
                IsRequired = true,
                CreatedAt = now,
                UpdatedAt = now
            }
        ];
    }
}
