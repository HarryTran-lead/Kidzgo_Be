using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.GetProgramById;

public sealed class GetProgramByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetProgramByIdQuery, GetProgramByIdResponse>
{
    public async Task<Result<GetProgramByIdResponse>> Handle(GetProgramByIdQuery query, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .Where(p => p.Id == query.Id && !p.IsDeleted)
            .Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                IsMakeup = p.IsMakeup,
                IsSupplementary = p.IsSupplementary,
                Code = p.Code,
                BaseFee = p.TuitionPlans
                    .Where(tp => tp.IsActive && !tp.IsDeleted)
                    .Select(tp => (decimal?)tp.TuitionAmount)
                    .Min() ?? 0,
                Fee = p.TuitionPlans
                    .Where(tp => tp.IsActive && !tp.IsDeleted)
                    .Select(tp => (decimal?)tp.TuitionAmount)
                    .Min() ?? 0,
                Description = p.Description,
                IsActive = p.IsActive,
                ClassCount = p.Classes.Count(c => c.Status != Domain.Classes.ClassStatus.Cancelled),
                StudentCount = p.Classes
                    .SelectMany(c => c.ClassEnrollments)
                    .Count(ce => ce.Status == Domain.Classes.EnrollmentStatus.Active)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (program is null)
        {
            return Result.Failure<GetProgramByIdResponse>(ProgramErrors.NotFound(query.Id));
        }

        var branchAssignments = await context.BranchPrograms
            .Where(bp => bp.ProgramId == program.Id && bp.IsActive)
            .OrderBy(bp => bp.Branch.Name)
            .Select(bp => new ProgramBranchAssignmentDto
            {
                BranchId = bp.BranchId,
                BranchName = bp.Branch.Name,
                IsActive = bp.IsActive,
                DefaultMakeupClassId = bp.DefaultMakeupClassId
            })
            .ToListAsync(cancellationToken);

        return new GetProgramByIdResponse
        {
            Id = program.Id,
            Name = program.Name,
            Code = program.Code,
            IsMakeup = program.IsMakeup,
            IsSupplementary = program.IsSupplementary,
            BaseFee = program.BaseFee,
            Fee = program.Fee,
            Description = program.Description,
            IsActive = program.IsActive,
            BranchAssignments = branchAssignments,
            ClassCount = program.ClassCount,
            StudentCount = program.StudentCount
        };
    }
}
