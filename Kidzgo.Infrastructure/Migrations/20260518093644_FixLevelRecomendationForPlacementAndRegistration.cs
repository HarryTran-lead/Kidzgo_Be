using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLevelRecomendationForPlacementAndRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProgramProgressionRules_SourceProgramId_IsActive",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryLevelId",
                schema: "public",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                WITH candidate_programs AS (
                    SELECT DISTINCT x."ProgramId"
                    FROM (
                        SELECT r."ProgramId" AS "ProgramId"
                        FROM public."Registrations" r

                        UNION ALL

                        SELECT pr."SourceProgramId" AS "ProgramId"
                        FROM public."ProgramProgressionRules" pr

                        UNION ALL

                        SELECT pr."TargetProgramId" AS "ProgramId"
                        FROM public."ProgramProgressionRules" pr
                        WHERE pr."TargetProgramId" IS NOT NULL

                        UNION ALL

                        SELECT pa."SourceProgramId" AS "ProgramId"
                        FROM public."ProgramProgressionAssessments" pa

                        UNION ALL

                        SELECT pa."TargetProgramId" AS "ProgramId"
                        FROM public."ProgramProgressionAssessments" pa
                        WHERE pa."TargetProgramId" IS NOT NULL
                    ) x
                    WHERE x."ProgramId" IS NOT NULL
                ),
                missing_programs AS (
                    SELECT cp."ProgramId"
                    FROM candidate_programs cp
                    INNER JOIN public."Programs" p ON p."Id" = cp."ProgramId"
                    LEFT JOIN public."Levels" lv ON lv."ProgramId" = cp."ProgramId"
                    WHERE lv."Id" IS NULL
                )
                INSERT INTO public."Levels"
                (
                    "Id",
                    "ProgramId",
                    "Code",
                    "Name",
                    "Order",
                    "Description",
                    "IsActive",
                    "CreatedAt",
                    "UpdatedAt"
                )
                SELECT
                    (
                        substr(md5('AUTO_LEVEL:' || mp."ProgramId"::text), 1, 8) || '-' ||
                        substr(md5('AUTO_LEVEL:' || mp."ProgramId"::text), 9, 4) || '-' ||
                        substr(md5('AUTO_LEVEL:' || mp."ProgramId"::text), 13, 4) || '-' ||
                        substr(md5('AUTO_LEVEL:' || mp."ProgramId"::text), 17, 4) || '-' ||
                        substr(md5('AUTO_LEVEL:' || mp."ProgramId"::text), 21, 12)
                    )::uuid AS "Id",
                    mp."ProgramId",
                    'AUTO-L1',
                    'Auto Level',
                    1,
                    'Auto generated by migration 20260518093644',
                    TRUE,
                    NOW(),
                    NOW()
                FROM missing_programs mp;
                """);

            migrationBuilder.Sql("""
                UPDATE public."Registrations" r
                SET "LevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = r."ProgramId" AND lv."IsActive" = TRUE
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE r."LevelId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."Registrations" r
                SET "LevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = r."ProgramId"
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE r."LevelId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionRules" r
                SET "SourceLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = r."SourceProgramId" AND lv."IsActive" = TRUE
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE r."SourceLevelId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionRules" r
                SET "SourceLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = r."SourceProgramId"
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE r."SourceLevelId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionRules" r
                SET "TargetLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = r."TargetProgramId" AND lv."IsActive" = TRUE
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE r."TargetLevelId" IS NULL AND r."TargetProgramId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionRules" r
                SET "TargetLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = r."TargetProgramId"
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE r."TargetLevelId" IS NULL AND r."TargetProgramId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionAssessments" a
                SET "SourceLevelId" = r."LevelId"
                FROM public."Registrations" r
                WHERE a."SourceLevelId" IS NULL
                  AND a."SourceRegistrationId" = r."Id"
                  AND r."LevelId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionAssessments" a
                SET "SourceLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = a."SourceProgramId" AND lv."IsActive" = TRUE
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE a."SourceLevelId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionAssessments" a
                SET "SourceLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = a."SourceProgramId"
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE a."SourceLevelId" IS NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionAssessments" a
                SET "TargetLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = a."TargetProgramId" AND lv."IsActive" = TRUE
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE a."TargetLevelId" IS NULL AND a."TargetProgramId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                UPDATE public."ProgramProgressionAssessments" a
                SET "TargetLevelId" = (
                    SELECT lv."Id"
                    FROM public."Levels" lv
                    WHERE lv."ProgramId" = a."TargetProgramId"
                    ORDER BY lv."Order", lv."CreatedAt", lv."Id"
                    LIMIT 1
                )
                WHERE a."TargetLevelId" IS NULL AND a."TargetProgramId" IS NOT NULL;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM public."Registrations"
                        WHERE "LevelId" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Cannot backfill Registrations.LevelId. Ensure every Program has at least one Level before migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM public."ProgramProgressionRules"
                        WHERE "SourceLevelId" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Cannot backfill ProgramProgressionRules.SourceLevelId. Ensure every source program has at least one Level before migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM public."ProgramProgressionAssessments"
                        WHERE "SourceLevelId" IS NULL
                    ) THEN
                        RAISE EXCEPTION 'Cannot backfill ProgramProgressionAssessments.SourceLevelId. Ensure source programs/registrations have valid Levels before migration.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "Registrations",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_LevelId",
                schema: "public",
                table: "TuitionPlans",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_LevelId",
                schema: "public",
                table: "Registrations",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_SecondaryLevelId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                column: "SourceLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_SourceLevelId_IsActive",
                schema: "public",
                table: "ProgramProgressionRules",
                columns: new[] { "SourceLevelId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                column: "TargetLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "SourceLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionAssessments_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "TargetLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_PrimaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests",
                column: "PrimaryLevelRecommendationId");

            migrationBuilder.CreateIndex(
                name: "IX_PlacementTests_SecondaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests",
                column: "SecondaryLevelRecommendationId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTests_Levels_PrimaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests",
                column: "PrimaryLevelRecommendationId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PlacementTests_Levels_SecondaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests",
                column: "SecondaryLevelRecommendationId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramProgressionAssessments_Levels_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "SourceLevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramProgressionAssessments_Levels_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments",
                column: "TargetLevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramProgressionRules_Levels_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                column: "SourceLevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProgramProgressionRules_Levels_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionRules",
                column: "TargetLevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Levels_LevelId",
                schema: "public",
                table: "Registrations",
                column: "LevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_Levels_SecondaryLevelId",
                schema: "public",
                table: "Registrations",
                column: "SecondaryLevelId",
                principalSchema: "public",
                principalTable: "Levels",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTests_Levels_PrimaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropForeignKey(
                name: "FK_PlacementTests_Levels_SecondaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramProgressionAssessments_Levels_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramProgressionAssessments_Levels_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramProgressionRules_Levels_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropForeignKey(
                name: "FK_ProgramProgressionRules_Levels_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Levels_LevelId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_Levels_SecondaryLevelId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_Levels_LevelId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_LevelId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_LevelId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_SecondaryLevelId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropIndex(
                name: "IX_ProgramProgressionRules_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropIndex(
                name: "IX_ProgramProgressionRules_SourceLevelId_IsActive",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropIndex(
                name: "IX_ProgramProgressionRules_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropIndex(
                name: "IX_ProgramProgressionAssessments_SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropIndex(
                name: "IX_ProgramProgressionAssessments_TargetLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTests_PrimaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropIndex(
                name: "IX_PlacementTests_SecondaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "LevelId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropColumn(
                name: "LevelId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "SecondaryLevelId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "SourceLevelId",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropColumn(
                name: "TargetLevelId",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropColumn(
                name: "SourceLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropColumn(
                name: "TargetLevelId",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropColumn(
                name: "PrimaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.DropColumn(
                name: "SecondaryLevelRecommendationId",
                schema: "public",
                table: "PlacementTests");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramProgressionRules_SourceProgramId_IsActive",
                schema: "public",
                table: "ProgramProgressionRules",
                columns: new[] { "SourceProgramId", "IsActive" });
        }
    }
}
