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
            .Include(pt => pt.SecondaryProgramRecommendationProgram)
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

        if (command.SecondaryProgramRecommendationId.HasValue)
        {
            if (command.SecondaryProgramRecommendationId.Value == Guid.Empty)
            {
                placementTest.SecondaryProgramRecommendationId = null;
                placementTest.SecondaryProgramSkillFocus = null;
            }
            else
            {
                var secondaryRecommendedProgram = await GetActiveProgramAsync(command.SecondaryProgramRecommendationId.Value, cancellationToken);
                if (secondaryRecommendedProgram is null)
                {
                    return Result.Failure<UpdatePlacementTestResultsResponse>(
                        Error.NotFound(
                            "PlacementTest.SecondaryProgramRecommendationNotFound",
                            $"Recommended secondary program '{command.SecondaryProgramRecommendationId.Value}' was not found."));
                }

                placementTest.SecondaryProgramRecommendationId = secondaryRecommendedProgram.Id;
                placementTest.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryProgramSkillFocus)
                    ? null
                    : command.SecondaryProgramSkillFocus.Trim();
            }
        }
        else if (command.SecondaryProgramSkillFocus is not null &&
                 placementTest.SecondaryProgramRecommendationId is not null)
        {
            placementTest.SecondaryProgramSkillFocus = string.IsNullOrWhiteSpace(command.SecondaryProgramSkillFocus)
                ? null
                : command.SecondaryProgramSkillFocus.Trim();
        }

        if (placementTest.ProgramRecommendationId.HasValue &&
            placementTest.SecondaryProgramRecommendationId.HasValue &&
            placementTest.ProgramRecommendationId == placementTest.SecondaryProgramRecommendationId)
        {
            return Result.Failure<UpdatePlacementTestResultsResponse>(
                Error.Validation(
                    "PlacementTest.SecondaryProgramDuplicated",
                    "Secondary program recommendation must be different from the primary program recommendation."));
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
            SecondaryProgramRecommendationId = placementTest.SecondaryProgramRecommendationId,
            SecondaryProgramRecommendationName = await GetProgramNameAsync(placementTest.SecondaryProgramRecommendationId, cancellationToken),
            SecondaryProgramSkillFocus = placementTest.SecondaryProgramSkillFocus,
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

        if (pt.StudentProfileId is null || !pt.ProgramRecommendationId.HasValue)
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
                         tp.IsActive &&
                         !tp.IsDeleted &&
                         (!tp.BranchId.HasValue || tp.BranchId == activeReg.BranchId))
            .OrderBy(tp => tp.TuitionAmount)
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
}
