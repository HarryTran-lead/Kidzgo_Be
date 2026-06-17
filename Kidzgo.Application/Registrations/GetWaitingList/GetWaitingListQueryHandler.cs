using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetWaitingList.Handler;

public sealed class GetWaitingListQueryHandler(
    IDbContext context
) : IQueryHandler<GetWaitingListQuery, GetWaitingListResponse>
{
    public async Task<Result<GetWaitingListResponse>> Handle(
        GetWaitingListQuery query,
        CancellationToken cancellationToken)
    {
        var track = string.IsNullOrWhiteSpace(query.Track)
            ? null
            : RegistrationTrackHelper.NormalizeTrack(query.Track);

        var registrations = await context.Registrations
            .Include(r => r.StudentProfile)
            .Include(r => r.Branch)
            .Include(r => r.Program)
            .Include(r => r.SecondaryLevel)
            .Include(r => r.TuitionPlan)
            .ToListAsync(cancellationToken);

        if (query.BranchId.HasValue)
        {
            registrations = registrations
                .Where(r => r.BranchId == query.BranchId.Value)
                .ToList();
        }

        var now = VietnamTime.UtcNow();
        var items = new List<WaitingListItemDto>();

        foreach (var registration in registrations)
        {
            if (registration.Status == RegistrationStatus.Completed || registration.Status == RegistrationStatus.Cancelled)
            {
                continue;
            }

            if ((track is null || track == RegistrationTrackHelper.PrimaryTrack) &&
                registration.ClassId is null &&
                (!query.LevelId.HasValue || registration.LevelId == query.LevelId.Value) &&
                (!query.ProgramId.HasValue || registration.ProgramId == query.ProgramId.Value))
            {
                items.Add(new WaitingListItemDto
                {
                    Id = registration.Id,
                    StudentProfileId = registration.StudentProfileId,
                    StudentName = registration.StudentProfile.Name ?? string.Empty,
                    BranchId = registration.BranchId,
                    BranchName = registration.Branch.Name,
                    Track = RegistrationTrackHelper.PrimaryTrack,
                    ProgramId = registration.ProgramId,
                    ProgramName = registration.Program.Name,
                    IsSupplementaryProgram = registration.Program.IsSupplementary,
                    ProgramSkillFocus = null,
                    TuitionPlanId = registration.TuitionPlanId,
                    TuitionPlanName = registration.TuitionPlan.Name,
                    RegistrationDate = registration.RegistrationDate,
                    ExpectedStartDate = registration.ExpectedStartDate,
                    PreferredSchedule = registration.PreferredSchedule,
                    RegistrationStatus = registration.Status.ToString(),
                    DaysWaiting = (int)(now - registration.RegistrationDate).TotalDays
                });
            }

            if ((track is null || track == RegistrationTrackHelper.SecondaryTrack) &&
                registration.SecondaryLevelId.HasValue &&
                registration.SecondaryClassId is null &&
                (!query.LevelId.HasValue || registration.SecondaryLevelId == query.LevelId.Value) &&
                (!query.ProgramId.HasValue || registration.ProgramId == query.ProgramId.Value))
            {
                items.Add(new WaitingListItemDto
                {
                    Id = registration.Id,
                    StudentProfileId = registration.StudentProfileId,
                    StudentName = registration.StudentProfile.Name ?? string.Empty,
                    BranchId = registration.BranchId,
                    BranchName = registration.Branch.Name,
                    Track = RegistrationTrackHelper.SecondaryTrack,
                    ProgramId = registration.ProgramId,
                    ProgramName = registration.Program.Name,
                    IsSupplementaryProgram = registration.Program.IsSupplementary,
                    ProgramSkillFocus = string.IsNullOrWhiteSpace(registration.SecondaryProgramSkillFocus)
                        ? registration.SecondaryLevel?.Name
                        : registration.SecondaryProgramSkillFocus,
                    TuitionPlanId = registration.TuitionPlanId,
                    TuitionPlanName = registration.TuitionPlan.Name,
                    RegistrationDate = registration.RegistrationDate,
                    ExpectedStartDate = registration.ExpectedStartDate,
                    PreferredSchedule = registration.PreferredSchedule,
                    RegistrationStatus = registration.Status.ToString(),
                    DaysWaiting = (int)(now - registration.RegistrationDate).TotalDays
                });
            }
        }

        items = items
            .OrderBy(i => i.ExpectedStartDate ?? i.RegistrationDate)
            .ToList();

        var totalCount = items.Count;
        items = items
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new GetWaitingListResponse
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }
}
