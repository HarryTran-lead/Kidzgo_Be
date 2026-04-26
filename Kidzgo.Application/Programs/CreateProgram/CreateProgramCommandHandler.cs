using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Programs;

namespace Kidzgo.Application.Programs.CreateProgram;

public sealed class CreateProgramCommandHandler(
    IDbContext context
) : ICommandHandler<CreateProgramCommand, CreateProgramResponse>
{
    public async Task<Result<CreateProgramResponse>> Handle(CreateProgramCommand command, CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();
        var program = new Program
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Code = command.Code,
            Description = command.Description?.Trim(),
            IsMakeup = command.IsMakeup,
            IsSupplementary = command.IsSupplementary,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Programs.Add(program);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateProgramResponse
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
