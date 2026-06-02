using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Syllabuses.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Syllabuses.PromoteSyllabusVersion;

public sealed class PromoteSyllabusVersionCommandHandler(IDbContext context)
    : ICommandHandler<PromoteSyllabusVersionCommand, PromoteSyllabusVersionResponse>
{
    public async Task<Result<PromoteSyllabusVersionResponse>> Handle(
        PromoteSyllabusVersionCommand command,
        CancellationToken cancellationToken)
    {
        var source = await context.Syllabuses
            .FirstOrDefaultAsync(x => x.Id == command.SourceSyllabusId && !x.IsDeleted, cancellationToken);

        if (source is null)
        {
            return Result.Failure<PromoteSyllabusVersionResponse>(SyllabusErrors.NotFound(command.SourceSyllabusId));
        }

        var target = await context.Syllabuses
            .FirstOrDefaultAsync(x => x.Id == command.TargetSyllabusId && !x.IsDeleted, cancellationToken);

        if (target is null)
        {
            return Result.Failure<PromoteSyllabusVersionResponse>(SyllabusErrors.NotFound(command.TargetSyllabusId));
        }

        if (source.ProgramId != target.ProgramId ||
            source.LevelId != target.LevelId ||
            !string.Equals(source.Code, target.Code, StringComparison.Ordinal))
        {
            return Result.Failure<PromoteSyllabusVersionResponse>(
                SyllabusErrors.VersionFamilyMismatch(command.SourceSyllabusId, command.TargetSyllabusId));
        }

        await SyllabusVersionPromotionService.PromoteAsync(context, target, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return new PromoteSyllabusVersionResponse
        {
            SyllabusId = target.Id,
            Version = target.Version,
            IsActive = true
        };
    }
}
