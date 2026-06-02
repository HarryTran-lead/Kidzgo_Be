using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.TuitionPlans.CreatePackageCurriculumMapping;

public sealed class CreatePackageCurriculumMappingCommandHandler(IDbContext context)
    : ICommandHandler<CreatePackageCurriculumMappingCommand, CreatePackageCurriculumMappingResponse>
{
    public async Task<Result<CreatePackageCurriculumMappingResponse>> Handle(
        CreatePackageCurriculumMappingCommand command,
        CancellationToken cancellationToken)
    {
        var tuitionPlan = await context.TuitionPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.TuitionPlanId && !x.IsDeleted, cancellationToken);

        if (tuitionPlan is null)
        {
            return Result.Failure<CreatePackageCurriculumMappingResponse>(TuitionPlanErrors.NotFound(command.TuitionPlanId));
        }

        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.SyllabusId && !x.IsDeleted, cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<CreatePackageCurriculumMappingResponse>(TuitionPlanErrors.SyllabusNotFound);
        }

        if (!syllabus.IsActive)
        {
            return Result.Failure<CreatePackageCurriculumMappingResponse>(TuitionPlanErrors.SyllabusInactive);
        }

        if (syllabus.ProgramId != tuitionPlan.ProgramId)
        {
            return Result.Failure<CreatePackageCurriculumMappingResponse>(TuitionPlanErrors.SyllabusProgramMismatch);
        }

        if (syllabus.LevelId != tuitionPlan.LevelId)
        {
            return Result.Failure<CreatePackageCurriculumMappingResponse>(TuitionPlanErrors.SyllabusLevelMismatch);
        }

        var mapping = await context.PackageCurriculumMappings
            .FirstOrDefaultAsync(
                x => x.TuitionPlanId == command.TuitionPlanId && x.SyllabusId == command.SyllabusId,
                cancellationToken);

        var now = VietnamTime.UtcNow();
        if (mapping is null)
        {
            mapping = new PackageCurriculumMapping
            {
                Id = Guid.NewGuid(),
                TuitionPlanId = command.TuitionPlanId,
                SyllabusId = command.SyllabusId,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.PackageCurriculumMappings.Add(mapping);
        }
        else if (mapping.IsActive)
        {
            return Result.Failure<CreatePackageCurriculumMappingResponse>(
                TuitionPlanErrors.CurriculumAlreadyMapped(command.TuitionPlanId, command.SyllabusId));
        }
        else
        {
            mapping.IsActive = true;
            mapping.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new CreatePackageCurriculumMappingResponse
        {
            Id = mapping.Id,
            TuitionPlanId = tuitionPlan.Id,
            TuitionPlanName = tuitionPlan.Name,
            SyllabusId = syllabus.Id,
            SyllabusCode = syllabus.Code,
            SyllabusVersion = syllabus.Version,
            SyllabusTitle = syllabus.Title,
            IsActive = mapping.IsActive
        };
    }
}
