using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Syllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Levels_LevelId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Modules_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Programs_ProgramId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_Levels_LevelId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_ProgramId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_LevelId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_ProgramId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "Level",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "LevelId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "ProgramId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.AlterColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LessonPlanTemplateId",
                schema: "public",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                schema: "public",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SessionIndexInModule",
                schema: "public",
                table: "Sessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Evaluation",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Grammar",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LanguageContent",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Objectives",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Procedure",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StudentMaterials",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherMaterials",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeachingMethodology",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Vocabulary",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentModuleId",
                schema: "public",
                table: "Classes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "Classes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StartModuleId",
                schema: "public",
                table: "Classes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ClassModuleProgresses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    RequiredSessions = table.Column<int>(type: "integer", nullable: false),
                    CompletedSessions = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassModuleProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassModuleProgresses_Classes_ClassId",
                        column: x => x.ClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassModuleProgresses_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Syllabuses",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Edition = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Overview = table.Column<string>(type: "text", nullable: true),
                    OverallObjectives = table.Column<string>(type: "text", nullable: true),
                    SpecificObjectives = table.Column<string>(type: "text", nullable: true),
                    EthicsAndAttitudes = table.Column<string>(type: "text", nullable: true),
                    BookOverview = table.Column<string>(type: "text", nullable: true),
                    TotalPeriods = table.Column<int>(type: "integer", nullable: true),
                    MinutesPerPeriod = table.Column<int>(type: "integer", nullable: true),
                    TotalLessons = table.Column<int>(type: "integer", nullable: true),
                    SourceFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AttachmentUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RawContentJson = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Syllabuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Syllabuses_Levels_LevelId",
                        column: x => x.LevelId,
                        principalSchema: "public",
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Syllabuses_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Syllabuses_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SyllabusLessons",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodFrom = table.Column<int>(type: "integer", nullable: true),
                    PeriodTo = table.Column<int>(type: "integer", nullable: true),
                    Topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LessonNumber = table.Column<int>(type: "integer", nullable: true),
                    ContentSummary = table.Column<string>(type: "text", nullable: true),
                    StructureSummary = table.Column<string>(type: "text", nullable: true),
                    StudentBookPages = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TeacherBookPages = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusLessons_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyllabusLessons_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalSchema: "public",
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyllabusResources",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Abbreviation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntendedUsers = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusResources_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalSchema: "public",
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SyllabusUnits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AllocatedPeriods = table.Column<int>(type: "integer", nullable: true),
                    LessonCount = table.Column<int>(type: "integer", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusUnits_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SyllabusUnits_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalSchema: "public",
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ModuleId",
                schema: "public",
                table: "TuitionPlans",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ProgramId_LevelId_ModuleId_TotalSessions_Name",
                schema: "public",
                table: "TuitionPlans",
                columns: new[] { "ProgramId", "LevelId", "ModuleId", "TotalSessions", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_LessonPlanTemplateId",
                schema: "public",
                table: "Sessions",
                column: "LessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_ModuleId",
                schema: "public",
                table: "Sessions",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_ModuleId_SessionIndex",
                schema: "public",
                table: "LessonPlanTemplates",
                columns: new[] { "ModuleId", "SessionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CurrentModuleId",
                schema: "public",
                table: "Classes",
                column: "CurrentModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_LevelId",
                schema: "public",
                table: "Classes",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_StartModuleId",
                schema: "public",
                table: "Classes",
                column: "StartModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassModuleProgresses_ClassId_ModuleId",
                schema: "public",
                table: "ClassModuleProgresses",
                columns: new[] { "ClassId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassModuleProgresses_ModuleId",
                schema: "public",
                table: "ClassModuleProgresses",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_CreatedBy",
                schema: "public",
                table: "Syllabuses",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_LevelId",
                schema: "public",
                table: "Syllabuses",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Title",
                schema: "public",
                table: "Syllabuses",
                columns: new[] { "ProgramId", "LevelId", "Title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusLessons_ModuleId",
                schema: "public",
                table: "SyllabusLessons",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusLessons_SyllabusId_OrderIndex",
                schema: "public",
                table: "SyllabusLessons",
                columns: new[] { "SyllabusId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusResources_SyllabusId_OrderIndex",
                schema: "public",
                table: "SyllabusResources",
                columns: new[] { "SyllabusId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusUnits_ModuleId",
                schema: "public",
                table: "SyllabusUnits",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusUnits_SyllabusId_OrderIndex",
                schema: "public",
                table: "SyllabusUnits",
                columns: new[] { "SyllabusId", "OrderIndex" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Levels_LevelId",
                schema: "public",
                table: "Classes",
                column: "LevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Modules_CurrentModuleId",
                schema: "public",
                table: "Classes",
                column: "CurrentModuleId",
                principalSchema: "public",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Modules_StartModuleId",
                schema: "public",
                table: "Classes",
                column: "StartModuleId",
                principalSchema: "public",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_Modules_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ModuleId",
                principalSchema: "public",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_LessonPlanTemplates_LessonPlanTemplateId",
                schema: "public",
                table: "Sessions",
                column: "LessonPlanTemplateId",
                principalSchema: "public",
                principalTable: "LessonPlanTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_Modules_ModuleId",
                schema: "public",
                table: "Sessions",
                column: "ModuleId",
                principalSchema: "public",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionPlans_Levels_LevelId",
                schema: "public",
                table: "TuitionPlans",
                column: "LevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionPlans_Modules_ModuleId",
                schema: "public",
                table: "TuitionPlans",
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
                name: "FK_Classes_Levels_LevelId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Modules_CurrentModuleId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Modules_StartModuleId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Modules_ModuleId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_LessonPlanTemplates_LessonPlanTemplateId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_Modules_ModuleId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_Levels_LevelId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_Modules_ModuleId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropTable(
                name: "ClassModuleProgresses",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SyllabusLessons",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SyllabusResources",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SyllabusUnits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Syllabuses",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_ModuleId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_ProgramId_LevelId_ModuleId_TotalSessions_Name",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_LessonPlanTemplateId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_ModuleId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_ModuleId_SessionIndex",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Classes_CurrentModuleId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_LevelId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_StartModuleId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropColumn(
                name: "LessonPlanTemplateId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "SessionIndexInModule",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "Evaluation",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "Grammar",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "LanguageContent",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "Objectives",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "Procedure",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "StudentMaterials",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "TeacherMaterials",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "TeachingMethodology",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "Vocabulary",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "CurrentModuleId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "LevelId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "StartModuleId",
                schema: "public",
                table: "Classes");

            migrationBuilder.AlterColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ModuleId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Level",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProgramId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ProgramId",
                schema: "public",
                table: "TuitionPlans",
                column: "ProgramId");

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
                name: "IX_LessonPlanTemplates_ProgramId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ProgramId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_Programs_ProgramId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "ProgramId",
                principalSchema: "public",
                principalTable: "Programs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionPlans_Levels_LevelId",
                schema: "public",
                table: "TuitionPlans",
                column: "LevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
