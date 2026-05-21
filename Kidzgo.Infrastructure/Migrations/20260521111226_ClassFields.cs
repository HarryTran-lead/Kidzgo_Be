using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ClassFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompletedSessions",
                schema: "public",
                table: "ClassModuleProgresses",
                newName: "StartSessionIndex");

            migrationBuilder.AddColumn<int>(
                name: "CompletedClassSessions",
                schema: "public",
                table: "ClassModuleProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CompletedLessonPlans",
                schema: "public",
                table: "ClassModuleProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CurrentSessionIndex",
                schema: "public",
                table: "ClassModuleProgresses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ActualEndDate",
                schema: "public",
                table: "Classes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrentLessonPlanTemplateId",
                schema: "public",
                table: "Classes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentSessionIndex",
                schema: "public",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpectedEndDate",
                schema: "public",
                table: "Classes",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StartSessionIndex",
                schema: "public",
                table: "Classes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_CurrentLessonPlanTemplateId",
                schema: "public",
                table: "Classes",
                column: "CurrentLessonPlanTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_LessonPlanTemplates_CurrentLessonPlanTemplateId",
                schema: "public",
                table: "Classes",
                column: "CurrentLessonPlanTemplateId",
                principalSchema: "public",
                principalTable: "LessonPlanTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_LessonPlanTemplates_CurrentLessonPlanTemplateId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_CurrentLessonPlanTemplateId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CompletedClassSessions",
                schema: "public",
                table: "ClassModuleProgresses");

            migrationBuilder.DropColumn(
                name: "CompletedLessonPlans",
                schema: "public",
                table: "ClassModuleProgresses");

            migrationBuilder.DropColumn(
                name: "CurrentSessionIndex",
                schema: "public",
                table: "ClassModuleProgresses");

            migrationBuilder.DropColumn(
                name: "ActualEndDate",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CurrentLessonPlanTemplateId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "CurrentSessionIndex",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ExpectedEndDate",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "StartSessionIndex",
                schema: "public",
                table: "Classes");

            migrationBuilder.RenameColumn(
                name: "StartSessionIndex",
                schema: "public",
                table: "ClassModuleProgresses",
                newName: "CompletedSessions");
        }
    }
}
