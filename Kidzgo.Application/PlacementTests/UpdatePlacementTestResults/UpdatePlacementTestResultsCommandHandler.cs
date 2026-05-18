using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.PlacementTests.Shared;
using Kidzgo.Application.Programs.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.CRM;
using Kidzgo.Domain.CRM.Errors;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.PlacementTests.UpdatePlacementTestResults;

public sealed class UpdatePlacementTestResultsCommandHandler(
    IDbContext context
) : ICommandHandler<UpdatePlacementTestResultsCommand, UpdatePlacementTestResultsResponse>
{
    public async Task<Result<UpdatePlacementTestResultsResponse>> Handle(
        UpdatePlacementTestResultsCommand command,
        CancellationToken cancellationToken)
    {
        var placementTest = await context.PlacementTests
            .Include(pt => pt.LeadChild)
            .Include(pt => pt.ProgramRecommendationProgram)
            .Include(pt => pt.PrimaryLevelRecommendationLevel)
            .Include(pt => pt.SecondaryLevelRecommendationLevel)
            .FirstOrDefaultAsync(pt => pt.Id == command.PlacementTestId, cancellationToken);

        if (placementTest is null)
        {
            return Result.Failure<UpdatePlacementTestResultsResponse>(
                PlacementTestErrors.NotFound(command.PlacementTestId));
        }

        if (command.ListeningScore.HasValue)
            placementTest.ListeningScore = command.ListeningScore.Value;

        if (command.SpeakingScore.HasValue)
            placementTest.SpeakingScore = command.SpeakingScore.Value;

        if (command.ReadingScore.HasValue)
            placementTest.ReadingScore = command.ReadingScore.Value;

        if (command.WritingScore.HasValue)
            placementTest.WritingScore = command.WritingScore.Value;

        if (command.ResultScore.HasValue)
            placementTest.ResultScore = command.ResultScore.Value;

        if (command.ProgramRecommendationId.HasValue)
        {
            if (command.ProgramRecommendationId.Value == Guid.Empty)
            {
                placementTest.ProgramRecommendationId = null;
                placementTest.PrimaryLevelRecommendationId = null;
                placementTest.SecondaryLevelRecommendationId = null;
                placementTest.SecondaryProgramSkillFocus = null;
            }
            else
            {
                var recommendedProgram = await GetActiveProgramAsync(command.ProgramRecommendationId.Value, cancellationToken);
                if (recommendedProgram is null)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.NotFound(
                            "PlacementTest.ProgramRecommendationNotFound",
                            $"Recommended primary program '{command.ProgramRecommendationId.Value}' was not found."));
                }

                placementTest.ProgramRecommendationId = recommendedProgram.Id;
            }
        }

        var primaryLevelRecommendationId = NormalizeNullableGuid(command.PrimaryLevelRecommendationId);
        if (command.PrimaryLevelRecommendationId.HasValue)
        {
            if (primaryLevelRecommendationId is null)
            {
                placementTest.PrimaryLevelRecommendationId = null;
            }
            else
            {
                var primaryLevel = await GetActiveLevelAsync(primaryLevelRecommendationId.Value, cancellationToken);
                if (primaryLevel is null)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.NotFound(
                            "PlacementTest.PrimaryLevelRecommendationNotFound",
                            $"Recommended primary level '{primaryLevelRecommendationId.Value}' was not found."));
                }

                if (!placementTest.ProgramRecommendationId.HasValue)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.Validation(
                            "PlacementTest.ProgramRecommendationRequired",
                            "Program recommendation is required when setting primary level recommendation."));
                }

                if (primaryLevel.ProgramId != placementTest.ProgramRecommendationId.Value)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.Validation(
                            "PlacementTest.PrimaryLevelProgramMismatch",
                            "Primary level recommendation must belong to the recommended program."));
                }

                placementTest.PrimaryLevelRecommendationId = primaryLevel.Id;
            }
        }

        var secondaryLevelRecommendationId = NormalizeNullableGuid(command.SecondaryLevelRecommendationId);
        if (command.SecondaryLevelRecommendationId.HasValue)
        {
            if (secondaryLevelRecommendationId is null)
            {
                placementTest.SecondaryLevelRecommendationId = null;
                placementTest.SecondaryProgramSkillFocus = null;
            }
            else
            {
                var secondaryLevel = await GetActiveLevelAsync(secondaryLevelRecommendationId.Value, cancellationToken);
                if (secondaryLevel is null)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.NotFound(
                            "PlacementTest.SecondaryLevelRecommendationNotFound",
                            $"Recommended secondary level '{secondaryLevelRecommendationId.Value}' was not found."));
                }

                if (!placementTest.ProgramRecommendationId.HasValue)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.Validation(
                            "PlacementTest.ProgramRecommendationRequired",
                            "Program recommendation is required when setting secondary level recommendation."));
                }

                if (secondaryLevel.ProgramId != placementTest.ProgramRecommendationId.Value)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.Validation(
                            "PlacementTest.SecondaryLevelProgramMismatch",
                            "Secondary level recommendation must belong to the recommended program."));
                }

                placementTest.SecondaryLevelRecommendationId = secondaryLevel.Id;
                placementTest.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryLevelSkillFocus)
                    ? null
                    : command.SecondaryLevelSkillFocus.Trim();
            }
        }
        else if (command.SecondaryLevelSkillFocus is not null &&
                 placementTest.SecondaryLevelRecommendationId is not null)
        {
            placementTest.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryLevelSkillFocus)
                ? null
                : command.SecondaryLevelSkillFocus.Trim();
        }
        else if (command.SecondaryLevelSkillFocus is not null &&
                 placementTest.SecondaryLevelRecommendationId is null)
        {
            return Result.Failure<UpdatePlacementTestResultsResponse>(
                Error.Validation(
                    "PlacementTest.SecondaryLevelMissing",
                    "Secondary level skill focus can only be set when secondary level recommendation exists."));
        }

        if (placementTest.PrimaryLevelRecommendationId.HasValue &&
            placementTest.SecondaryLevelRecommendationId.HasValue &&
            placementTest.PrimaryLevelRecommendationId == placementTest.SecondaryLevelRecommendationId)
        {
            return Result.Failure<UpdatePlacementTestResultsResponse>(
                Error.Validation(
                    "PlacementTest.SecondaryLevelDuplicated",
                    "Secondary level recommendation must be different from the primary level recommendation."));
        }

        if (command.AttachmentUrls is not null)
        {
            placementTest.AttachmentUrl = PlacementTestAttachmentUrlHelper.Serialize(command.AttachmentUrls);
        }

        var now = VietnamTime.UtcNow();
        Guid? newRegId = null;

        if (placementTest.ListeningScore.HasValue &&
            placementTest.SpeakingScore.HasValue &&
            placementTest.ReadingScore.HasValue &&
            placementTest.WritingScore.HasValue &&
            placementTest.ResultScore.HasValue &&
            placementTest.Status != PlacementTestStatus.Completed)
        {
            placementTest.Status = PlacementTestStatus.Completed;

            if (placementTest.LeadChildId.HasValue && placementTest.LeadChild is not null)
            {
                var leadChild = placementTest.LeadChild;
                if (leadChild.Status == LeadChildStatus.BookedTest)
                {
                    leadChild.Status = LeadChildStatus.TestDone;
                    leadChild.UpdatedAt = now;
                    context.LeadActivities.Add(new LeadActivity
                    {
                        Id = Guid.NewGuid(),
                        LeadId = leadChild.LeadId,
                        ActivityType = ActivityType.Note,
                        Content = $"Child '{leadChild.ChildName}' placement test completed -> status: TestDone",
                        CreatedAt = now,
                        CreatedBy = null
                    });
                }
            }

            if (placementTest.LeadId.HasValue)
            {
                var lead = await context.Leads
                    .FirstOrDefaultAsync(l => l.Id == placementTest.LeadId.Value, cancellationToken);

                if (lead is not null && lead.Status == LeadStatus.BookedTest)
                {
                    lead.Status = LeadStatus.TestDone;
                    lead.UpdatedAt = now;

                    if (!placementTest.LeadChildId.HasValue || placementTest.LeadChild is null)
                    {
                        context.LeadActivities.Add(new LeadActivity
                        {
                            Id = Guid.NewGuid(),
                            LeadId = lead.Id,
                            ActivityType = ActivityType.Note,
                            Content = "Placement test completed -> Lead status: TestDone",
                            CreatedAt = now,
                            CreatedBy = null
                        });
                    }
                }
            }

            newRegId = await AutoCreateRegistrationForRetakeAsync(placementTest, now, cancellationToken);
        }

        placementTest.UpdatedAt = now;
        await context.SaveChangesAsync(cancellationToken);

        var attachmentUrls = PlacementTestAttachmentUrlHelper.Parse(placementTest.AttachmentUrl);

        return new UpdatePlacementTestResultsResponse
        {
            Id = placementTest.Id,
            ListeningScore = placementTest.ListeningScore,
            SpeakingScore = placementTest.SpeakingScore,
            ReadingScore = placementTest.ReadingScore,
            WritingScore = placementTest.WritingScore,
            ResultScore = placementTest.ResultScore,
            ProgramRecommendationId = placementTest.ProgramRecommendationId,
            ProgramRecommendationName = await GetProgramNameAsync(placementTest.ProgramRecommendationId, cancellationToken),
            PrimaryLevelRecommendationId = placementTest.PrimaryLevelRecommendationId,
            PrimaryLevelRecommendationName = await GetLevelNameAsync(placementTest.PrimaryLevelRecommendationId, cancellationToken),
            SecondaryLevelRecommendationId = placementTest.SecondaryLevelRecommendationId,
            SecondaryLevelRecommendationName = await GetLevelNameAsync(placementTest.SecondaryLevelRecommendationId, cancellationToken),
            SecondaryLevelSkillFocus = placementTest.SecondaryProgramSkillFocus,
            AttachmentUrl = attachmentUrls.FirstOrDefault(),
            AttachmentUrls = attachmentUrls,
            Status = placementTest.Status.ToString(),
            UpdatedAt = placementTest.UpdatedAt,
            NewRegistrationId = newRegId
        };
    }

    private async Task<Guid?> AutoCreateRegistrationForRetakeAsync(
        Domain.CRM.PlacementTest pt,
        DateTime now,
        CancellationToken cancellationToken)
    {
        // Chi tao Registration moi neu PlacementTest nay co OriginalPlacementTestId (nghia la day la retake)
        if (pt.OriginalPlacementTestId is null)
            return null;

        if (pt.StudentProfileId is null ||
            !pt.ProgramRecommendationId.HasValue ||
            !pt.PrimaryLevelRecommendationId.HasValue)
            return null;

        var activeReg = await context.Registrations
            .Include(r => r.Program).Include(r => r.TuitionPlan).Include(r => r.Branch)
            .FirstOrDefaultAsync(r =>
                r.StudentProfileId == pt.StudentProfileId.Value &&
                r.Status != RegistrationStatus.Completed &&
                r.Status != RegistrationStatus.Cancelled,
                cancellationToken);

        if (activeReg is null)
            return null;

        var targetProgram = await context.Programs
            .FirstOrDefaultAsync(p =>
                p.Id == pt.ProgramRecommendationId.Value && p.IsActive && !p.IsDeleted,
                cancellationToken);

        if (targetProgram is null)
            return null;

        var programAssignedToBranch = await BranchProgramAccessHelper.IsProgramAssignedToBranchAsync(
            context,
            activeReg.BranchId,
            targetProgram.Id,
            cancellationToken);

        if (!programAssignedToBranch)
            return null;

        var targetTuitionPlan = await context.TuitionPlans
            .Where(tp => tp.ProgramId == targetProgram.Id &&
                         (tp.LevelId == pt.PrimaryLevelRecommendationId.Value || tp.LevelId == null) &&
                         tp.IsActive &&
                         !tp.IsDeleted)
            .OrderByDescending(tp => tp.LevelId == pt.PrimaryLevelRecommendationId.Value)
            .ThenBy(tp => tp.TuitionAmount)
            .FirstOrDefaultAsync(cancellationToken);

        if (targetTuitionPlan is null)
            return null;

        var remainingSessions = activeReg.RemainingSessions;
        var originalProgramName = activeReg.Program.Name;
        var originalRegId = activeReg.Id;

        activeReg.Status = RegistrationStatus.Completed;
        activeReg.UpdatedAt = now;

        var newReg = new Registration
        {
            Id = Guid.NewGuid(),
            StudentProfileId = pt.StudentProfileId.Value,
            BranchId = activeReg.BranchId,
            ProgramId = targetProgram.Id,
            LevelId = pt.PrimaryLevelRecommendationId.Value,
            SecondaryLevelId = pt.SecondaryLevelRecommendationId,
            SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(pt.SecondaryProgramSkillFocus)
                ? null
                : pt.SecondaryProgramSkillFocus.Trim(),
            TuitionPlanId = targetTuitionPlan.Id,
            RegistrationDate = now,
            ExpectedStartDate = now,
            PreferredSchedule = activeReg.PreferredSchedule,
            Note = $"Thi lai tu chuong trinh '{originalProgramName}' len '{targetProgram.Name}'. Giu lai {remainingSessions} buoi con lai. PlacementTest retake ID: {pt.Id}.",
            Status = RegistrationStatus.WaitingForClass,
            ClassId = null,
            ClassAssignedDate = null,
            EntryType = EntryType.Retake,
            OriginalRegistrationId = originalRegId,
            OperationType = OperationType.Retake,
            TotalSessions = remainingSessions,
            UsedSessions = 0,
            RemainingSessions = remainingSessions,
            OriginalTuitionAmount = 0m,
            DiscountAmount = 0m,
            CarryOverCreditAmount = 0m,
            FinalTuitionAmount = 0m,
            PricingAppliedAt = now,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.Registrations.Add(newReg);

        if (pt.LeadId.HasValue)
        {
            context.LeadActivities.Add(new LeadActivity
            {
                Id = Guid.NewGuid(),
                LeadId = pt.LeadId.Value,
                ActivityType = ActivityType.Note,
                Content = $"Student retake placement test completed. Original program: '{originalProgramName}' ({remainingSessions} sessions remaining). New program: '{targetProgram.Name}'. New Registration ID: {newReg.Id}.",
                CreatedAt = now
            });
        }

        return newReg.Id;
    }

    private Task<Kidzgo.Domain.Programs.Program?> GetActiveProgramAsync(Guid programId, CancellationToken cancellationToken)
    {
        return context.Programs
            .FirstOrDefaultAsync(p => p.Id == programId && p.IsActive && !p.IsDeleted, cancellationToken);
    }

    private Task<Kidzgo.Domain.Programs.Level?> GetActiveLevelAsync(Guid levelId, CancellationToken cancellationToken)
    {
        return context.Levels
            .FirstOrDefaultAsync(l => l.Id == levelId && l.IsActive, cancellationToken);
    }

    private async Task<string?> GetProgramNameAsync(Guid? programId, CancellationToken cancellationToken)
    {
        if (!programId.HasValue)
        {
            return null;
        }

        return await context.Programs
            .Where(p => p.Id == programId.Value)
            .Select(p => p.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<string?> GetLevelNameAsync(Guid? levelId, CancellationToken cancellationToken)
    {
        if (!levelId.HasValue)
        {
            return null;
        }

        return await context.Levels
            .Where(l => l.Id == levelId.Value)
            .Select(l => l.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Guid? NormalizeNullableGuid(Guid? value)
    {
        if (!value.HasValue || value.Value == Guid.Empty)
        {
            return null;
        }

        return value.Value;
    }
}
