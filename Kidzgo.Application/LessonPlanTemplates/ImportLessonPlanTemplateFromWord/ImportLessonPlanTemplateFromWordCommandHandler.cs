using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
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
        var module = await context.Modules
            .Where(x => x.Id == command.ModuleId && x.IsActive)
            .Select(x => new { x.Id, x.Name, x.LevelId, x.PlannedSessionCount })
            .FirstOrDefaultAsync(cancellationToken);

        if (module is null)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.ModuleNotFound(command.ModuleId));
        }

        var parsed = CurriculumWordImportParser.ParseLessonPlanDocx(command.FileStream, command.FileName);
        if (parsed.IsFailure)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(parsed.Error);
        }

        var sessionIndex = parsed.Value.LessonNumber ?? 0;
        if (sessionIndex <= 0 || sessionIndex > module.PlannedSessionCount)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.SessionIndexOutOfRange(sessionIndex, module.PlannedSessionCount));
        }

        var template = await context.LessonPlanTemplates
            .Include(x => x.Activities)
            .Include(x => x.Materials)
            .Include(x => x.HomeworkTemplates)
            .FirstOrDefaultAsync(
                x => x.ModuleId == command.ModuleId &&
                     x.SessionIndex == sessionIndex,
                cancellationToken);

        if (template is not null && !command.OverwriteExisting)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.DuplicateSessionIndex(command.ModuleId, sessionIndex));
        }

        var linkedSessionTemplateId = await context.SessionTemplates
            .Where(x => x.ModuleId == command.ModuleId && x.SessionIndexInModule == sessionIndex && x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .Select(x => (Guid?)x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var now = VietnamTime.UtcNow();
        if (template is null)
        {
            template = new LessonPlanTemplate
            {
                Id = Guid.NewGuid(),
                ModuleId = command.ModuleId,
                CreatedBy = userContext.UserId,
                CreatedAt = now
            };
            context.LessonPlanTemplates.Add(template);
        }
        else
        {
            context.LessonPlanTemplateActivities.RemoveRange(template.Activities);
            context.LessonPlanTemplateMaterials.RemoveRange(template.Materials);
            context.HomeworkTemplates.RemoveRange(template.HomeworkTemplates);
        }

        template.SessionTemplateId = linkedSessionTemplateId;
        template.Title = parsed.Value.Title;
        template.SessionIndex = sessionIndex;
        template.SessionOrder = sessionIndex;
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

        var orderIndex = 1;
        var materials = BuildMaterialTemplates(template.Id, parsed.Value, now, ref orderIndex);
        var homeworks = BuildHomeworkTemplates(template.Id, parsed.Value, now);

        context.LessonPlanTemplateMaterials.AddRange(materials);
        context.HomeworkTemplates.AddRange(homeworks);

        await context.SaveChangesAsync(cancellationToken);

        return new ImportLessonPlanTemplateFromWordResponse
        {
            LessonPlanTemplateId = template.Id,
            SessionTemplateId = linkedSessionTemplateId,
            SessionIndex = template.SessionIndex,
            Title = template.Title
        };
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
