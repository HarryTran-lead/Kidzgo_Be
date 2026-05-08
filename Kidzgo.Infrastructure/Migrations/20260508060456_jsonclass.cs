using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class jsonclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SchedulePattern",
                schema: "public",
                table: "Classes",
                newName: "WeeklyScheduleJson");

            migrationBuilder.AlterColumn<string>(
                name: "WeeklyScheduleJson",
                schema: "public",
                table: "Classes",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeeklyScheduleJson",
                schema: "public",
                table: "Classes",
                newName: "SchedulePattern");

            migrationBuilder.AlterColumn<string>(
                name: "SchedulePattern",
                schema: "public",
                table: "Classes",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
