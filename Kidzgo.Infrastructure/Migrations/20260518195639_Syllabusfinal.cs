using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Syllabusfinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Title",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFrom",
                schema: "public",
                table: "Syllabuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveTo",
                schema: "public",
                table: "Syllabuses",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PacingSchemeJson",
                schema: "public",
                table: "Syllabuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Version",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "RescheduledFromSessionId",
                schema: "public",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                schema: "public",
                table: "Modules",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Core");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionTemplateId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SyllabusId",
                schema: "public",
                table: "Classes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HomeworkTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: true),
                    MaterialReference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeworkTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HomeworkTemplates_LessonPlanTemplates_LessonPlanTemplateId",
                        column: x => x.LessonPlanTemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlanTemplateActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TeacherActivity = table.Column<string>(type: "text", nullable: true),
                    StudentActivity = table.Column<string>(type: "text", nullable: true),
                    Resources = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlanTemplateActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlanTemplateActivities_LessonPlanTemplates_LessonPlan~",
                        column: x => x.LessonPlanTemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonPlanTemplateMaterials",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    MaterialType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlanTemplateMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlanTemplateMaterials_LessonPlanTemplates_LessonPlanT~",
                        column: x => x.LessonPlanTemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessionTemplates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    LessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionIndex = table.Column<int>(type: "integer", nullable: false),
                    SessionIndexInModule = table.Column<int>(type: "integer", nullable: true),
                    LessonNumber = table.Column<int>(type: "integer", nullable: true),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ObjectiveSummary = table.Column<string>(type: "text", nullable: true),
                    VocabularySummary = table.Column<string>(type: "text", nullable: true),
                    GrammarSummary = table.Column<string>(type: "text", nullable: true),
                    ContentSummary = table.Column<string>(type: "text", nullable: true),
                    TeacherNotes = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_Levels_LevelId",
                        column: x => x.LevelId,
                        principalSchema: "public",
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionTemplates_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalSchema: "public",
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LessonPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    GeneralNote = table.Column<string>(type: "text", nullable: true),
                    HomeworkAssigned = table.Column<string>(type: "text", nullable: true),
                    CarryForwardContent = table.Column<string>(type: "text", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingLogs_LessonPlans_LessonPlanId",
                        column: x => x.LessonPlanId,
                        principalSchema: "public",
                        principalTable: "LessonPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeachingLogs_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingLogs_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassSessionLessons",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    LessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoveragePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ProgressStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSessionLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSessionLessons_LessonPlanTemplates_LessonPlanTemplateId",
                        column: x => x.LessonPlanTemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClassSessionLessons_SessionTemplates_SessionTemplateId",
                        column: x => x.SessionTemplateId,
                        principalSchema: "public",
                        principalTable: "SessionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClassSessionLessons_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingActivityLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlannedActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActualActivityText = table.Column<string>(type: "text", nullable: true),
                    ActualDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    WasCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingActivityLogs_LessonPlanTemplateActivities_PlannedAc~",
                        column: x => x.PlannedActivityId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplateActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeachingActivityLogs_TeachingLogs_TeachingLogId",
                        column: x => x.TeachingLogId,
                        principalSchema: "public",
                        principalTable: "TeachingLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingLogLessons",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeachingLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    LessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoveragePercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ProgressStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingLogLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeachingLogLessons_LessonPlanTemplates_LessonPlanTemplateId",
                        column: x => x.LessonPlanTemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeachingLogLessons_SessionTemplates_SessionTemplateId",
                        column: x => x.SessionTemplateId,
                        principalSchema: "public",
                        principalTable: "SessionTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TeachingLogLessons_TeachingLogs_TeachingLogId",
                        column: x => x.TeachingLogId,
                        principalSchema: "public",
                        principalTable: "TeachingLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Code_Version",
                schema: "public",
                table: "Syllabuses",
                columns: new[] { "ProgramId", "LevelId", "Code", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_RescheduledFromSessionId",
                schema: "public",
                table: "Sessions",
                column: "RescheduledFromSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_SessionTemplateId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "SessionTemplateId",
                unique: true,
                filter: "\"SessionTemplateId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SyllabusId",
                schema: "public",
                table: "Classes",
                column: "SyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionLessons_LessonPlanTemplateId",
                schema: "public",
                table: "ClassSessionLessons",
                column: "LessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionLessons_SessionId",
                schema: "public",
                table: "ClassSessionLessons",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSessionLessons_SessionTemplateId",
                schema: "public",
                table: "ClassSessionLessons",
                column: "SessionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_HomeworkTemplates_LessonPlanTemplateId",
                schema: "public",
                table: "HomeworkTemplates",
                column: "LessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplateActivities_LessonPlanTemplateId",
                schema: "public",
                table: "LessonPlanTemplateActivities",
                column: "LessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplateMaterials_LessonPlanTemplateId",
                schema: "public",
                table: "LessonPlanTemplateMaterials",
                column: "LessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_LevelId",
                schema: "public",
                table: "SessionTemplates",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_ModuleId_SessionIndexInModule",
                schema: "public",
                table: "SessionTemplates",
                columns: new[] { "ModuleId", "SessionIndexInModule" });

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_ProgramId",
                schema: "public",
                table: "SessionTemplates",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTemplates_SyllabusId_SessionIndex",
                schema: "public",
                table: "SessionTemplates",
                columns: new[] { "SyllabusId", "SessionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingActivityLogs_PlannedActivityId",
                schema: "public",
                table: "TeachingActivityLogs",
                column: "PlannedActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingActivityLogs_TeachingLogId",
                schema: "public",
                table: "TeachingActivityLogs",
                column: "TeachingLogId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogLessons_LessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogLessons",
                column: "LessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogLessons_SessionTemplateId",
                schema: "public",
                table: "TeachingLogLessons",
                column: "SessionTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogLessons_TeachingLogId",
                schema: "public",
                table: "TeachingLogLessons",
                column: "TeachingLogId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogs_LessonPlanId",
                schema: "public",
                table: "TeachingLogs",
                column: "LessonPlanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogs_SessionId",
                schema: "public",
                table: "TeachingLogs",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogs_SubmittedBy",
                schema: "public",
                table: "TeachingLogs",
                column: "SubmittedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Syllabuses_SyllabusId",
                schema: "public",
                table: "Classes",
                column: "SyllabusId",
                principalSchema: "public",
                principalTable: "Syllabuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_SessionTemplates_SessionTemplateId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "SessionTemplateId",
                principalSchema: "public",
                principalTable: "SessionTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Sessions_RescheduledFromSessionId",
                schema: "public",
                table: "Sessions",
                column: "RescheduledFromSessionId",
                principalSchema: "public",
                principalTable: "Sessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Syllabuses_SyllabusId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_SessionTemplates_SessionTemplateId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Sessions_RescheduledFromSessionId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropTable(
                name: "ClassSessionLessons",
                schema: "public");

            migrationBuilder.DropTable(
                name: "HomeworkTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LessonPlanTemplateMaterials",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingActivityLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingLogLessons",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LessonPlanTemplateActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SessionTemplates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeachingLogs",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Code_Version",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_RescheduledFromSessionId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_SessionTemplateId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Classes_SyllabusId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "EffectiveFrom",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "EffectiveTo",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "PacingSchemeJson",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "RescheduledFromSessionId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Type",
                schema: "public",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "SessionTemplateId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "SyllabusId",
                schema: "public",
                table: "Classes");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Title",
                schema: "public",
                table: "Syllabuses",
                columns: new[] { "ProgramId", "LevelId", "Title" },
                unique: true);
        }
    }
}
