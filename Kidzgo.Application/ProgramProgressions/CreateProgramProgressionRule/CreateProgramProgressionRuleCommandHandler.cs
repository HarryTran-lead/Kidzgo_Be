using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.CreateProgramProgressionRule;

public sealed class CreateProgramProgressionRuleCommandHandler(
    IDbContext context) : ICommandHandler<CreateProgramProgressionRuleCommand, ProgramProgressionRuleDto>
{
    public async Task<Result<ProgramProgressionRuleDto>> Handle(
        CreateProgramProgressionRuleCommand command,
        CancellationToken cancellationToken)
    {
        var sourceLevel = await context.Levels
            .FirstOrDefaultAsync(x => x.Id == command.SourceLevelId && x.IsActive, cancellationToken);

        if (sourceLevel is null)
        {
            return Result.Failure<ProgramProgressionRuleDto>(Error.Validation(
                "ProgramProgression.SourceLevelNotFound",
                $"Source level '{command.SourceLevelId}' was not found or inactive."));
        }

        if (command.SourceProgramId.HasValue &&
            command.SourceProgramId.Value != sourceLevel.ProgramId)
        {
            return Result.Failure<ProgramProgressionRuleDto>(Error.Validation(
                "ProgramProgression.SourceProgramLevelMismatch",
                "SourceProgramId does not match SourceLevelId."));
        }

        var sourceProgramId = sourceLevel.ProgramId;

        Kidzgo.Domain.Programs.Level? targetLevel = null;
        if (command.TargetLevelId.HasValue)
        {
            targetLevel = await context.Levels
                .FirstOrDefaultAsync(x => x.Id == command.TargetLevelId.Value && x.IsActive, cancellationToken);

            if (targetLevel is null)
            {
                return Result.Failure<ProgramProgressionRuleDto>(Error.Validation(
                    "ProgramProgression.TargetLevelNotFound",
                    $"Target level '{command.TargetLevelId.Value}' was not found or inactive."));
            }
        }

        if (command.TargetProgramId.HasValue &&
            targetLevel is not null &&
            command.TargetProgramId.Value != targetLevel.ProgramId)
        {
            return Result.Failure<ProgramProgressionRuleDto>(Error.Validation(
                "ProgramProgression.TargetProgramLevelMismatch",
                "TargetProgramId does not match TargetLevelId."));
        }

        var targetProgramId = targetLevel?.ProgramId ?? command.TargetProgramId;

        if (targetProgramId.HasValue && targetProgramId.Value == sourceProgramId)
        {
            if (!command.TargetLevelId.HasValue ||
                command.SourceLevelId == command.TargetLevelId.Value)
            {
                return Result.Failure<ProgramProgressionRuleDto>(Error.Validation(
                    "ProgramProgression.TargetProgramDuplicated",
                    "When source and target are in the same program, SourceLevelId and TargetLevelId must be provided and different."));
            }
        }

        var sourceProgram = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == sourceProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (sourceProgram is null)
        {
            return Result.Failure<ProgramProgressionRuleDto>(
                RegistrationErrors.ProgramNotFound(sourceProgramId));
        }

        Kidzgo.Domain.Programs.Program? targetProgram = null;
        if (targetProgramId.HasValue)
        {
            targetProgram = await context.Programs
                .FirstOrDefaultAsync(p => p.Id == targetProgramId.Value && p.IsActive && !p.IsDeleted, cancellationToken);

            if (targetProgram is null)
            {
                return Result.Failure<ProgramProgressionRuleDto>(
                    RegistrationErrors.ProgramNotFound(targetProgramId.Value));
            }
        }

        var configValidation = ProgramProgressionRuleDefinition.Validate(
            command.Method,
            command.MinimumShieldCount,
            command.MinimumSkillShieldCount,
            command.MinimumOverallScore,
            command.ShieldMappings.ToList(),
            command.ClassificationBands.ToList());
        if (configValidation.IsFailure)
        {
            return Result.Failure<ProgramProgressionRuleDto>(configValidation.Error);
        }

        if (command.IsActive)
        {
            var activeRuleExists = await context.ProgramProgressionRules
                .AnyAsync(r => r.SourceLevelId == command.SourceLevelId && r.IsActive, cancellationToken);

            if (activeRuleExists)
            {
                return Result.Failure<ProgramProgressionRuleDto>(
                    ProgramProgressionErrors.ActiveRuleAlreadyExists(sourceProgramId));
            }
        }

        var now = VietnamTime.UtcNow();
        var rule = new ProgramProgressionRule
        {
            Id = Guid.NewGuid(),
            SourceLevelId = command.SourceLevelId,
            TargetLevelId = command.TargetLevelId,
            SourceProgramId = sourceProgramId,
            TargetProgramId = targetProgramId,
            Method = command.Method,
            MinimumShieldCount = command.MinimumShieldCount,
            MinimumSkillShieldCount = command.MinimumSkillShieldCount,
            MinimumOverallScore = command.MinimumOverallScore,
            CarryOverRemainingSessions = command.CarryOverRemainingSessions,
            StopCurrentEnrollmentOnApproval = command.StopCurrentEnrollmentOnApproval,
            IsActive = command.IsActive,
            Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim(),
            ShieldMappingJson = ProgramProgressionRuleDefinition.SerializeShieldMappings(command.ShieldMappings),
            ClassificationBandsJson = ProgramProgressionRuleDefinition.SerializeClassificationBands(command.ClassificationBands),
            PracticeTestScoreMappingsJson = ProgramProgressionRuleDefinition.SerializePracticeTestScoreMappings(command.PracticeTestScoreMappings),
            CreatedAt = now,
            UpdatedAt = now,
            SourceLevel = sourceLevel,
            TargetLevel = targetLevel,
            SourceProgram = sourceProgram,
            TargetProgram = targetProgram
        };

        context.ProgramProgressionRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}
