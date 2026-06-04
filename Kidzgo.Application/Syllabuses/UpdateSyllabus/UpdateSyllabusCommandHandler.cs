using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.UpdateSyllabus;

public sealed class UpdateSyllabusCommandHandler(IDbContext context)
    : ICommandHandler<UpdateSyllabusCommand, UpdateSyllabusResponse>
{
    public async Task<Result<UpdateSyllabusResponse>> Handle(UpdateSyllabusCommand command, CancellationToken cancellationToken)
    {
        var syllabus = await context.Syllabuses
            .FirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted, cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<UpdateSyllabusResponse>(SyllabusErrors.NotFound(command.Id));
        }

        if (command.Version <= 0)
        {
            return Result.Failure<UpdateSyllabusResponse>(SyllabusErrors.InvalidVersion(command.Version));
        }

        var duplicate = await context.Syllabuses.AnyAsync(
            x => x.Id != command.Id &&
                 x.ProgramId == syllabus.ProgramId &&
                 x.LevelId == syllabus.LevelId &&
                 x.Code == command.Code &&
                 x.Version == command.Version &&
                 !x.IsDeleted,
            cancellationToken);

        if (duplicate)
        {
            return Result.Failure<UpdateSyllabusResponse>(
                SyllabusErrors.DuplicateVersion(syllabus.ProgramId, syllabus.LevelId, command.Code, command.Version));
        }

        syllabus.Code = command.Code.Trim();
        syllabus.Version = command.Version;
        syllabus.Title = command.Title.Trim();
        syllabus.Edition = command.Edition?.Trim();
        syllabus.EffectiveFrom = command.EffectiveFrom;
        syllabus.EffectiveTo = command.EffectiveTo;
        syllabus.PacingSchemeJson = command.PacingSchemeJson;
        syllabus.Overview = command.Overview;
        syllabus.OverallObjectives = command.OverallObjectives;
        syllabus.SpecificObjectives = command.SpecificObjectives;
        syllabus.EthicsAndAttitudes = command.EthicsAndAttitudes;
        syllabus.BookOverview = command.BookOverview;
        syllabus.TotalPeriods = command.TotalPeriods;
        syllabus.MinutesPerPeriod = command.MinutesPerPeriod;
        syllabus.TotalLessons = command.TotalLessons;
        syllabus.SourceFileName = command.SourceFileName;
        syllabus.AttachmentUrl = command.AttachmentUrl;
        syllabus.RawContentJson = command.RawContentJson;
        syllabus.IsActive = command.IsActive;
        syllabus.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateSyllabusResponse
        {
            Id = syllabus.Id,
            Code = syllabus.Code,
            Version = syllabus.Version,
            Title = syllabus.Title,
            IsActive = syllabus.IsActive
        };
    }
}
