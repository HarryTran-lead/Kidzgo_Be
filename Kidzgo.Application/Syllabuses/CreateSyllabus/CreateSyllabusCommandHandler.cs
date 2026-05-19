using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.CreateSyllabus;

public sealed class CreateSyllabusCommandHandler(IDbContext context)
    : ICommandHandler<CreateSyllabusCommand, CreateSyllabusResponse>
{
    public async Task<Result<CreateSyllabusResponse>> Handle(CreateSyllabusCommand command, CancellationToken cancellationToken)
    {
        var level = await context.Levels
            .Where(x => x.Id == command.LevelId && x.IsActive)
            .Select(x => new { x.Id, x.ProgramId })
            .FirstOrDefaultAsync(cancellationToken);

        if (level is null)
        {
            return Result.Failure<CreateSyllabusResponse>(SyllabusErrors.LevelNotFound(command.LevelId));
        }

        if (level.ProgramId != command.ProgramId)
        {
            return Result.Failure<CreateSyllabusResponse>(
                SyllabusErrors.LevelDoesNotBelongToProgram(command.LevelId, command.ProgramId));
        }

        var exists = await context.Syllabuses.AnyAsync(
            x => x.ProgramId == command.ProgramId &&
                 x.LevelId == command.LevelId &&
                 x.Code == command.Code &&
                 x.Version == command.Version &&
                 !x.IsDeleted,
            cancellationToken);

        if (exists)
        {
            return Result.Failure<CreateSyllabusResponse>(
                SyllabusErrors.DuplicateVersion(command.ProgramId, command.LevelId, command.Code, command.Version));
        }

        var now = VietnamTime.UtcNow();
        var syllabus = new Syllabus
        {
            Id = Guid.NewGuid(),
            ProgramId = command.ProgramId,
            LevelId = command.LevelId,
            Code = command.Code.Trim(),
            Version = command.Version.Trim(),
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
            IsActive = command.IsActive,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Syllabuses.Add(syllabus);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateSyllabusResponse
        {
            Id = syllabus.Id,
            ProgramId = syllabus.ProgramId,
            LevelId = syllabus.LevelId,
            Code = syllabus.Code,
            Version = syllabus.Version,
            Title = syllabus.Title,
            IsActive = syllabus.IsActive
        };
    }
}
