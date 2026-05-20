using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonPlanUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LessonPlanUnitId",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderIndexInUnit",
                schema: "public",
                table: "LessonPlanTemplates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LessonPlanUnits",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonPlanUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonPlanUnits_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_LessonPlanUnitId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "LessonPlanUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanTemplates_LessonPlanUnitId_OrderIndexInUnit",
                schema: "public",
                table: "LessonPlanTemplates",
                columns: new[] { "LessonPlanUnitId", "OrderIndexInUnit" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanUnits_ModuleId_NameNormalized",
                schema: "public",
                table: "LessonPlanUnits",
                columns: new[] { "ModuleId", "NameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonPlanUnits_ModuleId_OrderIndex",
                schema: "public",
                table: "LessonPlanUnits",
                columns: new[] { "ModuleId", "OrderIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_LessonPlanTemplates_LessonPlanUnits_LessonPlanUnitId",
                schema: "public",
                table: "LessonPlanTemplates",
                column: "LessonPlanUnitId",
                principalSchema: "public",
                principalTable: "LessonPlanUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LessonPlanTemplates_LessonPlanUnits_LessonPlanUnitId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropTable(
                name: "LessonPlanUnits",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_LessonPlanUnitId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropIndex(
                name: "IX_LessonPlanTemplates_LessonPlanUnitId_OrderIndexInUnit",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "LessonPlanUnitId",
                schema: "public",
                table: "LessonPlanTemplates");

            migrationBuilder.DropColumn(
                name: "OrderIndexInUnit",
                schema: "public",
                table: "LessonPlanTemplates");
        }
    }
}
