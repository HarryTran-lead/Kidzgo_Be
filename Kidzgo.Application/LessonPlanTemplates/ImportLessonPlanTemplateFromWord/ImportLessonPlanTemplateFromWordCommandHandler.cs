using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.LessonPlanTemplates.Shared;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.RegularExpressions;

namespace Kidzgo.Application.LessonPlanTemplates.ImportLessonPlanTemplateFromWord;

public sealed class ImportLessonPlanTemplateFromWordCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<ImportLessonPlanTemplateFromWordCommand, ImportLessonPlanTemplateFromWordResponse>
{
    private const int ActivityTitleMaxLength = 255;
    private const int MaterialNameMaxLength = 255;

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

        var syllabusId = command.SyllabusId;
        if (!syllabusId.HasValue && command.SessionIndexOverride.HasValue && command.SessionIndexOverride.Value > 0)
        {
            syllabusId = await context.SessionTemplates
                .AsNoTracking()
                .Where(x => x.ModuleId == module.Id &&
                            x.SessionIndexInModule == command.SessionIndexOverride.Value &&
                            x.IsActive)
                .OrderByDescending(x => x.UpdatedAt)
                .Select(x => (Guid?)x.SyllabusId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (!syllabusId.HasValue)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.SyllabusNotFound(command.SyllabusId));
        }

        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Select(x => new { x.Id, x.LevelId, x.IsActive, x.IsDeleted })
            .FirstOrDefaultAsync(x => x.Id == syllabusId.Value, cancellationToken);
        if (syllabus is null || syllabus.IsDeleted || !syllabus.IsActive)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.SyllabusNotFound(syllabusId));
        }

        if (syllabus.LevelId != module.LevelId)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.SyllabusModuleMismatch(syllabusId.Value, module.Id));
        }

        var parsed = CurriculumWordImportParser.ParseLessonPlanDocx(command.FileStream, command.FileName);
        if (parsed.IsFailure)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(parsed.Error);
        }

        var sessionIndex = command.SessionIndexOverride.GetValueOrDefault() > 0
            ? command.SessionIndexOverride!.Value
            : requestedUnit is not null
                ? await ResolveNextSessionIndexInModuleAsync(syllabusId.Value, module.Id, module.PlannedSessionCount, cancellationToken)
                : ResolveSessionIndex(parsed.Value, command.FileName, module.Name, module.PlannedSessionCount);
        if (sessionIndex <= 0 || sessionIndex > module.PlannedSessionCount)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.SessionIndexOutOfRange(sessionIndex, module.PlannedSessionCount));
        }

        var template = await context.LessonPlanTemplates
            .FirstOrDefaultAsync(
                x => x.ModuleId == module.Id &&
                     x.SyllabusId == syllabusId.Value &&
                     x.SessionIndex == sessionIndex,
                cancellationToken);

        if (template is null && command.OverwriteExisting)
        {
            template = await FindExistingTemplateBySourceFileNameAsync(
                syllabusId.Value,
                module.Id,
                command.FileName,
                cancellationToken);
        }

        if (template is not null && !command.OverwriteExisting)
        {
            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.DuplicateSessionIndex(module.Id, sessionIndex));
        }

        var linkedSessionTemplate = await context.SessionTemplates
            .Where(x => x.SyllabusId == syllabusId.Value &&
                        x.ModuleId == module.Id &&
                        x.SessionIndexInModule == sessionIndex &&
                        x.IsActive)
            .OrderBy(x => x.OrderIndex)
            .FirstOrDefaultAsync(cancellationToken);
        var linkedSessionTemplateId = linkedSessionTemplate?.Id;

        var now = VietnamTime.UtcNow();
        var canonicalLessonNumber = command.LessonNumberOverride ??
                                    LessonPlanUnitNameNormalizer.ExtractLessonNumber(
                                        command.FileName,
                                        parsed.Value.Title,
                                        parsed.Value.UnitTitle) ??
                                    parsed.Value.LessonNumber;
        var lessonPlanUnit = requestedUnit;
        if (lessonPlanUnit is null)
        {
            var unitName = command.LessonPlanUnitNameOverride;
            if (string.IsNullOrWhiteSpace(unitName))
            {
                unitName = LessonPlanUnitNameNormalizer.ExtractUnitName(
                    parsed.Value.Title,
                    parsed.Value.UnitTitle,
                    command.FileName);
            }

            if (!string.IsNullOrWhiteSpace(unitName))
            {
                var unitIdentity = LessonPlanUnitNameNormalizer.ExtractUnitIdentity(unitName)
                                   ?? new LessonPlanUnitIdentity(
                                       CanonicalDisplayName: LessonPlanUnitNameNormalizer.Normalize(unitName),
                                       NormalizedKey: LessonPlanUnitNameNormalizer.Normalize(unitName),
                                       UnitNumber: null,
                                       UnitTitle: null);

                lessonPlanUnit = await LessonPlanUnitResolver.FindOrCreateAsync(
                    context,
                    module.Id,
                    unitIdentity,
                    command.LessonPlanUnitOrderIndexOverride,
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
                SyllabusId = syllabusId.Value,
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

        var canonicalUnitName = lessonPlanUnit?.Name ??
                                command.LessonPlanUnitNameOverride ??
                                parsed.Value.UnitTitle;
        var canonicalTitle = BuildCanonicalTitle(
            canonicalUnitName,
            canonicalLessonNumber,
            parsed.Value.Title);

        template.SyllabusId = syllabusId.Value;
        template.SessionTemplateId = linkedSessionTemplateId;
        template.LessonPlanUnitId = lessonPlanUnit?.Id;
        template.Title = canonicalTitle;
        template.SessionIndex = sessionIndex;
        template.SessionOrder = sessionIndex;
        if (lessonPlanUnit is null)
        {
            template.OrderIndexInUnit = 0;
        }
        else
        {
            template.OrderIndexInUnit = canonicalLessonNumber.HasValue
                ? Math.Max(canonicalLessonNumber.Value - 1, 0)
                : await LessonPlanUnitResolver.GetNextOrderInUnitAsync(context, lessonPlanUnit.Id, cancellationToken);
        }
        template.SyllabusMetadata = canonicalUnitName;
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
        var activities = BuildActivityTemplates(template.Id, parsed.Value, now);
        var materials = BuildMaterialTemplates(template.Id, parsed.Value, now, ref orderIndex);
        var homeworks = BuildHomeworkTemplates(template.Id, parsed.Value, now);

        EntityStringLengthTrimmer.TrimToModelLimits(context, activities);
        EntityStringLengthTrimmer.TrimToModelLimits(context, materials);
        EntityStringLengthTrimmer.TrimToModelLimits(context, homeworks);

        context.LessonPlanTemplateActivities.AddRange(activities);
        context.LessonPlanTemplateMaterials.AddRange(materials);
        context.HomeworkTemplates.AddRange(homeworks);

        if (linkedSessionTemplate is not null)
        {
            linkedSessionTemplate.LessonPlanTemplateId = template.Id;
            linkedSessionTemplate.UpdatedAt = now;
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsLegacyModuleSessionIndexConstraintViolation(ex))
        {
            if (context is DbContext dbContext)
            {
                dbContext.ChangeTracker.Clear();
            }

            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(
                LessonPlanTemplateErrors.LegacyModuleSessionIndexConstraintStillActive);
        }
        catch (DbUpdateException ex) when (TryBuildLengthViolationError(context, ex, out var error))
        {
            if (context is DbContext dbContext)
            {
                dbContext.ChangeTracker.Clear();
            }

            return Result.Failure<ImportLessonPlanTemplateFromWordResponse>(error);
        }

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

    private static string BuildCanonicalTitle(
        string? canonicalUnitName,
        int? canonicalLessonNumber,
        string parsedTitle)
    {
        if (!string.IsNullOrWhiteSpace(canonicalUnitName) && canonicalLessonNumber.HasValue)
        {
            return $"{canonicalUnitName.Trim()} - Lesson {canonicalLessonNumber.Value}";
        }

        if (!string.IsNullOrWhiteSpace(canonicalUnitName))
        {
            return canonicalUnitName.Trim();
        }

        return parsedTitle;
    }

    private async Task<LessonPlanTemplate?> FindExistingTemplateBySourceFileNameAsync(
        Guid syllabusId,
        Guid moduleId,
        string fileName,
        CancellationToken cancellationToken)
    {
        var normalizedSourceFileName = NormalizeSourceFileName(fileName);
        if (string.IsNullOrWhiteSpace(normalizedSourceFileName))
        {
            return null;
        }

        var candidates = await context.LessonPlanTemplates
            .Where(x => x.SyllabusId == syllabusId && x.ModuleId == moduleId)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(x =>
            string.Equals(
                NormalizeSourceFileName(x.SourceFileName),
                normalizedSourceFileName,
                StringComparison.OrdinalIgnoreCase));
    }

    private async Task<int> ResolveNextSessionIndexInModuleAsync(
        Guid syllabusId,
        Guid moduleId,
        int plannedSessionCount,
        CancellationToken cancellationToken)
    {
        var usedSessionIndexes = await context.LessonPlanTemplates
            .AsNoTracking()
            .Where(x => x.SyllabusId == syllabusId &&
                        x.ModuleId == moduleId &&
                        !x.IsDeleted)
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

    private static string? NormalizeSourceFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var trimmed = Path.GetFileName(fileName).Trim();
        return string.IsNullOrWhiteSpace(trimmed)
            ? null
            : Regex.Replace(trimmed, @"\s+", " ");
    }

    private static List<LessonPlanTemplateActivity> BuildActivityTemplates(
        Guid templateId,
        ParsedLessonPlanDocument parsed,
        DateTime now)
    {
        var stages = SplitProcedureStages(parsed.Procedure);
        return stages.Select((stage, index) => new LessonPlanTemplateActivity
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Title = TruncateLabel(stage.Title, ActivityTitleMaxLength),
                TeacherActivity = string.Join("\n", stage.Details),
                StudentActivity = null,
                Resources = null,
                DurationMinutes = null,
                OrderIndex = index + 1,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();
    }

    private static List<LessonPlanTemplateMaterial> BuildMaterialTemplates(Guid templateId, ParsedLessonPlanDocument parsed, DateTime now, ref int orderIndex)
    {
        var items = new List<LessonPlanTemplateMaterial>();
        foreach (var material in SplitTextItems(parsed.TeacherMaterials))
        {
            items.Add(new LessonPlanTemplateMaterial
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Name = TruncateLabel(material, MaterialNameMaxLength),
                MaterialType = "teacher",
                Notes = material,
                OrderIndex = orderIndex++,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        foreach (var material in SplitTextItems(parsed.StudentMaterials))
        {
            items.Add(new LessonPlanTemplateMaterial
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Name = TruncateLabel(material, MaterialNameMaxLength),
                MaterialType = "student",
                Notes = material,
                OrderIndex = orderIndex++,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        return items;
    }

    private static List<HomeworkTemplate> BuildHomeworkTemplates(Guid templateId, ParsedLessonPlanDocument parsed, DateTime now)
    {
        return SplitTextItems(parsed.Homework)
            .Select((homework, index) => new HomeworkTemplate
            {
                Id = Guid.NewGuid(),
                LessonPlanTemplateId = templateId,
                Title = "Homework",
                Instructions = homework,
                OrderIndex = index + 1,
                IsRequired = true,
                CreatedAt = now,
                UpdatedAt = now
            })
            .ToList();
    }

    private static List<string> SplitTextItems(string? value)
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
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string TruncateLabel(string value, int maxLength)
    {
        var normalized = Regex.Replace(value, @"\s+", " ").Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength].Trim();
    }

    private static List<ProcedureStage> SplitProcedureStages(string? procedure)
    {
        var lines = SplitTextItems(procedure);
        if (lines.Count == 0)
        {
            return [];
        }

        var stages = new List<ProcedureStage>();
        ProcedureStage? current = null;

        foreach (var line in lines)
        {
            if (LooksLikeProcedureStageTitle(line))
            {
                current = new ProcedureStage(line, []);
                stages.Add(current);
                continue;
            }

            if (current is null)
            {
                current = new ProcedureStage("Procedure", []);
                stages.Add(current);
            }

            current.Details.Add(line);
        }

        return stages;
    }

    private static bool LooksLikeProcedureStageTitle(string value)
    {
        return value.EndsWith(':') ||
               Regex.IsMatch(
                   value,
                   @"^(warm[\s-]?up|lead[\s-]?in|presentation|practice|production|wrap[\s-]?up|review|homework|activity\s*\d+|stage\s*\d+)",
                   RegexOptions.IgnoreCase);
    }

    private static bool TryBuildLengthViolationError(
        IDbContext context,
        DbUpdateException exception,
        out Error error)
    {
        error = Error.None;

        if (!IsStringLengthViolation(exception))
        {
            return false;
        }

        if (context is not DbContext dbContext)
        {
            error = LessonPlanTemplateErrors.InvalidImportFile(
                "Imported lesson plan contains text longer than the database allows.");
            return true;
        }

        var offenders = FindStringLengthViolations(dbContext)
            .Take(5)
            .ToList();

        if (offenders.Count == 0)
        {
            error = LessonPlanTemplateErrors.InvalidImportFile(
                "Imported lesson plan contains text longer than the database allows, but the exact field could not be identified from the current EF model.");
            return true;
        }

        var details = string.Join(
            "; ",
            offenders.Select(x => $"{x.EntityName}.{x.PropertyName} length {x.Length} > {x.MaxLength}"));

        error = LessonPlanTemplateErrors.InvalidImportFile(
            $"Imported lesson plan contains text longer than the database allows: {details}.");
        return true;
    }

    private static bool IsStringLengthViolation(DbUpdateException exception)
    {
        if (exception.Message.Contains("value too long", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var innerException = exception.InnerException;
        if (innerException is null)
        {
            return false;
        }

        if (innerException.Message.Contains("value too long for type character varying", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var sqlState = innerException.GetType().GetProperty("SqlState")?.GetValue(innerException)?.ToString();
        return string.Equals(sqlState, "22001", StringComparison.Ordinal);
    }

    private static bool IsLegacyModuleSessionIndexConstraintViolation(DbUpdateException exception)
    {
        if (exception.InnerException is null)
        {
            return false;
        }

        var message = exception.InnerException.Message;
        if (!message.Contains("IX_LessonPlanTemplates_ModuleId_SessionIndex", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var sqlState = exception.InnerException.GetType().GetProperty("SqlState")?.GetValue(exception.InnerException)?.ToString();
        return string.Equals(sqlState, "23505", StringComparison.Ordinal);
    }

    private static IEnumerable<StringLengthViolation> FindStringLengthViolations(DbContext dbContext)
    {
        foreach (var entry in dbContext.ChangeTracker.Entries()
                     .Where(x => x.State is EntityState.Added or EntityState.Modified))
        {
            foreach (var property in entry.Properties)
            {
                if (property.Metadata.ClrType != typeof(string))
                {
                    continue;
                }

                if (property.CurrentValue is not string value)
                {
                    continue;
                }

                var maxLength = property.Metadata.GetMaxLength();
                if (!maxLength.HasValue || value.Length <= maxLength.Value)
                {
                    continue;
                }

                yield return new StringLengthViolation(
                    entry.Metadata.ClrType.Name,
                    property.Metadata.Name,
                    value.Length,
                    maxLength.Value);
            }
        }
    }

    private sealed record StringLengthViolation(
        string EntityName,
        string PropertyName,
        int Length,
        int MaxLength);

    private sealed record ProcedureStage(string Title, List<string> Details);
}
