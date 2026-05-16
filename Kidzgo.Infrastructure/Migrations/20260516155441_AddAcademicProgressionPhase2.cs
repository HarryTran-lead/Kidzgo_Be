using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicProgressionPhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionOrder",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CarryForwardContent",
                schema: "public",
                table: "LessonPlans",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletionPercent",
                schema: "public",
                table: "LessonPlans",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Levels",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Levels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Levels_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    PlannedSessionCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modules_Levels_LevelId",
                        column: x => x.LevelId,
                        principalSchema: "public",
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    TeacherComment = table.Column<string>(type: "text", nullable: true),
                    AssessedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assessments_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assessments_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Assessments_Users_AssessedBy",
                        column: x => x.AssessedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PromotionDecisions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionDecisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionDecisions_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionDecisions_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionDecisions_Users_ApprovedBy",
                        column: x => x.ApprovedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RemedialPlans",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeakSkills = table.Column<string>(type: "text", nullable: false),
                    RecommendedSessionCount = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RemedialPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RemedialPlans_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemedialPlans_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RemedialPlans_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherEvaluations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Speaking = table.Column<int>(type: "integer", nullable: false),
                    Listening = table.Column<int>(type: "integer", nullable: false),
                    Reading = table.Column<int>(type: "integer", nullable: false),
                    Writing = table.Column<int>(type: "integer", nullable: false),
                    Participation = table.Column<int>(type: "integer", nullable: false),
                    Confidence = table.Column<int>(type: "integer", nullable: false),
                    Behavior = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    EvaluatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluations_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluations_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeacherEvaluations_Users_EvaluatedBy",
                        column: x => x.EvaluatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentProgresses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletionPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    AssessmentStatus = table.Column<int>(type: "integer", nullable: false),
                    PromotionStatus = table.Column<int>(type: "integer", nullable: false),
                    LastAssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentLessonPlanTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProgresses_Assessments_LastAssessmentId",
                        column: x => x.LastAssessmentId,
                        principalSchema: "public",
                        principalTable: "Assessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentProgresses_LessonPlanTemplates_CurrentLessonPlanTemp~",
                        column: x => x.CurrentLessonPlanTemplateId,
                        principalSchema: "public",
                        principalTable: "LessonPlanTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StudentProgresses_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentProgresses_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_LevelId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_AssessedBy",
                schema: "public",
                table: "Assessments",
                column: "AssessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_ModuleId",
                schema: "public",
                table: "Assessments",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_SessionId",
                schema: "public",
                table: "Assessments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_StudentProfileId_ModuleId_AssessedAt",
                schema: "public",
                table: "Assessments",
                columns: new[] { "StudentProfileId", "ModuleId", "AssessedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Levels_ProgramId_Code",
                schema: "public",
                table: "Levels",
                columns: new[] { "ProgramId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Levels_ProgramId_Order",
                schema: "public",
                table: "Levels",
                columns: new[] { "ProgramId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_LevelId_Code",
                schema: "public",
                table: "Modules",
                columns: new[] { "LevelId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_LevelId_Order",
                schema: "public",
                table: "Modules",
                columns: new[] { "LevelId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromotionDecisions_ApprovedBy",
                schema: "public",
                table: "PromotionDecisions",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionDecisions_ModuleId",
                schema: "public",
                table: "PromotionDecisions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionDecisions_StudentProfileId_ModuleId_ApprovedAt",
                schema: "public",
                table: "PromotionDecisions",
                columns: new[] { "StudentProfileId", "ModuleId", "ApprovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RemedialPlans_CreatedBy",
                schema: "public",
                table: "RemedialPlans",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RemedialPlans_ModuleId",
                schema: "public",
                table: "RemedialPlans",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_RemedialPlans_StudentProfileId_ModuleId_CreatedAt",
                schema: "public",
                table: "RemedialPlans",
                columns: new[] { "StudentProfileId", "ModuleId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_CurrentLessonPlanTemplateId",
                schema: "public",
                table: "StudentProgresses",
                column: "CurrentLessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_LastAssessmentId",
                schema: "public",
                table: "StudentProgresses",
                column: "LastAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_ModuleId",
                schema: "public",
                table: "StudentProgresses",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgresses_StudentProfileId_ModuleId",
                schema: "public",
                table: "StudentProgresses",
                columns: new[] { "StudentProfileId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluations_EvaluatedBy",
                schema: "public",
                table: "TeacherEvaluations",
                column: "EvaluatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluations_ModuleId",
                schema: "public",
                table: "TeacherEvaluations",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherEvaluations_StudentProfileId_ModuleId_EvaluatedAt",
                schema: "public",
                table: "TeacherEvaluations",
                columns: new[] { "StudentProfileId", "ModuleId", "EvaluatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_Levels_LevelId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "LevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_Modules_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ModuleId",
                principalSchema: "public",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Levels_LevelId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Modules_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropTable(
                name: "PromotionDecisions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RemedialPlans",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentProgresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TeacherEvaluations",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Assessments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Modules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Levels",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_LevelId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "LevelId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "SessionOrder",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "CarryForwardContent",
                schema: "public",
                table: "LessonPlans");

            migrationBuilder.DropColumn(
                name: "CompletionPercent",
                schema: "public",
                table: "LessonPlans");
        }
    }
}
