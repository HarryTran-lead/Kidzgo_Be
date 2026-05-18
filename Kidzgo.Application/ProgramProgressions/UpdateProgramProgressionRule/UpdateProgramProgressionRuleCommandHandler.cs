using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.ProgramProgressions.Errors;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.ProgramProgressions.UpdateProgramProgressionRule;

public sealed class UpdateProgramProgressionRuleCommandHandler(
    IDbContext context) : ICommandHandler<UpdateProgramProgressionRuleCommand, ProgramProgressionRuleDto>
{
    public async Task<Result<ProgramProgressionRuleDto>> Handle(
        UpdateProgramProgressionRuleCommand command,
        CancellationToken cancellationToken)
    {
        var rule = await context.ProgramProgressionRules
            .Include(r => r.SourceProgram)
            .Include(r => r.TargetProgram)
            .Include(r => r.SourceLevel)
            .Include(r => r.TargetLevel)
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<ProgramProgressionRuleDto>(
                ProgramProgressionErrors.RuleNotFound(command.Id));
        }

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
                .AnyAsync(r =>
                    r.Id != command.Id &&
                    r.SourceLevelId == command.SourceLevelId &&
                    r.IsActive,
                    cancellationToken);

            if (activeRuleExists)
            {
                return Result.Failure<ProgramProgressionRuleDto>(
                    ProgramProgressionErrors.ActiveRuleAlreadyExists(sourceProgramId));
            }
        }

        rule.SourceLevelId = command.SourceLevelId;
        rule.TargetLevelId = command.TargetLevelId;
        rule.SourceProgramId = sourceProgramId;
        rule.TargetProgramId = targetProgramId;
        rule.Method = command.Method;
        rule.MinimumShieldCount = command.MinimumShieldCount;
        rule.MinimumSkillShieldCount = command.MinimumSkillShieldCount;
        rule.MinimumOverallScore = command.MinimumOverallScore;
        rule.CarryOverRemainingSessions = command.CarryOverRemainingSessions;
        rule.StopCurrentEnrollmentOnApproval = command.StopCurrentEnrollmentOnApproval;
        rule.IsActive = command.IsActive;
        rule.Notes = string.IsNullOrWhiteSpace(command.Notes) ? null : command.Notes.Trim();
        rule.ShieldMappingJson = ProgramProgressionRuleDefinition.SerializeShieldMappings(command.ShieldMappings);
        rule.ClassificationBandsJson = ProgramProgressionRuleDefinition.SerializeClassificationBands(command.ClassificationBands);
        rule.PracticeTestScoreMappingsJson = ProgramProgressionRuleDefinition.SerializePracticeTestScoreMappings(command.PracticeTestScoreMappings);
        rule.SourceLevel = sourceLevel;
        rule.TargetLevel = targetLevel;
        rule.SourceProgram = sourceProgram;
        rule.TargetProgram = targetProgram;
        rule.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}
