using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingLessonPlanAndSessionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public."LessonPlanTemplates"
                ADD COLUMN IF NOT EXISTS "SyllabusId" uuid NULL;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE public."Sessions"
                ADD COLUMN IF NOT EXISTS "CurriculumSnapshotJson" text NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public."LessonPlanTemplates"
                DROP COLUMN IF EXISTS "SyllabusId";
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE public."Sessions"
                DROP COLUMN IF EXISTS "CurriculumSnapshotJson";
                """);
        }
    }
}
