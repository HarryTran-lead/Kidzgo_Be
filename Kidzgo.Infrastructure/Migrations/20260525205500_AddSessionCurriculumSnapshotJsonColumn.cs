using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    public partial class AddSessionCurriculumSnapshotJsonColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE public."Sessions"
                ADD COLUMN IF NOT EXISTS "CurriculumSnapshotJson" text NULL;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE public."Sessions"
                DROP COLUMN IF EXISTS "CurriculumSnapshotJson";
                """);
        }
    }
}
