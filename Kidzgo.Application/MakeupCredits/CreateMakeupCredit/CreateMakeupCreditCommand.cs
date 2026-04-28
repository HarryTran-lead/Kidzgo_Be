using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.MakeupCredits.Settings;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.MakeupCredits.CreateMakeupCredit;

public sealed class CreateMakeupCreditCommand : ICommand<MakeupCreditResponse>
{
    public Guid StudentProfileId { get; set; }
    public Guid SourceSessionId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public CreatedReason CreatedReason { get; set; } = CreatedReason.LongTerm;
}

public sealed class CreateMakeupCreditCommandHandler(IDbContext context)
    : ICommandHandler<CreateMakeupCreditCommand, MakeupCreditResponse>
{
    public async Task<Result<MakeupCreditResponse>> Handle(
        CreateMakeupCreditCommand command,
        CancellationToken cancellationToken)
    {
        // Ensure profile exists
        bool profileExists = await context.Profiles
            .AnyAsync(p => p.Id == command.StudentProfileId && !p.IsDeleted && p.IsActive, cancellationToken);

        if (!profileExists)
        {
            return Result.Failure<MakeupCreditResponse>(MakeupCreditErrors.NotFound(command.StudentProfileId));
        }

        var sourceSession = await context.Sessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.SourceSessionId, cancellationToken);

        if (sourceSession is null)
        {
            return Result.Failure<MakeupCreditResponse>(MakeupCreditErrors.NotFound(command.SourceSessionId));
        }

        var settings = await MakeupSettingsHelper.GetOrCreateAsync(context, cancellationToken);
        var expiresAt = command.ExpiresAt ?? MakeupSettingsHelper.CalculateExpiresAt(
            sourceSession.PlannedDatetime,
            settings);

        var credit = new MakeupCredit
        {
            Id = Guid.NewGuid(),
            StudentProfileId = command.StudentProfileId,
            SourceSessionId = command.SourceSessionId,
            Status = MakeupCreditStatus.Available,
            CreatedReason = command.CreatedReason,
            ExpiresAt = expiresAt,
            CreatedAt = VietnamTime.UtcNow()
        };

        context.MakeupCredits.Add(credit);
        await context.SaveChangesAsync(cancellationToken);

        return new MakeupCreditResponse
        {
            Id = credit.Id,
            StudentProfileId = credit.StudentProfileId,
            SourceSessionId = credit.SourceSessionId,
            Status = credit.Status.ToString(),
            CreatedReason = credit.CreatedReason.ToString(),
            ExpiresAt = credit.ExpiresAt,
            UsedSessionId = credit.UsedSessionId,
            CreatedAt = credit.CreatedAt
        };
    }
}

