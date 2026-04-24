using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSchedulePatternMaxLength : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SchedulePattern",
                schema: "public",
                table: "ClassScheduleSegments",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "SessionSelectionPattern",
                schema: "public",
                table: "ClassEnrollmentScheduleSegments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionSelectionPattern",
                schema: "public",
                table: "ClassEnrollments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            // Xóa ClassScheduleSegments của các classes thuộc non-supplementary programs.
            // Các segments này bị tạo sai bởi seed script cũ.
            // Chỉ supplementary classes mới có segments theo business logic.
            migrationBuilder.Sql(@"
DELETE FROM public.""ClassScheduleSegments""
WHERE ""ClassId"" IN (
    SELECT c.""Id""
    FROM public.""Classes"" c
    INNER JOIN public.""Programs"" p ON c.""ProgramId"" = p.""Id""
    WHERE p.""IsSupplementary"" = FALSE
);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SchedulePattern",
                schema: "public",
                table: "ClassScheduleSegments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "SessionSelectionPattern",
                schema: "public",
                table: "ClassEnrollmentScheduleSegments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SessionSelectionPattern",
                schema: "public",
                table: "ClassEnrollments",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
