using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Sessions.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Sessions.UpdateSessionSectionType;

public sealed class UpdateSessionSectionTypeCommandHandler(
    IDbContext context
) : ICommandHandler<UpdateSessionSectionTypeCommand, UpdateSessionSectionTypeResponse>
{
    public async Task<Result<UpdateSessionSectionTypeResponse>> Handle(
        UpdateSessionSectionTypeCommand command,
        CancellationToken cancellationToken)
    {
        var session = await context.Sessions
            .FirstOrDefaultAsync(s => s.Id == command.SessionId, cancellationToken);

        if (session is null)
        {
            return Result.Failure<UpdateSessionSectionTypeResponse>(SessionErrors.NotFound(command.SessionId));
        }

        if (session.Status is SessionStatus.Cancelled or SessionStatus.Completed)
        {
            return Result.Failure<UpdateSessionSectionTypeResponse>(SessionErrors.InvalidStatus);
        }

        if (!command.IsPrivilegedUser)
        {
            var sessionDate = VietnamTime.ToVietnamDateOnly(session.ActualDatetime ?? session.PlannedDatetime);
            var today = VietnamTime.TodayDateOnly();

            if (sessionDate != today)
            {
                return Result.Failure<UpdateSessionSectionTypeResponse>(
                    SessionErrors.TeacherCanOnlyChangeSectionTypeOnSessionDate(session.Id, sessionDate, today));
            }
        }

        session.SectionType = command.SectionType;
        session.UpdatedAt = VietnamTime.UtcNow();

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateSessionSectionTypeResponse
        {
            Id = session.Id,
            SectionType = session.SectionType.ToString(),
            UpdatedAt = session.UpdatedAt
        });
    }
}
