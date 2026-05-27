using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.LessonPlans.Errors;
using Kidzgo.Domain.Schools.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Branches.UpsertBranchSyllabus;

public sealed class UpsertBranchSyllabusCommandHandler(IDbContext context)
    : ICommandHandler<UpsertBranchSyllabusCommand, UpsertBranchSyllabusResponse>
{
    public async Task<Result<UpsertBranchSyllabusResponse>> Handle(
        UpsertBranchSyllabusCommand command,
        CancellationToken cancellationToken)
    {
        if (command.EffectiveFrom.HasValue &&
            command.EffectiveTo.HasValue &&
            command.EffectiveFrom.Value > command.EffectiveTo.Value)
        {
            return Result.Failure<UpsertBranchSyllabusResponse>(
                CurriculumAssignmentErrors.InvalidEffectiveRange(
                    command.EffectiveFrom.Value,
                    command.EffectiveTo.Value));
        }

        var branch = await context.Branches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == command.BranchId, cancellationToken);

        if (branch is null)
        {
            return Result.Failure<UpsertBranchSyllabusResponse>(BranchErrors.NotFound(command.BranchId));
        }

        var syllabus = await context.Syllabuses
            .AsNoTracking()
            .Include(x => x.Program)
            .Include(x => x.Level)
            .FirstOrDefaultAsync(
                x => x.Id == command.SyllabusId && !x.IsDeleted,
                cancellationToken);

        if (syllabus is null)
        {
            return Result.Failure<UpsertBranchSyllabusResponse>(SyllabusErrors.NotFound(command.SyllabusId));
        }

        if (!syllabus.IsActive)
        {
            return Result.Failure<UpsertBranchSyllabusResponse>(
                CurriculumAssignmentErrors.SyllabusInactive(command.SyllabusId));
        }

        var now = VietnamTime.UtcNow();
        var assignment = await context.CurriculumAssignments
            .FirstOrDefaultAsync(
                x => x.BranchId == command.BranchId && x.SyllabusId == command.SyllabusId,
                cancellationToken);

        if (assignment is null)
        {
            assignment = new CurriculumAssignment
            {
                Id = Guid.NewGuid(),
                BranchId = command.BranchId,
                ProgramId = syllabus.ProgramId,
                LevelId = syllabus.LevelId,
                SyllabusId = syllabus.Id,
                EffectiveFrom = command.EffectiveFrom,
                EffectiveTo = command.EffectiveTo,
                IsActive = command.IsActive,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.CurriculumAssignments.Add(assignment);
        }
        else
        {
            assignment.ProgramId = syllabus.ProgramId;
            assignment.LevelId = syllabus.LevelId;
            assignment.EffectiveFrom = command.EffectiveFrom;
            assignment.EffectiveTo = command.EffectiveTo;
            assignment.IsActive = command.IsActive;
            assignment.UpdatedAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);

        return new UpsertBranchSyllabusResponse
        {
            CurriculumAssignmentId = assignment.Id,
            BranchId = assignment.BranchId,
            SyllabusId = assignment.SyllabusId,
            ProgramId = syllabus.ProgramId,
            ProgramName = syllabus.Program.Name,
            LevelId = syllabus.LevelId,
            LevelName = syllabus.Level.Name,
            Code = syllabus.Code,
            Version = syllabus.Version,
            Title = syllabus.Title,
            EffectiveFrom = assignment.EffectiveFrom,
            EffectiveTo = assignment.EffectiveTo,
            IsActive = assignment.IsActive
        };
    }
}
