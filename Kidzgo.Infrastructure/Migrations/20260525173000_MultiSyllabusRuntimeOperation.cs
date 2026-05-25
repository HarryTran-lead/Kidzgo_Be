using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    public partial class MultiSyllabusRuntimeOperation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurriculumSnapshotJson",
                schema: "public",
                table: "Sessions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SyllabusId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE public."LessonPlanTemplates" AS lpt
                SET "SyllabusId" = st."SyllabusId"
                FROM public."SessionTemplates" AS st
                WHERE lpt."SessionTemplateId" = st."Id"
                  AND lpt."SyllabusId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."LessonPlanTemplates" AS lpt
                SET "SyllabusId" = chosen."Id"
                FROM public."Modules" AS m
                INNER JOIN public."Levels" AS lvl ON lvl."Id" = m."LevelId"
                INNER JOIN LATERAL (
                    SELECT s."Id"
                    FROM public."Syllabuses" AS s
                    WHERE s."ProgramId" = lvl."ProgramId"
                      AND s."LevelId" = m."LevelId"
                      AND s."IsDeleted" = FALSE
                    ORDER BY s."IsActive" DESC, s."UpdatedAt" DESC, s."CreatedAt" DESC
                    LIMIT 1
                ) AS chosen ON TRUE
                WHERE lpt."ModuleId" = m."Id"
                  AND lpt."SyllabusId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "SyllabusId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CurriculumAssignments",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumAssignments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurriculumAssignments_Levels_LevelId",
                        column: x => x.LevelId,
                        principalSchema: "public",
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurriculumAssignments_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurriculumAssignments_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalSchema: "public",
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO public."CurriculumAssignments"
                    ("Id", "BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom", "EffectiveTo", "IsActive", "CreatedAt", "UpdatedAt")
                SELECT (
                           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 1, 8) || '-' ||
                           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 9, 4) || '-' ||
                           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 13, 4) || '-' ||
                           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 17, 4) || '-' ||
                           substring(md5(bp."BranchId"::text || ':' || s."Id"::text), 21, 12)
                       )::uuid,
                       bp."BranchId",
                       s."ProgramId",
                       s."LevelId",
                       s."Id",
                       s."EffectiveFrom",
                       s."EffectiveTo",
                       TRUE,
                       NOW(),
                       NOW()
                FROM public."BranchPrograms" AS bp
                INNER JOIN public."Syllabuses" AS s
                    ON s."ProgramId" = bp."ProgramId"
                WHERE bp."IsActive" = TRUE
                  AND s."IsDeleted" = FALSE
                  AND s."IsActive" = TRUE
                ON CONFLICT ("BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom") DO NOTHING;
                """);

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_ModuleId_SessionIndex",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumAssignments_BranchId_ProgramId_LevelId_IsActive",
                schema: "public",
                table: "CurriculumAssignments",
                columns: new[] { "BranchId", "ProgramId", "LevelId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumAssignments_BranchId_ProgramId_LevelId_SyllabusId_EffectiveFrom",
                schema: "public",
                table: "CurriculumAssignments",
                columns: new[] { "BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumAssignments_LevelId",
                schema: "public",
                table: "CurriculumAssignments",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumAssignments_ProgramId",
                schema: "public",
                table: "CurriculumAssignments",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumAssignments_SyllabusId",
                schema: "public",
                table: "CurriculumAssignments",
                column: "SyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_SyllabusId_ModuleId_SessionIndex",
                schema: "public",
                table: "LessonPlanTemplates",
                columns: new[] { "SyllabusId", "ModuleId", "SessionIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_SyllabusId_ModuleId_SessionOrder",
                schema: "public",
                table: "LessonPlanTemplates",
                columns: new[] { "SyllabusId", "ModuleId", "SessionOrder" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_Syllabuses_SyllabusId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "SyllabusId",
                principalSchema: "public",
                principalTable: "Syllabuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_Syllabuses_SyllabusId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropTable(
                name: "CurriculumAssignments",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_SyllabusId_ModuleId_SessionIndex",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_SyllabusId_ModuleId_SessionOrder",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_ModuleId_SessionIndex",
                schema: "public",
                table: "LessonPlanTemplates",
                columns: new[] { "ModuleId", "SessionIndex" },
                unique: true);

            migrationBuilder.DropColumn(
                name: "SyllabusId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "CurriculumSnapshotJson",
                schema: "public",
                table: "Sessions");
        }
    }
}
