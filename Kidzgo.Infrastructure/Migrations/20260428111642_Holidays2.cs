using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Holidays2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Holidays_Branches_BranchId",
                schema: "public",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_BranchId_StartDate_EndDate",
                schema: "public",
                table: "Holidays");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "public",
                table: "Holidays");

            migrationBuilder.CreateTable(
                name: "MakeupSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreditExpiryDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MakeupSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_StartDate_EndDate",
                schema: "public",
                table: "Holidays",
                columns: new[] { "StartDate", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MakeupSettings",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Holidays_StartDate_EndDate",
                schema: "public",
                table: "Holidays");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "public",
                table: "Holidays",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_BranchId_StartDate_EndDate",
                schema: "public",
                table: "Holidays",
                columns: new[] { "BranchId", "StartDate", "EndDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_Holidays_Branches_BranchId",
                schema: "public",
                table: "Holidays",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
