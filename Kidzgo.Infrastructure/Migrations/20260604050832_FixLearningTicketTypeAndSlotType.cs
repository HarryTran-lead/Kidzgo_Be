using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixLearningTicketTypeAndSlotType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DayGroup",
                schema: "public",
                table: "SlotTypes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TeacherType",
                schema: "public",
                table: "SlotTypes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeBand",
                schema: "public",
                table: "SlotTypes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UsageType",
                schema: "public",
                table: "SlotTypes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "AllowedDayGroups",
                schema: "public",
                table: "LearningTicketTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AllowedTeacherTypes",
                schema: "public",
                table: "LearningTicketTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AllowedTimeBands",
                schema: "public",
                table: "LearningTicketTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AllowedUsageTypes",
                schema: "public",
                table: "LearningTicketTypes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompatibilityMode",
                schema: "public",
                table: "LearningTicketTypes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DayGroup",
                schema: "public",
                table: "SlotTypes");

            migrationBuilder.DropColumn(
                name: "TeacherType",
                schema: "public",
                table: "SlotTypes");

            migrationBuilder.DropColumn(
                name: "TimeBand",
                schema: "public",
                table: "SlotTypes");

            migrationBuilder.DropColumn(
                name: "UsageType",
                schema: "public",
                table: "SlotTypes");

            migrationBuilder.DropColumn(
                name: "AllowedDayGroups",
                schema: "public",
                table: "LearningTicketTypes");

            migrationBuilder.DropColumn(
                name: "AllowedTeacherTypes",
                schema: "public",
                table: "LearningTicketTypes");

            migrationBuilder.DropColumn(
                name: "AllowedTimeBands",
                schema: "public",
                table: "LearningTicketTypes");

            migrationBuilder.DropColumn(
                name: "AllowedUsageTypes",
                schema: "public",
                table: "LearningTicketTypes");

            migrationBuilder.DropColumn(
                name: "CompatibilityMode",
                schema: "public",
                table: "LearningTicketTypes");
        }
    }
}
