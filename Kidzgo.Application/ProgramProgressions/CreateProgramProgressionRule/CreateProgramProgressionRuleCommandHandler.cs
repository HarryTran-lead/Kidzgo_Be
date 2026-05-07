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
                .AnyAsync(r => r.SourceProgramId == command.SourceProgramId && r.IsActive, cancellationToken);

            if (activeRuleExists)
            {
                return Result.Failure<ProgramProgressionRuleDto>(
                    ProgramProgressionErrors.ActiveRuleAlreadyExists(command.SourceProgramId));
            }
        }

        var now = VietnamTime.UtcNow();
        var rule = new ProgramProgressionRule
        {
            Id = Guid.NewGuid(),
            SourceProgramId = command.SourceProgramId,
            TargetProgramId = command.TargetProgramId,
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
            SourceProgram = sourceProgram,
            TargetProgram = targetProgram
        };

        context.ProgramProgressionRules.Add(rule);
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(rule.ToDto());
    }
}
