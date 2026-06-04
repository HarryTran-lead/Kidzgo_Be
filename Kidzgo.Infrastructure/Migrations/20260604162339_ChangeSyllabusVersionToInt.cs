using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSyllabusVersionToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Code_Version",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.AddColumn<int>(
                name: "VersionInt",
                schema: "public",
                table: "Syllabuses",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql(
                """
                WITH parsed AS (
                    SELECT
                        "Id",
                        "ProgramId",
                        "LevelId",
                        "Code",
                        "CreatedAt",
                        CASE
                            WHEN "Version" ~ '^\s*\d+\s*$'
                                THEN regexp_replace("Version", '\s+', '', 'g')::integer
                            WHEN "Version" ~* '^\s*v\s*\d+\s*$'
                                THEN regexp_replace("Version", '\D', '', 'g')::integer
                            ELSE NULL
                        END AS parsed_version
                    FROM public."Syllabuses"
                ),
                ranked AS (
                    SELECT
                        p.*,
                        CASE
                            WHEN p.parsed_version > 0 THEN ROW_NUMBER() OVER (
                                PARTITION BY p."ProgramId", p."LevelId", p."Code", p.parsed_version
                                ORDER BY p."CreatedAt", p."Id")
                            ELSE NULL
                        END AS parsed_rank
                    FROM parsed p
                ),
                resolved AS (
                    SELECT
                        r."Id",
                        CASE
                            WHEN r.parsed_version > 0 AND r.parsed_rank = 1 THEN r.parsed_version
                            ELSE
                                COALESCE(MAX(CASE
                                    WHEN r.parsed_version > 0 AND r.parsed_rank = 1
                                        THEN r.parsed_version
                                    ELSE NULL
                                END) OVER (
                                    PARTITION BY r."ProgramId", r."LevelId", r."Code"
                                ), 0)
                                +
                                SUM(CASE
                                    WHEN NOT (r.parsed_version > 0 AND r.parsed_rank = 1)
                                        THEN 1
                                    ELSE 0
                                END) OVER (
                                    PARTITION BY r."ProgramId", r."LevelId", r."Code"
                                    ORDER BY r."CreatedAt", r."Id"
                                    ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                                )
                        END AS new_version
                    FROM ranked r
                )
                UPDATE public."Syllabuses" s
                SET "VersionInt" = resolved.new_version
                FROM resolved
                WHERE s."Id" = resolved."Id";
                """);

            migrationBuilder.Sql(
                """
                WITH unresolved AS (
                    SELECT
                        s."Id",
                        COALESCE(MAX(existing."VersionInt"), 0) AS family_max,
                        ROW_NUMBER() OVER (
                            PARTITION BY s."ProgramId", s."LevelId", s."Code"
                            ORDER BY s."CreatedAt", s."Id") AS fallback_rank
                    FROM public."Syllabuses" s
                    LEFT JOIN public."Syllabuses" existing
                        ON existing."ProgramId" IS NOT DISTINCT FROM s."ProgramId"
                       AND existing."LevelId" IS NOT DISTINCT FROM s."LevelId"
                       AND existing."Code" IS NOT DISTINCT FROM s."Code"
                       AND existing."VersionInt" IS NOT NULL
                    WHERE s."VersionInt" IS NULL
                    GROUP BY s."Id", s."ProgramId", s."LevelId", s."Code", s."CreatedAt"
                )
                UPDATE public."Syllabuses" s
                SET "VersionInt" = unresolved.family_max + unresolved.fallback_rank
                FROM unresolved
                WHERE s."Id" = unresolved."Id";
                """);

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.RenameColumn(
                name: "VersionInt",
                schema: "public",
                table: "Syllabuses",
                newName: "Version");

            migrationBuilder.AlterColumn<int>(
                name: "Version",
                schema: "public",
                table: "Syllabuses",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Code_Version",
                schema: "public",
                table: "Syllabuses",
                columns: new[] { "ProgramId", "LevelId", "Code", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Code_Version",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.AddColumn<string>(
                name: "VersionText",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(
                """
                UPDATE public."Syllabuses"
                SET "VersionText" = "Version"::text;
                """);

            migrationBuilder.DropColumn(
                name: "Version",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.RenameColumn(
                name: "VersionText",
                schema: "public",
                table: "Syllabuses",
                newName: "Version");

            migrationBuilder.AlterColumn<string>(
                name: "Version",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Syllabuses_ProgramId_LevelId_Code_Version",
                schema: "public",
                table: "Syllabuses",
                columns: new[] { "ProgramId", "LevelId", "Code", "Version" },
                unique: true);
        }
    }
}
