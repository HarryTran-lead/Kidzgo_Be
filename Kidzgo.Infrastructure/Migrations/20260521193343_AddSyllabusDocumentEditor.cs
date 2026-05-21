using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSyllabusDocumentEditor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchiveReason",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentStatus",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<int>(
                name: "DocumentVersion",
                schema: "public",
                table: "Syllabuses",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "ParserVersion",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SectionsJson",
                schema: "public",
                table: "Syllabuses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                schema: "public",
                table: "Syllabuses",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Manual");

            migrationBuilder.AddColumn<string>(
                name: "WarningsJson",
                schema: "public",
                table: "Syllabuses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchiveReason",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "DocumentStatus",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "DocumentVersion",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "ParserVersion",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "SectionsJson",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "public",
                table: "Syllabuses");

            migrationBuilder.DropColumn(
                name: "WarningsJson",
                schema: "public",
                table: "Syllabuses");
        }
    }
}
