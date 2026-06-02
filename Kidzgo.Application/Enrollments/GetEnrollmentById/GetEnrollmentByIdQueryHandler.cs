using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Services;
using Kidzgo.Domain.Classes.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Enrollments.GetEnrollmentById;

public sealed class GetEnrollmentByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetEnrollmentByIdQuery, GetEnrollmentByIdResponse>
{
    public async Task<Result<GetEnrollmentByIdResponse>> Handle(GetEnrollmentByIdQuery query, CancellationToken cancellationToken)
    {
        var enrollment = await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.Class)
                .ThenInclude(c => c.Branch)
            .Include(e => e.StudentProfile)
            .Include(e => e.TuitionPlan)
            .Include(e => e.ScheduleSegments)
            .FirstOrDefaultAsync(e => e.Id == query.Id, cancellationToken);

        if (enrollment is null)
        {
            return Result.Failure<GetEnrollmentByIdResponse>(
                EnrollmentErrors.NotFound(query.Id));
        }

        var studentBranchState = await context.StudentBranchStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.StudentProfileId == enrollment.StudentProfileId, cancellationToken);

        return new GetEnrollmentByIdResponse
        {
            Id = enrollment.Id,
            ClassId = enrollment.ClassId,
            ClassCode = enrollment.Class.Code,
            ClassTitle = enrollment.Class.Title,
            ProgramId = enrollment.Class.ProgramId,
            ProgramName = enrollment.Class.Program.Name,
            BranchId = enrollment.Class.BranchId,
            BranchName = enrollment.Class.Branch.Name,
            StudentProfileId = enrollment.StudentProfileId,
            StudentName = enrollment.StudentProfile.DisplayName,
            StudentHomeBranchId = studentBranchState?.HomeBranchId,
            StudentActiveBranchId = studentBranchState?.ActiveBranchId,
            IsCrossBranchEnrollment = studentBranchState is not null && studentBranchState.ActiveBranchId != enrollment.Class.BranchId,
            EnrollDate = enrollment.EnrollDate,
            Status = enrollment.Status.ToString(),
            TuitionPlanId = enrollment.TuitionPlanId,
            TuitionPlanName = enrollment.TuitionPlan?.Name,
            WeeklyPattern = ParseWeeklyPatternOrNull(enrollment.SessionSelectionPattern),
            ScheduleSegments = enrollment.ScheduleSegments
                .OrderBy(segment => segment.EffectiveFrom)
                .Select(segment => new EnrollmentScheduleSegmentDto
                {
                    Id = segment.Id,
                    EffectiveFrom = segment.EffectiveFrom,
                    EffectiveTo = segment.EffectiveTo,
                    WeeklyPattern = ParseWeeklyPatternOrNull(segment.SessionSelectionPattern)
                })
                .ToList(),
            CreatedAt = enrollment.CreatedAt,
            UpdatedAt = enrollment.UpdatedAt
        };
    }

    private static List<Kidzgo.Application.Abstraction.Services.WeeklyPatternEntry>? ParseWeeklyPatternOrNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var result = SchedulePatternSupport.ParseWeeklyPattern(value);
        return result.IsSuccess ? result.Value : null;
    }
}

