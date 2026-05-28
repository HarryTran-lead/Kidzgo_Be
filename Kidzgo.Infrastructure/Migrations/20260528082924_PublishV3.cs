using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PublishV3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsParentPublished",
                schema: "public",
                table: "StudentReports",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ParentPublishedAt",
                schema: "public",
                table: "StudentReports",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentPublishedBy",
                schema: "public",
                table: "StudentReports",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentReports_ParentPublishedBy",
                schema: "public",
                table: "StudentReports",
                column: "ParentPublishedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentReports_Users_ParentPublishedBy",
                schema: "public",
                table: "StudentReports",
                column: "ParentPublishedBy",
                principalSchema: "public",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentReports_Users_ParentPublishedBy",
                schema: "public",
                table: "StudentReports");

            migrationBuilder.DropIndex(
                name: "IX_StudentReports_ParentPublishedBy",
                schema: "public",
                table: "StudentReports");

            migrationBuilder.DropColumn(
                name: "IsParentPublished",
                schema: "public",
                table: "StudentReports");

            migrationBuilder.DropColumn(
                name: "ParentPublishedAt",
                schema: "public",
                table: "StudentReports");

            migrationBuilder.DropColumn(
                name: "ParentPublishedBy",
                schema: "public",
                table: "StudentReports");
        }
    }
}
