using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260528013000_AddReportV3TemplateAndRuleConfigContent")]
    public partial class AddReportV3TemplateAndRuleConfigContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParametersJson",
                schema: "public",
                table: "RiskRuleConfigs",
                type: "jsonb",
                nullable: false,
                defaultValueSql: "'{}'::jsonb");

            migrationBuilder.Sql(
                """
                UPDATE public."RiskRuleConfigs"
                SET "ParametersJson" = CASE "RiskType"
                    WHEN 'LowAttendance' THEN '{"attendanceRateBelow":70,"forceHighAttendanceBelow":50}'::jsonb
                    WHEN 'AttendanceDiscipline' THEN '{"absentWithoutNoticeAtLeast":2}'::jsonb
                    WHEN 'LearningDelay' THEN '{"delayBufferPercent":10}'::jsonb
                    WHEN 'AcademicFail' THEN '{}'::jsonb
                    WHEN 'WeakCommunication' THEN '{"speakingAtMost":2,"confidenceAtMost":2}'::jsonb
                    WHEN 'PackageExpiring' THEN '{"remainingTicketsAtMost":3}'::jsonb
                    WHEN 'ClassCurriculumDelay' THEN '{"progressLagTolerancePercent":0}'::jsonb
                    WHEN 'HighReviewRatio' THEN '{"reviewRatioAtLeast":40}'::jsonb
                    ELSE '{}'::jsonb
                END
                WHERE "ParametersJson" IS NULL
                   OR "ParametersJson"::text = '""'
                   OR btrim("ParametersJson"::text) = '';
                """);

            migrationBuilder.Sql(
                """
                INSERT INTO public."RiskRuleConfigs" ("Id", "RiskType", "Score", "IsActive", "ParametersJson", "UpdatedBy", "UpdatedAt")
                VALUES
                    ('00000000-0000-0000-0000-000000000301', 'AcademicFail', 100, TRUE, '{}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000302', 'LowAttendance', 90, TRUE, '{"attendanceRateBelow":70,"forceHighAttendanceBelow":50}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000303', 'AttendanceDiscipline', 80, TRUE, '{"absentWithoutNoticeAtLeast":2}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000304', 'LearningDelay', 75, TRUE, '{"delayBufferPercent":10}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000305', 'WeakCommunication', 70, TRUE, '{"speakingAtMost":2,"confidenceAtMost":2}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000306', 'PackageExpiring', 60, TRUE, '{"remainingTicketsAtMost":3}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000307', 'ClassCurriculumDelay', 55, TRUE, '{"progressLagTolerancePercent":0}'::jsonb, NULL, now()),
                    ('00000000-0000-0000-0000-000000000308', 'HighReviewRatio', 50, TRUE, '{"reviewRatioAtLeast":40}'::jsonb, NULL, now())
                ON CONFLICT ("RiskType") DO NOTHING;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."ReportTemplates"
                SET "ContentSchema" = '{
                    "parent_messages":{
                        "AcademicFail":"Student needs more time to strengthen speaking confidence before the next learning milestone.",
                        "LowAttendance":"Please support attendance consistency so learning progress can improve steadily.",
                        "PackageExpiring":"Remaining sessions are low ({remainingTickets}). Please review package renewal options.",
                        "default":"Student is maintaining stable learning momentum and can continue with the next module goals."
                    },
                    "risk_reasons":{
                        "LowAttendance":"Attendance rate ({attendanceRate}%) is below {attendanceRateBelow}%.",
                        "AttendanceDiscipline":"Absence without notice occurred {absentWithoutNotice} times (threshold: {absentWithoutNoticeAtLeast}).",
                        "LearningDelay":"Completion ({completionPercent}%) is below expected ({expectedCompletionPercent}%) with {delayBufferPercent}% buffer.",
                        "AcademicFail":"Latest assessment result is FAIL.",
                        "WeakCommunication":"Speaking ({speaking}) or confidence ({confidence}) is at or below configured threshold.",
                        "PackageExpiring":"Remaining learning tickets ({remainingTickets}) are at or below {remainingTicketsAtMost}.",
                        "ClassCurriculumDelay":"Class progress ({classActualProgressPercent}%) is behind expected progress ({expectedCompletionPercent}%).",
                        "HighReviewRatio":"Review section ratio ({classReviewRatioPercent}%) is at least {reviewRatioAtLeast}%."
                    },
                    "recommendations":{
                        "LowAttendance":"Contact parent to verify attendance schedule and absence reasons.",
                        "AttendanceDiscipline":"Confirm attendance policy with parent and student.",
                        "LearningDelay":"Add focused review support for delayed lessons.",
                        "AcademicFail":"Create remedial recommendation before reassessment.",
                        "WeakCommunication":"Increase speaking-focused activities in class.",
                        "PackageExpiring":"Advise parent on package renewal options.",
                        "ClassCurriculumDelay":"Review class pacing and teaching plan.",
                        "HighReviewRatio":"Review teaching plan to balance review and new content.",
                        "default":"Follow up with student and parent for corrective action."
                    },
                    "strengths":{
                        "good_attendance":"Good attendance consistency.",
                        "strong_progress":"Strong learning progress in current module.",
                        "confident_speaking":"Confident speaking participation."
                    },
                    "weaknesses":{
                        "learning_delay":"Learning progress is behind expected module pacing.",
                        "assessment_fail":"Latest assessment result requires remediation.",
                        "weak_communication":"Communication confidence needs additional support."
                    },
                    "internal_notes":{
                        "snapshot_immutable":"Snapshot is immutable and generated from read-only sources.",
                        "insight_generated":"Rule-based insight generation executed successfully."
                    }
                }'::jsonb
                WHERE "ContentSchema" IS NULL
                   OR "ContentSchema"::text = '""'
                   OR btrim("ContentSchema"::text) = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParametersJson",
                schema: "public",
                table: "RiskRuleConfigs");
        }
    }
}
