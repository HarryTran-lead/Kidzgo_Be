using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.LeaveRequests.GetLeaveRequests;

public sealed class GetLeaveRequestsQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetLeaveRequestsQuery, Page<GetLeaveRequestsResponse>>
{
    public async Task<Result<Page<GetLeaveRequestsResponse>>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        Guid? studentProfileId = request.StudentProfileId;
        if (!request.StudentProfileId.HasValue)
        {
            var currentUserRole = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == userContext.UserId)
                .Select(u => u.Role)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentUserRole is UserRole.Parent)
            {
                if (!userContext.StudentId.HasValue)
                {
                    return new Page<GetLeaveRequestsResponse>(
                        new List<GetLeaveRequestsResponse>(),
                        0,
                        request.PageNumber,
                        request.PageSize);
                }

                studentProfileId = userContext.StudentId.Value;
            }
        }

        var query = context.LeaveRequests
            .AsQueryable();

        if (studentProfileId.HasValue)
        {
            query = query.Where(l => l.StudentProfileId == studentProfileId.Value);
        }

        if (request.ClassId.HasValue)
        {
            query = query.Where(l => l.ClassId == request.ClassId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(l => l.Status == request.Status.Value);
        }

        // Apply branch filter
        if (request.BranchId.HasValue)
        {
            query = query.Where(l => l.Class.BranchId == request.BranchId.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.RequestedAt)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .Select(l => new GetLeaveRequestsResponse
            {
                Id = l.Id,
                StudentProfileId = l.StudentProfileId,
                ClassId = l.ClassId,
                ClassName = l.Class.Title,
                SessionId = l.SessionId,
                SessionDate = l.SessionDate,
                EndDate = l.EndDate,
                Reason = l.Reason,
                NoticeHours = l.NoticeHours,
                Status = l.Status.ToString(),
                RequestedAt = l.RequestedAt,
                ApprovedAt = l.ApprovedAt
            })
            .ToListAsync(cancellationToken);

        return new Page<GetLeaveRequestsResponse>(items, total, request.PageNumber, request.PageSize);
    }
}

