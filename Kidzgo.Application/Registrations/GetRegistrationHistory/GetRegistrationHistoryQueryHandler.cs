using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.AuditLogs;
using Kidzgo.Application.AuditLogs.GetAuditLogs;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetRegistrationHistory;

public sealed class GetRegistrationHistoryQueryHandler(
    IDbContext context
) : IQueryHandler<GetRegistrationHistoryQuery, GetRegistrationHistoryResponse>
{
    public async Task<Result<GetRegistrationHistoryResponse>> Handle(
        GetRegistrationHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var registrationExists = await context.Registrations
            .AsNoTracking()
            .AnyAsync(r => r.Id == query.RegistrationId, cancellationToken);

        if (!registrationExists)
        {
            return Result.Failure<GetRegistrationHistoryResponse>(
                RegistrationErrors.NotFound(query.RegistrationId));
        }

        var auditLogsQuery = context.AuditLogs
            .AsNoTracking()
            .Include(a => a.ActorUser)
            .Include(a => a.ActorProfile)
            .Where(a => a.EntityType == RegistrationAuditLogHelper.EntityType &&
                        a.EntityId == query.RegistrationId);

        var totalCount = await auditLogsQuery.CountAsync(cancellationToken);

        var auditLogs = await auditLogsQuery
            .OrderByDescending(a => a.CreatedAt)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .ToListAsync(cancellationToken);

        var page = new Page<AuditLogDto>(
            auditLogs.Select(AuditLogContractMapper.ToDto).ToList(),
            totalCount,
            query.PageNumber,
            query.PageSize);

        return Result.Success(new GetRegistrationHistoryResponse
        {
            RegistrationId = query.RegistrationId,
            History = page
        });
    }
}
