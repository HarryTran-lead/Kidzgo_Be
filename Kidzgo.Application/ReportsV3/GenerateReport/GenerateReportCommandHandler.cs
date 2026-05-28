using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.Globalization;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.ReportsV3.Shared;
using Kidzgo.Domain.AcademicProgression;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.LearningTickets;
using Kidzgo.Domain.Reports;
using Kidzgo.Domain.Sessions;
using Kidzgo.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Class = Kidzgo.Domain.Classes.Class;
using Module = Kidzgo.Domain.Programs.Module;

namespace Kidzgo.Application.ReportsV3.GenerateReport;

public sealed class GenerateReportCommandHandler(
    IDbContext context,
    IUserContext userContext)
    : ICommandHandler<GenerateReportCommand, GenerateReportResponse>
{
    public async Task<Result<GenerateReportResponse>> Handle(
        GenerateReportCommand command,
        CancellationToken cancellationToken)
    {
        var now = VietnamTime.UtcNow();

        var currentUser = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);

        if (currentUser is null)
        {
            return Result.Failure<GenerateReportResponse>(
                Error.NotFound("Report.UserNotFound", "Current user was not found."));
        }

        if (currentUser.Role is not (UserRole.Admin or UserRole.ManagementStaff or UserRole.Teacher))
        {
            return Result.Failure<GenerateReportResponse>(
                Error.Unauthorized("Report.GenerateForbidden", "Current role cannot generate reports."));
        }

        var period = await context.ReportPeriods
            .FirstOrDefaultAsync(p => p.Id == command.PeriodId, cancellationToken);

        if (period is null)
        {
            return Result.Failure<GenerateReportResponse>(
                Error.NotFound("Report.PeriodNotFound", "Report period was not found."));
        }

        var student = await context.Profiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(
                p => p.Id == command.StudentId &&
                     p.ProfileType == ProfileType.Student &&
                     p.IsActive &&
                     !p.IsDeleted,
                cancellationToken);

        if (student is null)
        {
            return Result.Failure<GenerateReportResponse>(
                Error.NotFound("Report.StudentNotFound", "Student profile was not found."));
        }

        var reportClass = await ResolveClassAsync(command.ClassId, student.Id, cancellationToken);
        if (reportClass is null)
        {
            return Result.Failure<GenerateReportResponse>(
                Error.NotFound("Report.ClassNotFound", "Class context was not found for report generation."));
        }

        if (currentUser.Role == UserRole.Teacher &&
            reportClass.MainTeacherId != currentUser.Id &&
            reportClass.AssistantTeacherId != currentUser.Id)
        {
            return Result.Failure<GenerateReportResponse>(
                Error.Unauthorized("Report.TeacherScopeDenied", "Teacher can only generate reports for their classes."));
        }

        var template = await EnsureTemplateAsync(command.ReportType, currentUser.Id, now, cancellationToken);
        var scopeHash = BuildScopeHash(command, reportClass.BranchId);

        if (!string.IsNullOrWhiteSpace(command.IdempotencyKey))
        {
            var existingRun = await context.ReportRuns
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(
                    x => x.IdempotencyKey == command.IdempotencyKey &&
                         x.ScopeHash == scopeHash,
                    cancellationToken);

            if (existingRun is not null)
            {
                var existingReport = await context.StudentReports
                    .Where(r => r.ReportRunId == existingRun.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefaultAsync(cancellationToken);

                if (existingReport is not null)
                {
                    return Result.Success(new GenerateReportResponse
                    {
                        ReportRunId = existingRun.Id,
                        StudentReportId = existingReport.Id,
                        Status = existingRun.Status.ToString().ToLowerInvariant()
                    });
                }
            }
        }

        var reportRun = new ReportRun
        {
            Id = Guid.NewGuid(),
            ReportTemplateId = template.Id,
            ReportPeriodId = period.Id,
            ClassId = reportClass.Id,
            StudentId = student.Id,
            BranchId = command.BranchId ?? reportClass.BranchId,
            Status = ReportRunStatus.Pending,
            GeneratedBy = currentUser.Id,
            GeneratedAt = now,
            IdempotencyKey = command.IdempotencyKey,
            ScopeHash = scopeHash,
            CreatedAt = now,
            UpdatedAt = now
        };

        context.ReportRuns.Add(reportRun);
        await context.SaveChangesAsync(cancellationToken);

        try
        {
            reportRun.Status = ReportRunStatus.Processing;
            reportRun.UpdatedAt = VietnamTime.UtcNow();
            await context.SaveChangesAsync(cancellationToken);

            var aggregated = await AggregateSnapshotDataAsync(
                student,
                reportClass,
                period,
                cancellationToken);

            var riskRuleConfigs = await context.RiskRuleConfigs
                .ToListAsync(cancellationToken);
            var runtimeRiskRules = RiskRuleRuntimeConfig.Build(riskRuleConfigs);
            var templateRuntime = ReportTemplateRuntime.Create(template.Type, template.ContentSchema);

            var detectedRisks = DetectStudentRisks(aggregated, runtimeRiskRules, templateRuntime);
            var snapshot = BuildSnapshot(
                student,
                reportClass,
                period,
                aggregated,
                detectedRisks,
                runtimeRiskRules,
                templateRuntime);
            snapshot.Risks = detectedRisks
                .Select(r => $"{r.RiskType}: {r.Reason}")
                .ToList();
            snapshot.Recommendations = detectedRisks
                .Select(r => templateRuntime.GetRecommendation(r.RiskType))
                .Distinct()
                .ToList();

            var snapshotJson = JsonSerializer.Serialize(snapshot, ReportJson.SnapshotOptions);
            var summaryText = BuildSummaryText(snapshot);

            var studentReport = new StudentReport
            {
                Id = Guid.NewGuid(),
                StudentId = student.Id,
                ClassId = reportClass.Id,
                BranchId = reportClass.BranchId,
                ModuleId = reportClass.CurrentModuleId,
                SyllabusId = reportClass.SyllabusId,
                ReportPeriodId = period.Id,
                ReportRunId = reportRun.Id,
                ReportType = command.ReportType,
                SnapshotJson = snapshotJson,
                SummaryText = summaryText,
                Status = StudentReportStatus.Processing,
                IsParentPublished = false,
                ParentPublishedAt = null,
                ParentPublishedBy = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            context.StudentReports.Add(studentReport);

            var previousReports = await context.StudentReports
                .Where(r =>
                    r.StudentId == student.Id &&
                    r.ReportPeriodId == period.Id &&
                    r.ReportType == command.ReportType &&
                    r.Id != studentReport.Id &&
                    r.Status != StudentReportStatus.Failed)
                .ToListAsync(cancellationToken);

            foreach (var previousReport in previousReports)
            {
                previousReport.Status = StudentReportStatus.Superseded;
                previousReport.UpdatedAt = now;
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (var risk in detectedRisks)
            {
                await UpsertRiskAlertAsync(
                    student.Id,
                    reportClass.Id,
                    reportClass.BranchId,
                    period.Id,
                    risk.RiskType,
                    risk.Severity,
                    risk.Reason,
                    risk.Source,
                    now,
                    cancellationToken);

                if (risk.RiskType is RiskType.ClassCurriculumDelay or RiskType.HighReviewRatio)
                {
                    await UpsertRiskAlertAsync(
                        null,
                        reportClass.Id,
                        reportClass.BranchId,
                        period.Id,
                        risk.RiskType,
                        risk.Severity,
                        risk.Reason,
                        risk.Source,
                        now,
                        cancellationToken);
                }

                await UpsertRecommendationAsync(
                    student.Id,
                    reportClass.Id,
                    risk.RiskType,
                    risk.Severity,
                    templateRuntime.GetRecommendation(risk.RiskType),
                    now,
                    cancellationToken);
            }

            var insights = BuildInsights(snapshot, detectedRisks, templateRuntime);
            if (insights.Count > 0)
            {
                context.AIInsights.AddRange(insights.Select(insight => new AIInsight
                {
                    Id = Guid.NewGuid(),
                    StudentReportId = studentReport.Id,
                    InsightType = insight.InsightType,
                    Content = insight.Content,
                    ConfidenceScore = insight.ConfidenceScore,
                    SourceDataJson = insight.SourceDataJson,
                    CreatedAt = now
                }));
            }

            studentReport.Status = StudentReportStatus.Completed;
            studentReport.UpdatedAt = VietnamTime.UtcNow();

            reportRun.Status = ReportRunStatus.Completed;
            reportRun.UpdatedAt = studentReport.UpdatedAt;

            await context.SaveChangesAsync(cancellationToken);

            return Result.Success(new GenerateReportResponse
            {
                ReportRunId = reportRun.Id,
                StudentReportId = studentReport.Id,
                Status = "completed"
            });
        }
        catch (Exception ex)
        {
            reportRun.Status = ReportRunStatus.Failed;
            reportRun.ErrorMessage = ex.Message;
            reportRun.UpdatedAt = VietnamTime.UtcNow();
            await context.SaveChangesAsync(cancellationToken);

            return Result.Failure<GenerateReportResponse>(
                Error.Problem("Report.GenerateFailed", $"Report generation failed: {ex.Message}"));
        }
    }

    private async Task<Class?> ResolveClassAsync(Guid? classId, Guid studentId, CancellationToken cancellationToken)
    {
        if (classId.HasValue)
        {
            return await context.Classes
                .Include(c => c.Branch)
                .Include(c => c.Program)
                .Include(c => c.Level)
                .Include(c => c.CurrentModule)
                .Include(c => c.Syllabus)
                .Include(c => c.CurrentLessonPlanTemplate)
                .FirstOrDefaultAsync(c => c.Id == classId.Value, cancellationToken);
        }

        return await context.ClassEnrollments
            .Include(e => e.Class)
                .ThenInclude(c => c.Branch)
            .Include(e => e.Class)
                .ThenInclude(c => c.Program)
            .Include(e => e.Class)
                .ThenInclude(c => c.Level)
            .Include(e => e.Class)
                .ThenInclude(c => c.CurrentModule)
            .Include(e => e.Class)
                .ThenInclude(c => c.Syllabus)
            .Include(e => e.Class)
                .ThenInclude(c => c.CurrentLessonPlanTemplate)
            .Where(e => e.StudentProfileId == studentId && e.Status == EnrollmentStatus.Active)
            .OrderByDescending(e => e.EnrollDate)
            .Select(e => e.Class)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ReportTemplate> EnsureTemplateAsync(
        StudentReportType reportType,
        Guid createdBy,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var templateType = reportType switch
        {
            StudentReportType.Parent => ReportTemplateType.Parent,
            StudentReportType.Academic => ReportTemplateType.Academic,
            _ => ReportTemplateType.Internal
        };

        var existingTemplate = await context.ReportTemplates
            .FirstOrDefaultAsync(
                t => t.Type == templateType && t.IsActive,
                cancellationToken);

        if (existingTemplate is not null)
        {
            if (string.IsNullOrWhiteSpace(existingTemplate.ContentSchema))
            {
                existingTemplate.ContentSchema = ReportTemplateRuntime.CreateDefaultSchemaJson(templateType);
                await context.SaveChangesAsync(cancellationToken);
            }

            return existingTemplate;
        }

        var fallbackTemplate = new ReportTemplate
        {
            Id = Guid.NewGuid(),
            Code = $"default-{templateType.ToString().ToLowerInvariant()}",
            Name = $"Default {templateType} Template",
            Type = templateType,
            ContentSchema = ReportTemplateRuntime.CreateDefaultSchemaJson(templateType),
            IsActive = true,
            CreatedBy = createdBy,
            CreatedAt = now
        };

        context.ReportTemplates.Add(fallbackTemplate);
        await context.SaveChangesAsync(cancellationToken);
        return fallbackTemplate;
    }

    private static string BuildScopeHash(GenerateReportCommand command, Guid branchId)
    {
        var raw = string.Join(
            "|",
            command.ReportType,
            command.StudentId,
            command.ClassId ?? Guid.Empty,
            branchId,
            command.PeriodId);

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(bytes);
    }

    private async Task<AggregatedSnapshotData> AggregateSnapshotDataAsync(
        Profile student,
        Class reportClass,
        ReportPeriod period,
        CancellationToken cancellationToken)
    {
        var periodFromUtc = VietnamTime.TreatAsVietnamLocal(period.StartDate.ToDateTime(TimeOnly.MinValue));
        var periodToUtc = VietnamTime.EndOfVietnamDayUtc(
            VietnamTime.TreatAsVietnamLocal(period.EndDate.ToDateTime(TimeOnly.MinValue)));

        var studentAttendances = await context.Attendances
            .Where(a =>
                a.StudentProfileId == student.Id &&
                a.Session.ClassId == reportClass.Id &&
                a.Session.PlannedDatetime >= periodFromUtc &&
                a.Session.PlannedDatetime <= periodToUtc)
            .Select(a => new
            {
                a.AttendanceStatus,
                a.AbsenceType
            })
            .ToListAsync(cancellationToken);

        var attendanceTotal = studentAttendances.Count;
        var presentCount = studentAttendances.Count(a =>
            a.AttendanceStatus == AttendanceStatus.Present ||
            a.AttendanceStatus == AttendanceStatus.Makeup);
        var absentWithNotice = studentAttendances.Count(a =>
            a.AttendanceStatus == AttendanceStatus.Absent &&
            a.AbsenceType != AbsenceType.NoNotice);
        var absentWithoutNotice = studentAttendances.Count(a =>
            a.AttendanceStatus == AttendanceStatus.Absent &&
            a.AbsenceType == AbsenceType.NoNotice);

        var attendanceRate = attendanceTotal == 0
            ? 0m
            : Math.Round((decimal)presentCount * 100 / attendanceTotal, 2);

        var classSessionsInPeriod = await context.Sessions
            .Where(s =>
                s.ClassId == reportClass.Id &&
                s.PlannedDatetime >= periodFromUtc &&
                s.PlannedDatetime <= periodToUtc &&
                s.Status != SessionStatus.Cancelled)
            .Select(s => s.SectionType)
            .ToListAsync(cancellationToken);

        var runtimeSummary = new RuntimeSummaryData
        {
            NormalSections = classSessionsInPeriod.Count(s => s == SectionType.Normal),
            ReviewSections = classSessionsInPeriod.Count(s => s == SectionType.Review),
            MakeupSections = classSessionsInPeriod.Count(s => s == SectionType.Makeup),
            RemedialSections = classSessionsInPeriod.Count(s => s == SectionType.Remedial),
            AssessmentSections = classSessionsInPeriod.Count(s => s == SectionType.Assessment)
        };

        var ledgers = await context.LearningTicketLedgers
            .Where(l => l.StudentProfileId == student.Id && l.CreatedAt <= periodToUtc)
            .Select(l => new
            {
                l.TransactionType,
                l.Quantity
            })
            .ToListAsync(cancellationToken);

        var granted = ledgers
            .Where(l => l.TransactionType == LearningTicketTransactionType.Grant)
            .Sum(l => l.Quantity);
        var consumed = ledgers
            .Where(l => l.TransactionType == LearningTicketTransactionType.Consume)
            .Sum(l => l.Quantity);

        var remaining = ledgers.Sum(l => l.TransactionType switch
        {
            LearningTicketTransactionType.Grant => l.Quantity,
            LearningTicketTransactionType.Consume => -l.Quantity,
            LearningTicketTransactionType.Refund => l.Quantity,
            LearningTicketTransactionType.Adjustment => l.Quantity,
            LearningTicketTransactionType.Void => -l.Quantity,
            _ => 0
        });

        var studentProgress = await context.StudentProgresses
            .Where(sp =>
                sp.StudentProfileId == student.Id &&
                sp.ModuleId == reportClass.CurrentModuleId)
            .OrderByDescending(sp => sp.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var latestAssessment = await context.Assessments
            .Where(a =>
                a.StudentProfileId == student.Id &&
                a.ModuleId == reportClass.CurrentModuleId &&
                a.AssessedAt <= periodToUtc)
            .OrderByDescending(a => a.AssessedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var latestEvaluation = await context.TeacherEvaluations
            .Where(te =>
                te.StudentProfileId == student.Id &&
                te.ModuleId == reportClass.CurrentModuleId &&
                te.EvaluatedAt <= periodToUtc)
            .OrderByDescending(te => te.EvaluatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var totalSectionsOfModule = reportClass.CurrentModule?.PlannedSessionCount ?? 0;
        if (totalSectionsOfModule <= 0)
        {
            totalSectionsOfModule = await context.Sessions
                .Where(s =>
                    s.ClassId == reportClass.Id &&
                    s.ModuleId == reportClass.CurrentModuleId &&
                    s.Status != SessionStatus.Cancelled)
                .CountAsync(cancellationToken);
        }

        var expectedCompletedSections = await context.Sessions
            .Where(s =>
                s.ClassId == reportClass.Id &&
                s.ModuleId == reportClass.CurrentModuleId &&
                s.Status != SessionStatus.Cancelled &&
                s.PlannedDatetime <= periodToUtc)
            .CountAsync(cancellationToken);

        var expectedCompletionPercent = totalSectionsOfModule <= 0
            ? 0m
            : Math.Min(100m, Math.Round((decimal)expectedCompletedSections * 100 / totalSectionsOfModule, 2));

        var classCompletedSections = await context.Sessions
            .Where(s =>
                s.ClassId == reportClass.Id &&
                s.ModuleId == reportClass.CurrentModuleId &&
                s.Status == SessionStatus.Completed)
            .CountAsync(cancellationToken);

        var classActualProgressPercent = totalSectionsOfModule <= 0
            ? 0m
            : Math.Min(100m, Math.Round((decimal)classCompletedSections * 100 / totalSectionsOfModule, 2));

        return new AggregatedSnapshotData
        {
            AttendanceTotal = attendanceTotal,
            AttendancePresent = presentCount,
            AttendanceAbsentWithNotice = absentWithNotice,
            AttendanceAbsentWithoutNotice = absentWithoutNotice,
            AttendanceRate = attendanceRate,
            TicketGranted = granted,
            TicketConsumed = consumed,
            TicketRemaining = remaining,
            RuntimeSummary = runtimeSummary,
            StudentProgress = studentProgress,
            LatestAssessment = latestAssessment,
            LatestEvaluation = latestEvaluation,
            ExpectedCompletionPercent = expectedCompletionPercent,
            ClassActualProgressPercent = classActualProgressPercent,
            ClassReviewRatioPercent = runtimeSummary.TotalSections <= 0
                ? 0m
                : Math.Round((decimal)runtimeSummary.ReviewSections * 100 / runtimeSummary.TotalSections, 2)
        };
    }

    private static ReportSnapshot BuildSnapshot(
        Profile student,
        Class reportClass,
        ReportPeriod period,
        AggregatedSnapshotData data,
        IReadOnlyCollection<DetectedRisk> detectedRisks,
        IReadOnlyDictionary<RiskType, RiskRuleRuntimeConfig> runtimeRiskRules,
        ReportTemplateRuntime templateRuntime)
    {
        var riskTypes = detectedRisks
            .Select(r => r.RiskType)
            .ToHashSet();
        var packageExpiringThreshold = runtimeRiskRules[RiskType.PackageExpiring]
            .GetInt("remainingTicketsAtMost", 3);

        var snapshot = new ReportSnapshot
        {
            Student = new ReportSnapshotStudent
            {
                Id = student.Id.ToString(),
                Name = student.DisplayName,
                Branch = reportClass.Branch?.Name ?? string.Empty,
                Class = reportClass.Title
            },
            AcademicContext = new ReportSnapshotAcademicContext
            {
                Program = reportClass.Program?.Name ?? string.Empty,
                Level = reportClass.Level?.Name ?? string.Empty,
                Module = reportClass.CurrentModule?.Name ?? string.Empty,
                Syllabus = reportClass.Syllabus?.Title ?? string.Empty,
                SyllabusVersion = reportClass.Syllabus?.Version ?? string.Empty
            },
            Period = new ReportSnapshotPeriod
            {
                From = period.StartDate.ToString("yyyy-MM-dd"),
                To = period.EndDate.ToString("yyyy-MM-dd"),
                Type = period.Type.ToString().ToLowerInvariant()
            },
            AttendanceSummary = new ReportSnapshotAttendanceSummary
            {
                TotalSections = data.AttendanceTotal,
                Present = data.AttendancePresent,
                Late = 0,
                AbsentWithNotice = data.AttendanceAbsentWithNotice,
                AbsentWithoutNotice = data.AttendanceAbsentWithoutNotice,
                AttendanceRate = data.AttendanceRate
            },
            TicketSummary = new ReportSnapshotTicketSummary
            {
                Granted = data.TicketGranted,
                Consumed = data.TicketConsumed,
                Remaining = data.TicketRemaining,
                PackageExpiring = data.TicketRemaining <= packageExpiringThreshold
            },
            RuntimeSummary = new ReportSnapshotRuntimeSummary
            {
                NormalSections = data.RuntimeSummary.NormalSections,
                ReviewSections = data.RuntimeSummary.ReviewSections,
                MakeupSections = data.RuntimeSummary.MakeupSections,
                RemedialSections = data.RuntimeSummary.RemedialSections,
                AssessmentSections = data.RuntimeSummary.AssessmentSections
            },
            LearningProgress = new ReportSnapshotLearningProgress
            {
                CompletionPercent = data.StudentProgress?.CompletionPercent ?? 0,
                CurrentStatus = data.StudentProgress?.Status.ToString() ?? StudentProgressStatus.NotStarted.ToString(),
                PromotionStatus = data.StudentProgress?.PromotionStatus.ToString() ?? PromotionStatus.Pending.ToString(),
                CurrentLesson = reportClass.CurrentLessonPlanTemplate?.Title ?? string.Empty
            },
            AssessmentSummary = new ReportSnapshotAssessmentSummary
            {
                LatestScore = data.LatestAssessment?.Score,
                LatestResult = data.LatestAssessment?.Result.ToString() ?? string.Empty,
                TeacherComment = data.LatestAssessment?.TeacherComment ?? string.Empty
            },
            TeacherEvaluation = new ReportSnapshotTeacherEvaluation
            {
                Speaking = data.LatestEvaluation?.Speaking,
                Listening = data.LatestEvaluation?.Listening,
                Reading = data.LatestEvaluation?.Reading,
                Writing = data.LatestEvaluation?.Writing,
                Participation = data.LatestEvaluation?.Participation,
                Confidence = data.LatestEvaluation?.Confidence,
                Notes = data.LatestEvaluation?.Notes ?? string.Empty
            }
        };

        if (!riskTypes.Contains(RiskType.LowAttendance) &&
            snapshot.AttendanceSummary.AttendanceRate >= 90)
        {
            snapshot.Strengths.Add(templateRuntime.GetStrength("good_attendance"));
        }

        if (!riskTypes.Contains(RiskType.LearningDelay) &&
            snapshot.LearningProgress.CompletionPercent >= 80)
        {
            snapshot.Strengths.Add(templateRuntime.GetStrength("strong_progress"));
        }

        if (!riskTypes.Contains(RiskType.WeakCommunication) &&
            (snapshot.TeacherEvaluation.Speaking ?? 0) >= 4 &&
            (snapshot.TeacherEvaluation.Confidence ?? 0) >= 4)
        {
            snapshot.Strengths.Add(templateRuntime.GetStrength("confident_speaking"));
        }

        if (riskTypes.Contains(RiskType.LearningDelay))
        {
            snapshot.Weaknesses.Add(templateRuntime.GetWeakness("learning_delay"));
        }

        if (riskTypes.Contains(RiskType.AcademicFail))
        {
            snapshot.Weaknesses.Add(templateRuntime.GetWeakness("assessment_fail"));
        }

        if (riskTypes.Contains(RiskType.WeakCommunication))
        {
            snapshot.Weaknesses.Add(templateRuntime.GetWeakness("weak_communication"));
        }

        var primaryRisk = DeterminePrimaryRisk(detectedRisks, runtimeRiskRules);
        snapshot.ParentMessage = templateRuntime.GetParentMessage(
            primaryRisk?.RiskType,
            BuildParentMessageTokens(snapshot));
        snapshot.InternalNotes = templateRuntime.GetInternalNote(
            "snapshot_immutable",
            "Snapshot is immutable and generated from read-only sources.");

        return snapshot;
    }

    private static string BuildSummaryText(ReportSnapshot snapshot)
    {
        var strengths = snapshot.Strengths.Count == 0
            ? "none"
            : string.Join("; ", snapshot.Strengths);

        var weaknesses = snapshot.Weaknesses.Count == 0
            ? "none"
            : string.Join("; ", snapshot.Weaknesses);

        return $"Attendance {snapshot.AttendanceSummary.AttendanceRate:F2}%, completion {snapshot.LearningProgress.CompletionPercent:F2}%. Strengths: {strengths}. Weaknesses: {weaknesses}.";
    }

    private static DetectedRisk? DeterminePrimaryRisk(
        IReadOnlyCollection<DetectedRisk> detectedRisks,
        IReadOnlyDictionary<RiskType, RiskRuleRuntimeConfig> runtimeRiskRules)
    {
        return detectedRisks
            .OrderByDescending(r => r.Severity)
            .ThenByDescending(r => runtimeRiskRules[r.RiskType].Score)
            .ThenBy(r => r.RiskType)
            .FirstOrDefault();
    }

    private static Dictionary<string, string> BuildParentMessageTokens(ReportSnapshot snapshot)
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["remainingTickets"] = snapshot.TicketSummary.Remaining.ToString(CultureInfo.InvariantCulture),
            ["attendanceRate"] = FormatDecimal(snapshot.AttendanceSummary.AttendanceRate),
            ["completionPercent"] = FormatDecimal(snapshot.LearningProgress.CompletionPercent)
        };
    }

    private static List<DetectedRisk> DetectStudentRisks(
        AggregatedSnapshotData data,
        IReadOnlyDictionary<RiskType, RiskRuleRuntimeConfig> runtimeRiskRules,
        ReportTemplateRuntime templateRuntime)
    {
        var risks = new List<DetectedRisk>();
        var hasAssessmentFail = data.LatestAssessment?.Result == AssessmentResult.Fail;
        var lowAttendanceRule = runtimeRiskRules[RiskType.LowAttendance];
        var forceHighAttendanceThreshold = lowAttendanceRule.GetDecimal("forceHighAttendanceBelow", 50m);

        var lowAttendanceThreshold = lowAttendanceRule.GetDecimal("attendanceRateBelow", 70m);
        if (lowAttendanceRule.IsActive && data.AttendanceRate < lowAttendanceThreshold)
        {
            risks.Add(CreateRisk(
                RiskType.LowAttendance,
                "attendance",
                lowAttendanceRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["attendanceRate"] = FormatDecimal(data.AttendanceRate),
                    ["attendanceRateBelow"] = FormatDecimal(lowAttendanceThreshold)
                }));
        }

        var attendanceDisciplineRule = runtimeRiskRules[RiskType.AttendanceDiscipline];
        var absentWithoutNoticeThreshold = attendanceDisciplineRule.GetInt("absentWithoutNoticeAtLeast", 2);
        if (attendanceDisciplineRule.IsActive &&
            data.AttendanceAbsentWithoutNotice >= absentWithoutNoticeThreshold)
        {
            risks.Add(CreateRisk(
                RiskType.AttendanceDiscipline,
                "attendance",
                attendanceDisciplineRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["absentWithoutNotice"] = data.AttendanceAbsentWithoutNotice.ToString(CultureInfo.InvariantCulture),
                    ["absentWithoutNoticeAtLeast"] = absentWithoutNoticeThreshold.ToString(CultureInfo.InvariantCulture)
                }));
        }

        var learningDelayRule = runtimeRiskRules[RiskType.LearningDelay];
        var delayBufferPercent = learningDelayRule.GetDecimal("delayBufferPercent", 10m);
        var actualCompletion = data.StudentProgress?.CompletionPercent ?? 0;
        if (learningDelayRule.IsActive &&
            actualCompletion < data.ExpectedCompletionPercent - delayBufferPercent)
        {
            risks.Add(CreateRisk(
                RiskType.LearningDelay,
                "progress",
                learningDelayRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["completionPercent"] = FormatDecimal(actualCompletion),
                    ["expectedCompletionPercent"] = FormatDecimal(data.ExpectedCompletionPercent),
                    ["delayBufferPercent"] = FormatDecimal(delayBufferPercent)
                }));
        }

        var academicFailRule = runtimeRiskRules[RiskType.AcademicFail];
        if (academicFailRule.IsActive && hasAssessmentFail)
        {
            risks.Add(CreateRisk(
                RiskType.AcademicFail,
                "assessment",
                academicFailRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime));
        }

        var weakCommunicationRule = runtimeRiskRules[RiskType.WeakCommunication];
        var speakingThreshold = weakCommunicationRule.GetInt("speakingAtMost", 2);
        var confidenceThreshold = weakCommunicationRule.GetInt("confidenceAtMost", 2);
        var speaking = data.LatestEvaluation?.Speaking ?? 5;
        var confidence = data.LatestEvaluation?.Confidence ?? 5;
        if (weakCommunicationRule.IsActive &&
            (speaking <= speakingThreshold || confidence <= confidenceThreshold))
        {
            risks.Add(CreateRisk(
                RiskType.WeakCommunication,
                "teacher_evaluation",
                weakCommunicationRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["speaking"] = speaking.ToString(CultureInfo.InvariantCulture),
                    ["confidence"] = confidence.ToString(CultureInfo.InvariantCulture),
                    ["speakingAtMost"] = speakingThreshold.ToString(CultureInfo.InvariantCulture),
                    ["confidenceAtMost"] = confidenceThreshold.ToString(CultureInfo.InvariantCulture)
                }));
        }

        var packageExpiringRule = runtimeRiskRules[RiskType.PackageExpiring];
        var remainingTicketsThreshold = packageExpiringRule.GetInt("remainingTicketsAtMost", 3);
        if (packageExpiringRule.IsActive && data.TicketRemaining <= remainingTicketsThreshold)
        {
            risks.Add(CreateRisk(
                RiskType.PackageExpiring,
                "ticket",
                packageExpiringRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["remainingTickets"] = data.TicketRemaining.ToString(CultureInfo.InvariantCulture),
                    ["remainingTicketsAtMost"] = remainingTicketsThreshold.ToString(CultureInfo.InvariantCulture)
                }));
        }

        var classCurriculumDelayRule = runtimeRiskRules[RiskType.ClassCurriculumDelay];
        var classProgressTolerance = classCurriculumDelayRule.GetDecimal("progressLagTolerancePercent", 0m);
        if (classCurriculumDelayRule.IsActive &&
            data.ClassActualProgressPercent + classProgressTolerance < data.ExpectedCompletionPercent)
        {
            risks.Add(CreateRisk(
                RiskType.ClassCurriculumDelay,
                "class_progress",
                classCurriculumDelayRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["classActualProgressPercent"] = FormatDecimal(data.ClassActualProgressPercent),
                    ["expectedCompletionPercent"] = FormatDecimal(data.ExpectedCompletionPercent),
                    ["progressLagTolerancePercent"] = FormatDecimal(classProgressTolerance)
                }));
        }

        var highReviewRatioRule = runtimeRiskRules[RiskType.HighReviewRatio];
        var reviewRatioThreshold = highReviewRatioRule.GetDecimal("reviewRatioAtLeast", 40m);
        if (highReviewRatioRule.IsActive && data.ClassReviewRatioPercent >= reviewRatioThreshold)
        {
            risks.Add(CreateRisk(
                RiskType.HighReviewRatio,
                "runtime",
                highReviewRatioRule,
                data,
                hasAssessmentFail,
                forceHighAttendanceThreshold,
                templateRuntime,
                new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["classReviewRatioPercent"] = FormatDecimal(data.ClassReviewRatioPercent),
                    ["reviewRatioAtLeast"] = FormatDecimal(reviewRatioThreshold)
                }));
        }

        return risks;
    }

    private static DetectedRisk CreateRisk(
        RiskType riskType,
        string source,
        RiskRuleRuntimeConfig rule,
        AggregatedSnapshotData data,
        bool hasAssessmentFail,
        decimal forceHighAttendanceThreshold,
        ReportTemplateRuntime templateRuntime,
        IReadOnlyDictionary<string, string>? reasonTokens = null)
    {
        var severity = RiskRuleDefaults.ToSeverity(
            rule.Score,
            data.AttendanceRate,
            hasAssessmentFail,
            forceHighAttendanceThreshold);

        return new DetectedRisk
        {
            RiskType = riskType,
            Severity = severity,
            Reason = templateRuntime.GetRiskReason(riskType, reasonTokens),
            Source = source
        };
    }

    private static string FormatDecimal(decimal value)
    {
        return value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private async Task UpsertRiskAlertAsync(
        Guid? studentId,
        Guid classId,
        Guid branchId,
        Guid periodId,
        RiskType riskType,
        RiskSeverity newSeverity,
        string reason,
        string source,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var existingOpenAlert = await context.RiskAlerts
            .Where(x =>
                x.StudentId == studentId &&
                x.ClassId == classId &&
                x.BranchId == branchId &&
                x.RiskType == riskType &&
                x.ReportPeriodId == periodId &&
                x.Status == RiskAlertStatus.Open)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingOpenAlert is null)
        {
            context.RiskAlerts.Add(new RiskAlert
            {
                Id = Guid.NewGuid(),
                StudentId = studentId,
                ClassId = classId,
                BranchId = branchId,
                ReportPeriodId = periodId,
                RiskType = riskType,
                Severity = newSeverity,
                Reason = reason,
                Source = source,
                Status = RiskAlertStatus.Open,
                CreatedAt = now
            });

            return;
        }

        if (newSeverity > existingOpenAlert.Severity)
        {
            existingOpenAlert.Severity = newSeverity;
            existingOpenAlert.Reason = reason;
            existingOpenAlert.Source = source;
        }
    }

    private async Task UpsertRecommendationAsync(
        Guid studentId,
        Guid classId,
        RiskType riskType,
        RiskSeverity severity,
        string content,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var assignedRole = RiskRuleDefaults.GetAssignedRole(riskType);

        var existingPending = await context.Recommendations
            .FirstOrDefaultAsync(
                x => x.StudentId == studentId &&
                     x.ClassId == classId &&
                     x.RecommendationType == riskType &&
                     x.AssignedRole == assignedRole &&
                     x.Status == RecommendationStatus.Pending,
                cancellationToken);

        if (existingPending is not null)
        {
            return;
        }

        context.Recommendations.Add(new Recommendation
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            ClassId = classId,
            RecommendationType = riskType,
            Content = content,
            Priority = RiskRuleDefaults.ToPriority(severity),
            AssignedRole = assignedRole,
            Status = RecommendationStatus.Pending,
            DueAt = RiskRuleDefaults.CalculateDueAt(now, severity),
            CreatedAt = now
        });
    }

    private static List<InsightSeed> BuildInsights(
        ReportSnapshot snapshot,
        IReadOnlyCollection<DetectedRisk> risks,
        ReportTemplateRuntime templateRuntime)
    {
        var insights = new List<InsightSeed>();

        foreach (var strength in snapshot.Strengths)
        {
            insights.Add(new InsightSeed
            {
                InsightType = AIInsightType.Strength,
                Content = strength,
                ConfidenceScore = 0.8m
            });
        }

        foreach (var weakness in snapshot.Weaknesses)
        {
            insights.Add(new InsightSeed
            {
                InsightType = AIInsightType.Weakness,
                Content = weakness,
                ConfidenceScore = 0.8m
            });
        }

        foreach (var risk in risks)
        {
            insights.Add(new InsightSeed
            {
                InsightType = AIInsightType.Risk,
                Content = $"{risk.RiskType}: {risk.Reason}",
                ConfidenceScore = 0.9m,
                SourceDataJson = JsonSerializer.Serialize(
                    new
                    {
                        risk_type = risk.RiskType.ToString(),
                        severity = risk.Severity.ToString()
                    },
                    ReportJson.SnapshotOptions)
            });
        }

        insights.Add(new InsightSeed
        {
            InsightType = AIInsightType.Note,
            Content = templateRuntime.GetInternalNote(
                "insight_generated",
                "Rule-based insight generation executed successfully."),
            ConfidenceScore = 1m
        });

        return insights;
    }

    private sealed class AggregatedSnapshotData
    {
        public int AttendanceTotal { get; init; }
        public int AttendancePresent { get; init; }
        public int AttendanceAbsentWithNotice { get; init; }
        public int AttendanceAbsentWithoutNotice { get; init; }
        public decimal AttendanceRate { get; init; }
        public int TicketGranted { get; init; }
        public int TicketConsumed { get; init; }
        public int TicketRemaining { get; init; }
        public RuntimeSummaryData RuntimeSummary { get; init; } = new();
        public StudentProgress? StudentProgress { get; init; }
        public Assessment? LatestAssessment { get; init; }
        public TeacherEvaluation? LatestEvaluation { get; init; }
        public decimal ExpectedCompletionPercent { get; init; }
        public decimal ClassActualProgressPercent { get; init; }
        public decimal ClassReviewRatioPercent { get; init; }
    }

    private sealed class RuntimeSummaryData
    {
        public int NormalSections { get; init; }
        public int ReviewSections { get; init; }
        public int MakeupSections { get; init; }
        public int RemedialSections { get; init; }
        public int AssessmentSections { get; init; }
        public int TotalSections =>
            NormalSections + ReviewSections + MakeupSections + RemedialSections + AssessmentSections;
    }

    private sealed class DetectedRisk
    {
        public RiskType RiskType { get; init; }
        public RiskSeverity Severity { get; init; }
        public string Reason { get; init; } = string.Empty;
        public string Source { get; init; } = string.Empty;
    }

    private sealed class InsightSeed
    {
        public AIInsightType InsightType { get; init; }
        public string Content { get; init; } = string.Empty;
        public decimal? ConfidenceScore { get; init; }
        public string? SourceDataJson { get; init; }
    }
}
