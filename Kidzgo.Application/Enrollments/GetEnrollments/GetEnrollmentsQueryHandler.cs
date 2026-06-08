using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.GetEnrollments;

public sealed class GetEnrollmentsQueryHandler(
    IDbContext context
) : IQueryHandler<GetEnrollmentsQuery, GetEnrollmentsResponse>
{
    public async Task<Result<GetEnrollmentsResponse>> Handle(GetEnrollmentsQuery query, CancellationToken cancellationToken)
    {
        var enrollmentsQuery = context.ClassEnrollments
            .Include(e => e.Class)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .AsQueryable();

        // Filter by class
        if (query.ClassId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => e.ClassId == query.ClassId.Value);
        }

        // Filter by student
        if (query.StudentProfileId.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => e.StudentProfileId == query.StudentProfileId.Value);
        }

        // Filter by status
        if (query.Status.HasValue)
        {
            enrollmentsQuery = enrollmentsQuery.Where(e => e.Status == query.Status.Value);
        }

        // Get total count
        int totalCount = await enrollmentsQuery.CountAsync(cancellationToken);

        // Apply pagination
        var enrollments = await enrollmentsQuery
            .OrderByDescending(e => e.EnrollDate)
            .ApplyPagination(query.PageNumber, query.PageSize)
            .Select(e => new EnrollmentDto
            {
                Id = e.Id,
                ClassId = e.ClassId,
                ClassCode = e.Class.Code,
                ClassTitle = e.Class.Title,
                StudentProfileId = e.StudentProfileId,
                StudentName = e.StudentProfile.DisplayName,
                RegistrationId = e.RegistrationId,
                EnrollDate = e.EnrollDate,
                Status = e.Status.ToString(),
                TuitionPlanId = e.TuitionPlanId,
                TuitionPlanName = e.TuitionPlan != null ? e.TuitionPlan.Name : null
            })
            .ToListAsync(cancellationToken);

        var page = new Page<EnrollmentDto>(
            enrollments,
            totalCount,
            query.PageNumber,
            query.PageSize);

        return new GetEnrollmentsResponse
        {
            Enrollments = page
        };
    }
}

