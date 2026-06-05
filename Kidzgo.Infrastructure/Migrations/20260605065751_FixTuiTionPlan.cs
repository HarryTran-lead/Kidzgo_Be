using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixTuiTionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TuitionPlanModuleSelections",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuitionPlanModuleSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TuitionPlanModuleSelections_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TuitionPlanModuleSelections_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlanModuleSelections_ModuleId",
                schema: "public",
                table: "TuitionPlanModuleSelections",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlanModuleSelections_TuitionPlanId_ModuleId",
                schema: "public",
                table: "TuitionPlanModuleSelections",
                columns: new[] { "TuitionPlanId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlanModuleSelections_TuitionPlanId_OrderIndex",
                schema: "public",
                table: "TuitionPlanModuleSelections",
                columns: new[] { "TuitionPlanId", "OrderIndex" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TuitionPlanModuleSelections",
                schema: "public");
        }
    }
}
