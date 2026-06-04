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
                deduped AS (
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
                family_max AS (
                    SELECT
                        "ProgramId",
                        "LevelId",
                        "Code",
                        COALESCE(MAX(CASE
                            WHEN parsed_version > 0 AND parsed_rank = 1 THEN parsed_version
                            ELSE NULL
                        END), 0) AS max_version
                    FROM deduped
                    GROUP BY "ProgramId", "LevelId", "Code"
                ),
                reassigned AS (
                    SELECT
                        d."Id",
                        f.max_version + ROW_NUMBER() OVER (
                            PARTITION BY d."ProgramId", d."LevelId", d."Code"
                            ORDER BY d."CreatedAt", d."Id") AS new_version
                    FROM deduped d
                    INNER JOIN family_max f
                        ON f."ProgramId" = d."ProgramId"
                       AND f."LevelId" = d."LevelId"
                       AND f."Code" = d."Code"
                    WHERE NOT (d.parsed_version > 0 AND d.parsed_rank = 1)
                ),
                resolved AS (
                    SELECT
                        d."Id",
                        CASE
                            WHEN d.parsed_version > 0 AND d.parsed_rank = 1 THEN d.parsed_version
                            ELSE r.new_version
                        END AS new_version
                    FROM deduped d
                    LEFT JOIN reassigned r ON r."Id" = d."Id"
                )
                UPDATE public."Syllabuses" s
                SET "VersionInt" = resolved.new_version
                FROM resolved
                WHERE s."Id" = resolved."Id";
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
