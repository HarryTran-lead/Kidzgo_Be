using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Recommendations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendationType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    AssignedRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recommendations_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recommendations_Profiles_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportPeriods",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportPeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReportTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContentSchema = table.Column<string>(type: "jsonb", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportTemplates_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskRuleConfigs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskRuleConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskRuleConfigs_Users_UpdatedBy",
                        column: x => x.UpdatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RiskAlerts",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    RiskType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Severity = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiskAlerts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskAlerts_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskAlerts_Profiles_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RiskAlerts_ReportPeriods_ReportPeriodId",
                        column: x => x.ReportPeriodId,
                        principalSchema: "public",
                        principalTable: "ReportPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportRuns",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GeneratedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErrorMessage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ScopeHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportRuns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRuns_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRuns_Profiles_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRuns_ReportPeriods_ReportPeriodId",
                        column: x => x.ReportPeriodId,
                        principalSchema: "public",
                        principalTable: "ReportPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRuns_ReportTemplates_ReportTemplateId",
                        column: x => x.ReportTemplateId,
                        principalSchema: "public",
                        principalTable: "ReportTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportRuns_Users_GeneratedBy",
                        column: x => x.GeneratedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentReports",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportRunId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SnapshotJson = table.Column<string>(type: "jsonb", nullable: false),
                    SummaryText = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentReports_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentReports_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentReports_Profiles_StudentId",
                        column: x => x.StudentId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentReports_ReportPeriods_ReportPeriodId",
                        column: x => x.ReportPeriodId,
                        principalSchema: "public",
                        principalTable: "ReportPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentReports_ReportRuns_ReportRunId",
                        column: x => x.ReportRunId,
                        principalSchema: "public",
                        principalTable: "ReportRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AIInsights",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsightType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    SourceDataJson = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AIInsights_StudentReports_StudentReportId",
                        column: x => x.StudentReportId,
                        principalSchema: "public",
                        principalTable: "StudentReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportShareLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RecipientContact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportShareLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportShareLogs_StudentReports_StudentReportId",
                        column: x => x.StudentReportId,
                        principalSchema: "public",
                        principalTable: "StudentReports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIInsights_StudentReportId",
                schema: "public",
                table: "AIInsights",
                column: "StudentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_AssignedRole_Status_DueAt",
                schema: "public",
                table: "Recommendations",
                columns: new[] { "AssignedRole", "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_ClassId",
                schema: "public",
                table: "Recommendations",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Recommendations_StudentId_Status_DueAt",
                schema: "public",
                table: "Recommendations",
                columns: new[] { "StudentId", "Status", "DueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ReportPeriods_Code",
                schema: "public",
                table: "ReportPeriods",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_BranchId",
                schema: "public",
                table: "ReportRuns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_ClassId",
                schema: "public",
                table: "ReportRuns",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_GeneratedBy",
                schema: "public",
                table: "ReportRuns",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_IdempotencyKey_ScopeHash",
                schema: "public",
                table: "ReportRuns",
                columns: new[] { "IdempotencyKey", "ScopeHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_ReportPeriodId",
                schema: "public",
                table: "ReportRuns",
                column: "ReportPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_ReportTemplateId",
                schema: "public",
                table: "ReportRuns",
                column: "ReportTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportRuns_StudentId",
                schema: "public",
                table: "ReportRuns",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportShareLogs_ProviderMessageId",
                schema: "public",
                table: "ReportShareLogs",
                column: "ProviderMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportShareLogs_StudentReportId",
                schema: "public",
                table: "ReportShareLogs",
                column: "StudentReportId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_Code",
                schema: "public",
                table: "ReportTemplates",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportTemplates_CreatedBy",
                schema: "public",
                table: "ReportTemplates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAlerts_BranchId",
                schema: "public",
                table: "RiskAlerts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAlerts_ClassId",
                schema: "public",
                table: "RiskAlerts",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_RiskAlerts_ReportPeriodId",
                schema: "public",
                table: "RiskAlerts",
                column: "ReportPeriodId");

            migrationBuilder.CreateIndex(
                name: "risk_alert_dedup_idx",
                schema: "public",
                table: "RiskAlerts",
                columns: new[] { "StudentId", "ClassId", "BranchId", "RiskType", "ReportPeriodId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskRuleConfigs_RiskType",
                schema: "public",
                table: "RiskRuleConfigs",
                column: "RiskType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RiskRuleConfigs_UpdatedBy",
                schema: "public",
                table: "RiskRuleConfigs",
                column: "UpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_BranchId",
                schema: "public",
                table: "StudentReports",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_ClassId",
                schema: "public",
                table: "StudentReports",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_ReportPeriodId",
                schema: "public",
                table: "StudentReports",
                column: "ReportPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_ReportRunId",
                schema: "public",
                table: "StudentReports",
                column: "ReportRunId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_StudentId_ReportPeriodId_ReportType_CreatedAt",
                schema: "public",
                table: "StudentReports",
                columns: new[] { "StudentId", "ReportPeriodId", "ReportType", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIInsights",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Recommendations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportShareLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskAlerts",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RiskRuleConfigs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentReports",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportRuns",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportPeriods",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ReportTemplates",
                schema: "public");
        }
    }
}
