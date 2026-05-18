using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PlacementTests.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.GetPlacementTestById;

public sealed class GetPlacementTestByIdQueryHandler(
    IDbContext context
) : IQueryHandler<GetPlacementTestByIdQuery, GetPlacementTestByIdResponse>
{
    public async Task<Result<GetPlacementTestByIdResponse>> Handle(
        GetPlacementTestByIdQuery query,
        CancellationToken cancellationToken)
    {
        // UC-028: Get Placement Test by ID
        var placementTest = await context.PlacementTests
            .Include(pt => pt.Lead)
            .Include(pt => pt.LeadChild)
            .Include(pt => pt.StudentProfile)
            .Include(pt => pt.Class)
            .Include(pt => pt.PlacementRoom)
            .Include(pt => pt.InvigilatorUser)
            .Include(pt => pt.ProgramRecommendationProgram)
            .Include(pt => pt.PrimaryLevelRecommendationLevel)
            .Include(pt => pt.SecondaryLevelRecommendationLevel)
            .FirstOrDefaultAsync(pt => pt.Id == query.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<GetPlacementTestByIdResponse>(
                PlacementTestErrors.NotFound(query.PlacementTestId));
        }

        var attachmentUrls = PlacementTestAttachmentUrlHelper.Parse(placementTest.AttachmentUrl);

        return new GetPlacementTestByIdResponse
        {
            Id = placementTest.Id,
            LeadId = placementTest.LeadId,
            LeadChildId = placementTest.LeadChildId,
            LeadContactName = placementTest.Lead?.ContactName,
            ChildName = placementTest.LeadChild?.ChildName,
            StudentProfileId = placementTest.StudentProfileId,
            StudentName = placementTest.StudentProfile?.DisplayName,
            ClassId = placementTest.ClassId,
            ClassName = placementTest.Class?.Title,
            ScheduledAt = placementTest.ScheduledAt,
            DurationMinutes = placementTest.DurationMinutes,
            Status = placementTest.Status.ToString(),
            RoomId = placementTest.RoomId,
            RoomName = placementTest.PlacementRoom?.Name,
            Room = placementTest.Room,
            InvigilatorUserId = placementTest.InvigilatorUserId,
            InvigilatorName = placementTest.InvigilatorUser?.Name,
            ResultScore = placementTest.ResultScore,
            ListeningScore = placementTest.ListeningScore,
            SpeakingScore = placementTest.SpeakingScore,
            ReadingScore = placementTest.ReadingScore,
            WritingScore = placementTest.WritingScore,
            ProgramRecommendationId = placementTest.ProgramRecommendationId,
            ProgramRecommendationName = placementTest.ProgramRecommendationProgram?.Name,
            PrimaryLevelRecommendationId = placementTest.PrimaryLevelRecommendationId,
            PrimaryLevelRecommendationName = placementTest.PrimaryLevelRecommendationLevel?.Name,
            SecondaryLevelRecommendationId = placementTest.SecondaryLevelRecommendationId,
            SecondaryLevelRecommendationName = placementTest.SecondaryLevelRecommendationLevel?.Name,
            SecondaryLevelSkillFocus = placementTest.SecondaryProgramSkillFocus,
            Notes = placementTest.Notes,
            AttachmentUrl = attachmentUrls.FirstOrDefault(),
            AttachmentUrls = attachmentUrls,
            IsAccountProfileCreated = placementTest.StudentProfileId.HasValue ||
                                      placementTest.LeadChild?.ConvertedStudentProfileId.HasValue == true,
            IsConvertedToEnrolled = placementTest.LeadChildId.HasValue
                ? placementTest.LeadChild?.Status == LeadChildStatus.Enrolled
                : placementTest.Lead?.Status == LeadStatus.Enrolled,
            CreatedAt = placementTest.CreatedAt,
            UpdatedAt = placementTest.UpdatedAt
        };
    }
}

