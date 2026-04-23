using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Programs.UpdateProgram;

public sealed class UpdateProgramCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateProgramCommand, UpdateProgramResponse>
{
    public async Task<Result<UpdateProgramResponse>> Handle(UpdateProgramCommand command, CancellationToken cancellationToken)
    {
        var program = await context.Programs
            .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted, cancellationToken);

        if (program is null)
        {
            return Result.Failure<UpdateProgramResponse>(ProgramErrors.NotFound(command.Id));
        }

        program.Name = command.Name;
        program.Code = command.Code;
        program.IsMakeup = command.IsMakeup;
        program.IsSupplementary = command.IsSupplementary;
        program.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return new UpdateProgramResponse
        {
            Id = program.Id,
            Name = program.Name,
            Code = program.Code,
            IsMakeup = program.IsMakeup,
            IsSupplementary = program.IsSupplementary,
            DefaultTuitionAmount = 0,
            UnitPriceSession = 0,
            Description = program.Description,
            IsActive = program.IsActive
        };
    }
}
