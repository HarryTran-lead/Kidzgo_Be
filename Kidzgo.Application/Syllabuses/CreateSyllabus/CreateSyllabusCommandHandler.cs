using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.CreateSyllabus;

public sealed class CreateSyllabusCommandHandler(IDbContext context)
    : ICommandHandler<CreateSyllabusCommand, SyllabusDocumentResponse>
{
    public async Task<Result<SyllabusDocumentResponse>> Handle(CreateSyllabusCommand command, CancellationToken cancellationToken)
    {
        var level = await context.Levels
            .Where(x => x.Id == command.LevelId && x.IsActive)
            .Select(x => new { x.Id, x.ProgramId })
            .FirstOrDefaultAsync(cancellationToken);

        if (level is null)
        {
            return Result.Failure<SyllabusDocumentResponse>(SyllabusErrors.LevelNotFound(command.LevelId));
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<SyllabusDocumentResponse>(
                SyllabusErrors.LevelDoesNotBelongToProgram(command.LevelId, command.ProgramId));
        }

        var normalizedCode = command.Code.Trim();
        try
        {
            await SyllabusDocumentRules.EnsureUniqueActiveCodeAsync(
                context,
                command.ProgramId,
                command.LevelId,
                normalizedCode,
                ignoreId: null,
                cancellationToken);
        }
        catch (SyllabusDocumentRuleException ex)
        {
            return Result.Failure<SyllabusDocumentResponse>(ex.Error);
        }

        var now = VietnamTime.UtcNow();
        var status = SyllabusDocumentMapper.NormalizeStatus(command.Status);
        var sourceType = SyllabusDocumentMapper.NormalizeSourceType(command.SourceType);
        var version = string.IsNullOrWhiteSpace(command.Version)
            ? $"doc-{now:yyyyMMddHHmmssfff}"
            : command.Version.Trim();
        var sections = SyllabusDocumentMapper.BuildInitialManualSections();
        var warnings = new List<SyllabusDocumentWarningDto>();
        var syllabus = new Syllabus
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            Code = normalizedCode,
            Version = version,
            Title = command.Title.Trim(),
            Edition = command.Edition?.Trim(),
            EffectiveFrom = command.EffectiveFrom,
            EffectiveTo = command.EffectiveTo,
            PacingSchemeJson = command.PacingSchemeJson,
            Overview = command.Overview,
            OverallObjectives = command.OverallObjectives,
            SpecificObjectives = command.SpecificObjectives,
            EthicsAndAttitudes = command.EthicsAndAttitudes,
            BookOverview = command.BookOverview,
            TotalPeriods = command.TotalPeriods,
            MinutesPerPeriod = command.MinutesPerPeriod,
            TotalLessons = command.TotalLessons,
            SourceFileName = command.SourceFileName,
            AttachmentUrl = command.AttachmentUrl,
            RawContentJson = command.RawContentJson,
            DocumentStatus = status,
            SourceType = sourceType,
            DocumentVersion = 1,
            SectionsJson = SyllabusDocumentMapper.WriteSections(sections),
            WarningsJson = SyllabusDocumentMapper.WriteWarnings(warnings),
            IsActive = command.IsActive,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Syllabuses.Add(syllabus);
        await context.SaveChangesAsync(cancellationToken);

        return SyllabusDocumentMapper.ToResponse(
            syllabus,
            sections,
            warnings,
            totalUnits: 0,
            totalSessions: 0,
            totalLessons: syllabus.TotalLessons ?? 0,
            totalPeriods: syllabus.TotalPeriods ?? 0);
    }
}
