using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TeachingLogFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActualContent",
                schema: "public",
                table: "TeachingLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualHomework",
                schema: "public",
                table: "TeachingLogs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActualLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActualTeachingType",
                schema: "public",
                table: "TeachingLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PlannedLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeacherNote",
                schema: "public",
                table: "TeachingLogs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogs_ActualLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs",
                column: "ActualLessonPlanTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingLogs_PlannedLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs",
                column: "PlannedLessonPlanTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingLogs_LessonPlanTemplates_ActualLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs",
                column: "ActualLessonPlanTemplateId",
                principalSchema: "public",
                principalTable: "LessonPlanTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TeachingLogs_LessonPlanTemplates_PlannedLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs",
                column: "PlannedLessonPlanTemplateId",
                principalSchema: "public",
                principalTable: "LessonPlanTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeachingLogs_LessonPlanTemplates_ActualLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_TeachingLogs_LessonPlanTemplates_PlannedLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TeachingLogs_ActualLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropIndex(
                name: "IX_TeachingLogs_PlannedLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropColumn(
                name: "ActualContent",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropColumn(
                name: "ActualHomework",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropColumn(
                name: "ActualLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropColumn(
                name: "ActualTeachingType",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropColumn(
                name: "PlannedLessonPlanTemplateId",
                schema: "public",
                table: "TeachingLogs");

            migrationBuilder.DropColumn(
                name: "TeacherNote",
                schema: "public",
                table: "TeachingLogs");
        }
    }
}
