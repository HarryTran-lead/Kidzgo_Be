using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStarterUnitConceptFromCurriculumImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE public."CurriculumImportModuleRules"
                SET
                    "UnitFrom" = CASE
                        WHEN "IncludeStarterUnit" THEN 0
                        WHEN COALESCE("UnitFrom", "UnitTo") IS NULL THEN NULL
                        ELSE GREATEST(LEAST(COALESCE("UnitFrom", "UnitTo"), COALESCE("UnitTo", "UnitFrom")), 0)
                    END,
                    "UnitTo" = CASE
                        WHEN "IncludeStarterUnit" THEN GREATEST(COALESCE("UnitFrom", "UnitTo", 0), COALESCE("UnitTo", "UnitFrom", 0), 0)
                        WHEN COALESCE("UnitFrom", "UnitTo") IS NULL THEN NULL
                        ELSE GREATEST(COALESCE("UnitFrom", "UnitTo"), COALESCE("UnitTo", "UnitFrom"), 0)
                    END
                WHERE
                    "IncludeStarterUnit" = TRUE OR
                    "UnitFrom" IS NOT NULL OR
                    "UnitTo" IS NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."Modules" AS m
                SET "PlannedSessionCount" = calc."ExpectedLessonPlanCount"
                FROM (
                    SELECT
                        r."ModuleId",
                        (
                            CASE
                                WHEN r."UnitFrom" IS NOT NULL AND r."UnitTo" IS NOT NULL AND r."UnitFrom" <= r."UnitTo"
                                    THEN ((r."UnitTo" - r."UnitFrom" + 1) * c."RegularUnitLessonPlanCount")
                                ELSE 0
                            END +
                            CASE
                                WHEN r."RevisionNumber" IS NOT NULL
                                    THEN c."RevisionLessonPlanCount"
                                ELSE 0
                            END
                        ) AS "ExpectedLessonPlanCount"
                    FROM public."CurriculumImportModuleRules" AS r
                    INNER JOIN public."CurriculumImportConfigurations" AS c
                        ON c."Id" = r."CurriculumImportConfigurationId"
                ) AS calc
                WHERE m."Id" = calc."ModuleId";
                """);

            migrationBuilder.DropColumn(
                name: "IncludeStarterUnit",
                schema: "public",
                table: "CurriculumImportModuleRules");

            migrationBuilder.DropColumn(
                name: "StarterUnitLessonPlanCount",
                schema: "public",
                table: "CurriculumImportConfigurations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IncludeStarterUnit",
                schema: "public",
                table: "CurriculumImportModuleRules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StarterUnitLessonPlanCount",
                schema: "public",
                table: "CurriculumImportConfigurations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE public."CurriculumImportModuleRules"
                SET "IncludeStarterUnit" = TRUE
                WHERE "UnitFrom" = 0;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."CurriculumImportModuleRules"
                SET
                    "UnitFrom" = CASE
                        WHEN "UnitTo" IS NOT NULL AND "UnitTo" > 0 THEN 1
                        ELSE NULL
                    END,
                    "UnitTo" = CASE
                        WHEN "UnitTo" IS NOT NULL AND "UnitTo" > 0 THEN "UnitTo"
                        ELSE NULL
                    END
                WHERE "IncludeStarterUnit" = TRUE;
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."CurriculumImportConfigurations" AS c
                SET "StarterUnitLessonPlanCount" = c."RegularUnitLessonPlanCount"
                WHERE EXISTS (
                    SELECT 1
                    FROM public."CurriculumImportModuleRules" AS r
                    WHERE
                        r."CurriculumImportConfigurationId" = c."Id" AND
                        r."IncludeStarterUnit" = TRUE
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE public."Modules" AS m
                SET "PlannedSessionCount" = calc."ExpectedLessonPlanCount"
                FROM (
                    SELECT
                        r."ModuleId",
                        (
                            CASE
                                WHEN r."IncludeStarterUnit"
                                    THEN c."StarterUnitLessonPlanCount"
                                ELSE 0
                            END +
                            CASE
                                WHEN r."UnitFrom" IS NOT NULL AND r."UnitTo" IS NOT NULL AND r."UnitFrom" <= r."UnitTo"
                                    THEN ((r."UnitTo" - r."UnitFrom" + 1) * c."RegularUnitLessonPlanCount")
                                ELSE 0
                            END +
                            CASE
                                WHEN r."RevisionNumber" IS NOT NULL
                                    THEN c."RevisionLessonPlanCount"
                                ELSE 0
                            END
                        ) AS "ExpectedLessonPlanCount"
                    FROM public."CurriculumImportModuleRules" AS r
                    INNER JOIN public."CurriculumImportConfigurations" AS c
                        ON c."Id" = r."CurriculumImportConfigurationId"
                ) AS calc
                WHERE m."Id" = calc."ModuleId";
                """);
        }
    }
}
