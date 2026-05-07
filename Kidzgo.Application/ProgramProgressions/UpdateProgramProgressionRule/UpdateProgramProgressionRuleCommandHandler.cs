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
            .FirstOrDefaultAsync(r => r.Id == command.Id, cancellationToken);

        if (rule is null)
        {
            return Result.Failure<ProgramProgressionRuleDto>(
                ProgramProgressionErrors.RuleNotFound(command.Id));
        }

        if (command.TargetProgramId.HasValue && command.TargetProgramId.Value == command.SourceProgramId)
        {
            return Result.Failure<ProgramProgressionRuleDto>(Error.Validation(
                "ProgramProgression.TargetProgramDuplicated",
                "Target program must be different from source program."));
        }

        var sourceProgram = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.SourceProgramId && p.IsActive && !p.IsDeleted, cancellationToken);

        if (sourceProgram is null)
        {
            return Result.Failure<ProgramProgressionRuleDto>(
                RegistrationErrors.ProgramNotFound(command.SourceProgramId));
        }

        Kidzgo.Domain.Programs.Program? targetProgram = null;
        if (command.TargetProgramId.HasValue)
        {
            targetProgram = await context.Programs
                .FirstOrDefaultAsync(p => p.Id == command.TargetProgramId.Value && p.IsActive && !p.IsDeleted, cancellationToken);

            if (targetProgram is null)
            {
                return Result.Failure<ProgramProgressionRuleDto>(
                    RegistrationErrors.ProgramNotFound(command.TargetProgramId.Value));
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
                    r.SourceProgramId == command.SourceProgramId &&
                    r.IsActive,
                    cancellationToken);

            if (activeRuleExists)
            {
                return Result.Failure<ProgramProgressionRuleDto>(
                    ProgramProgressionErrors.ActiveRuleAlreadyExists(command.SourceProgramId));
            }
        }

        rule.SourceProgramId = command.SourceProgramId;
        rule.TargetProgramId = command.TargetProgramId;
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
        rule.SourceProgram = sourceProgram;
        rule.TargetProgram = targetProgram;
        rule.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}
