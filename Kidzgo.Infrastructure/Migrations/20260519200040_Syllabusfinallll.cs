using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Syllabusfinallll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurriculumImportConfigurations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    LevelId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegularUnitLessonPlanCount = table.Column<int>(type: "integer", nullable: false),
                    StarterUnitLessonPlanCount = table.Column<int>(type: "integer", nullable: false),
                    RevisionLessonPlanCount = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumImportConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumImportConfigurations_Levels_LevelId",
                        column: x => x.LevelId,
                        principalSchema: "public",
                        principalTable: "Levels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CurriculumImportConfigurations_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CurriculumImportModuleRules",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurriculumImportConfigurationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IncludeStarterUnit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UnitFrom = table.Column<int>(type: "integer", nullable: true),
                    UnitTo = table.Column<int>(type: "integer", nullable: true),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurriculumImportModuleRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CurriculumImportModuleRules_CurriculumImportConfigurations_~",
                        column: x => x.CurriculumImportConfigurationId,
                        principalSchema: "public",
                        principalTable: "CurriculumImportConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurriculumImportModuleRules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumImportConfigurations_LevelId",
                schema: "public",
                table: "CurriculumImportConfigurations",
                column: "LevelId");

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumImportConfigurations_ProgramId_LevelId",
                schema: "public",
                table: "CurriculumImportConfigurations",
                columns: new[] { "ProgramId", "LevelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumImportModuleRules_CurriculumImportConfigurationI~1",
                schema: "public",
                table: "CurriculumImportModuleRules",
                columns: new[] { "CurriculumImportConfigurationId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumImportModuleRules_CurriculumImportConfigurationId~",
                schema: "public",
                table: "CurriculumImportModuleRules",
                columns: new[] { "CurriculumImportConfigurationId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurriculumImportModuleRules_ModuleId",
                schema: "public",
                table: "CurriculumImportModuleRules",
                column: "ModuleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurriculumImportModuleRules",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CurriculumImportConfigurations",
                schema: "public");
        }
    }
}
