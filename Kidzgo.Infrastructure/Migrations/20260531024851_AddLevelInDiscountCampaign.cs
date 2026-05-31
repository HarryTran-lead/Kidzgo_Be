using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLevelInDiscountCampaign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LevelId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_LevelId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationDiscountCampaigns_Levels_LevelId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "LevelId",
                principalSchema: "public",
                principalTable: "Levels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationDiscountCampaigns_Levels_LevelId",
                schema: "public",
                table: "RegistrationDiscountCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationDiscountCampaigns_LevelId",
                schema: "public",
                table: "RegistrationDiscountCampaigns");

            migrationBuilder.DropColumn(
                name: "LevelId",
                schema: "public",
                table: "RegistrationDiscountCampaigns");
        }
    }
}
