using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260528113000_ReconcileCurriculumAssignmentsTable")]
    public partial class ReconcileCurriculumAssignmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CREATE TABLE IF NOT EXISTS public."CurriculumAssignments" (
                    "Id" uuid NOT NULL,
                    "BranchId" uuid NOT NULL,
                    "ProgramId" uuid NOT NULL,
                    "LevelId" uuid NOT NULL,
                    "SyllabusId" uuid NOT NULL,
                    "EffectiveFrom" timestamp with time zone NULL,
                    "EffectiveTo" timestamp with time zone NULL,
                    "IsActive" boolean NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NOT NULL,
                    CONSTRAINT "PK_CurriculumAssignments" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_CurriculumAssignments_Branches_BranchId" FOREIGN KEY ("BranchId") REFERENCES public."Branches" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_CurriculumAssignments_Levels_LevelId" FOREIGN KEY ("LevelId") REFERENCES public."Levels" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_CurriculumAssignments_Programs_ProgramId" FOREIGN KEY ("ProgramId") REFERENCES public."Programs" ("Id") ON DELETE RESTRICT,
                    CONSTRAINT "FK_CurriculumAssignments_Syllabuses_SyllabusId" FOREIGN KEY ("SyllabusId") REFERENCES public."Syllabuses" ("Id") ON DELETE CASCADE
                );
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_LevelId"
                ON public."CurriculumAssignments" ("LevelId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_ProgramId"
                ON public."CurriculumAssignments" ("ProgramId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_SyllabusId"
                ON public."CurriculumAssignments" ("SyllabusId");
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_CurriculumAssignments_BranchId_ProgramId_LevelId_IsActive"
                ON public."CurriculumAssignments" ("BranchId", "ProgramId", "LevelId", "IsActive");
                """);

            migrationBuilder.Sql(
                """
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_CurriculumAssignments_BranchId_ProgramId_LevelId_SyllabusId_EffectiveFrom"
                ON public."CurriculumAssignments" ("BranchId", "ProgramId", "LevelId", "SyllabusId", "EffectiveFrom");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reconcile migration intentionally keeps existing schema unchanged on rollback.
        }
    }
}
