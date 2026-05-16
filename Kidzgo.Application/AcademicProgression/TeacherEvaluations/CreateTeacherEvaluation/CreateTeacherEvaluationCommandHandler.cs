using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.AcademicProgression.Shared;
using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.AcademicProgression.TeacherEvaluations.CreateTeacherEvaluation;

public sealed class CreateTeacherEvaluationCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<CreateTeacherEvaluationCommand, TeacherEvaluationDto>
{
    public async Task<Result<TeacherEvaluationDto>> Handle(CreateTeacherEvaluationCommand command, CancellationToken cancellationToken)
    {
        var moduleExists = await context.Modules.AnyAsync(x => x.Id == command.ModuleId, cancellationToken);
        if (!moduleExists)
        {
            return Result.Failure<TeacherEvaluationDto>(AcademicProgressionErrors.ModuleNotFound(command.ModuleId));
        }

        var studentExists = await context.Profiles
            .AnyAsync(x => x.Id == command.StudentProfileId && x.ProfileType == Domain.Users.ProfileType.Student, cancellationToken);
        if (!studentExists)
        {
            return Result.Failure<TeacherEvaluationDto>(
                Error.NotFound("AcademicProgression.StudentNotFound", $"Student '{command.StudentProfileId}' was not found."));
        }

        var now = VietnamTime.UtcNow();
        var evaluation = new TeacherEvaluation
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            ModuleId = command.ModuleId,
            Speaking = command.Speaking,
            Listening = command.Listening,
            Reading = command.Reading,
            Writing = command.Writing,
            Participation = command.Participation,
            Confidence = command.Confidence,
            Behavior = command.Behavior,
            Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
            EvaluatedBy = userContext.UserId,
            EvaluatedAt = command.EvaluatedAt ?? now,
            CreatedAt = now
        };

        context.TeacherEvaluations.Add(evaluation);
        await context.SaveChangesAsync(cancellationToken);

        var module = await context.Modules.AsNoTracking().Include(x => x.Level).FirstAsync(x => x.Id == evaluation.ModuleId, cancellationToken);
        return Result.Success(new TeacherEvaluationDto
        {
            Id = evaluation.Id,
            StudentProfileId = evaluation.StudentProfileId,
            ModuleId = evaluation.ModuleId,
            ModuleCode = module.Code,
            Speaking = evaluation.Speaking,
            Listening = evaluation.Listening,
            Reading = evaluation.Reading,
            Writing = evaluation.Writing,
            Participation = evaluation.Participation,
            Confidence = evaluation.Confidence,
            Behavior = evaluation.Behavior,
            Notes = evaluation.Notes,
            EvaluatedBy = evaluation.EvaluatedBy,
            EvaluatedAt = evaluation.EvaluatedAt
        });
    }
}
