using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProgressRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PracticeTestScoreMappingsJson",
                schema: "public",
                table: "ProgramProgressionRules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ListeningPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReadingPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpeakingPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WritingPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PracticeTestScoreMappingsJson",
                schema: "public",
                table: "ProgramProgressionRules");

            migrationBuilder.DropColumn(
                name: "ListeningPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropColumn(
                name: "ReadingPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropColumn(
                name: "SpeakingPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments");

            migrationBuilder.DropColumn(
                name: "WritingPracticeScore",
                schema: "public",
                table: "ProgramProgressionAssessments");
        }
    }
}
