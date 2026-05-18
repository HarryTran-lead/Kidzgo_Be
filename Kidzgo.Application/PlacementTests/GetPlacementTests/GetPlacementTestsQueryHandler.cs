using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Query;
using Kidzgo.Application.PlacementTests.Shared;
using Kidzgo.Application.Time;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.GetPlacementTests;

public sealed class GetPlacementTestsQueryHandler(
    IDbContext context
) : IQueryHandler<GetPlacementTestsQuery, GetPlacementTestsResponse>
{
    public async Task<Result<GetPlacementTestsResponse>> Handle(
        GetPlacementTestsQuery query,
        CancellationToken cancellationToken)
    {
        // UC-028: Query Placement Tests with filters
        var placementTestsQuery = context.PlacementTests.AsQueryable();

        if (query.LeadId.HasValue)
        {
            placementTestsQuery = placementTestsQuery.Where(pt => pt.LeadId == query.LeadId.Value);
        }

        if (query.StudentProfileId.HasValue)
        {
            placementTestsQuery = placementTestsQuery.Where(pt => pt.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.Status.HasValue)
        {
            placementTestsQuery = placementTestsQuery.Where(pt => pt.Status == query.Status.Value);
        }

        if (query.FromDate.HasValue)
        {
            var fromDateUtc = VietnamTime.NormalizeToUtc(query.FromDate.Value);
            placementTestsQuery = placementTestsQuery.Where(pt => pt.ScheduledAt >= fromDateUtc);
        }

        if (query.ToDate.HasValue)
        {
            var toDateUtc = VietnamTime.EndOfVietnamDayUtc(VietnamTime.NormalizeToUtc(query.ToDate.Value));
            placementTestsQuery = placementTestsQuery.Where(pt => pt.ScheduledAt <= toDateUtc);
        }

        placementTestsQuery = ApplySorting(placementTestsQuery, query);

        var totalCount = await placementTestsQuery.CountAsync(cancellationToken);

        var placementTestEntities = await placementTestsQuery
            .Include(pt => pt.Lead)
            .Include(pt => pt.LeadChild)
            .Include(pt => pt.StudentProfile)
            .Include(pt => pt.Class)
            .Include(pt => pt.PlacementRoom)
            .Include(pt => pt.InvigilatorUser)
            .Include(pt => pt.ProgramRecommendationProgram)
            .Include(pt => pt.PrimaryLevelRecommendationLevel)
            .Include(pt => pt.SecondaryLevelRecommendationLevel)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var placementTests = placementTestEntities
            .Select(pt =>
            {
                var attachmentUrls = PlacementTestAttachmentUrlHelper.Parse(pt.AttachmentUrl);

                return new PlacementTestDto
                {
                    Id = pt.Id,
                    LeadId = pt.LeadId,
                    LeadChildId = pt.LeadChildId,
                    LeadContactName = pt.Lead?.ContactName,
                    ChildName = pt.LeadChild?.ChildName,
                    StudentProfileId = pt.StudentProfileId,
                    StudentName = pt.StudentProfile?.DisplayName,
                    ClassId = pt.ClassId,
                    ClassName = pt.Class?.Title,
                    ScheduledAt = pt.ScheduledAt,
                    DurationMinutes = pt.DurationMinutes,
                    Status = pt.Status.ToString(),
                    RoomId = pt.RoomId,
                    RoomName = pt.PlacementRoom?.Name,
                    Room = pt.Room,
                    InvigilatorUserId = pt.InvigilatorUserId,
                    InvigilatorName = pt.InvigilatorUser?.Name,
                    ResultScore = pt.ResultScore,
                    ListeningScore = pt.ListeningScore,
                    SpeakingScore = pt.SpeakingScore,
                    ReadingScore = pt.ReadingScore,
                    WritingScore = pt.WritingScore,
                    ProgramRecommendationId = pt.ProgramRecommendationId,
                    ProgramRecommendationName = pt.ProgramRecommendationProgram?.Name,
                    PrimaryLevelRecommendationId = pt.PrimaryLevelRecommendationId,
                    PrimaryLevelRecommendationName = pt.PrimaryLevelRecommendationLevel?.Name,
                    SecondaryLevelRecommendationId = pt.SecondaryLevelRecommendationId,
                    SecondaryLevelRecommendationName = pt.SecondaryLevelRecommendationLevel?.Name,
                    SecondaryLevelSkillFocus = pt.SecondaryProgramSkillFocus,
                    Notes = pt.Notes,
                    AttachmentUrl = attachmentUrls.FirstOrDefault(),
                    AttachmentUrls = attachmentUrls,
                    IsAccountProfileCreated = pt.StudentProfileId.HasValue ||
                                              pt.LeadChild?.ConvertedStudentProfileId.HasValue == true,
                    IsConvertedToEnrolled = pt.LeadChildId.HasValue
                        ? pt.LeadChild?.Status == LeadChildStatus.Enrolled
                        : pt.Lead?.Status == LeadStatus.Enrolled,
                    CreatedAt = pt.CreatedAt,
                    UpdatedAt = pt.UpdatedAt
                };
            })
            .ToList();

        return Result.Success(new GetPlacementTestsResponse
        {
            PlacementTests = placementTests,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        });
    }

    private static IQueryable<PlacementTest> ApplySorting(
        IQueryable<PlacementTest> placementTestsQuery,
        GetPlacementTestsQuery query)
    {
        if (string.Equals(query.SortBy, "branch", StringComparison.OrdinalIgnoreCase))
        {
            return query.SortOrder == SortOrder.Ascending
                ? placementTestsQuery
                    .OrderBy(pt => pt.ClassId.HasValue
                        ? pt.Class != null && pt.Class.Branch != null
                            ? pt.Class.Branch.Name
                            : string.Empty
                        : pt.Lead != null && pt.Lead.BranchPreferenceNavigation != null
                            ? pt.Lead.BranchPreferenceNavigation.Name
                            : string.Empty)
                    .ThenByDescending(pt => pt.ScheduledAt ?? pt.CreatedAt)
                : placementTestsQuery
                    .OrderByDescending(pt => pt.ClassId.HasValue
                        ? pt.Class != null && pt.Class.Branch != null
                            ? pt.Class.Branch.Name
                            : string.Empty
                        : pt.Lead != null && pt.Lead.BranchPreferenceNavigation != null
                            ? pt.Lead.BranchPreferenceNavigation.Name
                            : string.Empty)
                    .ThenByDescending(pt => pt.ScheduledAt ?? pt.CreatedAt);
        }

        return query.SortOrder == SortOrder.Ascending
            ? placementTestsQuery.OrderBy(pt => pt.ScheduledAt ?? pt.CreatedAt)
            : placementTestsQuery.OrderByDescending(pt => pt.ScheduledAt ?? pt.CreatedAt);
    }
}

