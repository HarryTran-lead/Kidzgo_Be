using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260426120000_EnhanceLandingPageSettingsMarketingContent")]
    public partial class EnhanceLandingPageSettingsMarketingContent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeaturedClassesSectionSubtitle",
                schema: "public",
                table: "LandingPageSettings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedClassesSectionTitle",
                schema: "public",
                table: "LandingPageSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedClassConfigsJson",
                schema: "public",
                table: "LandingPageSettings",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FeaturedProgramsSectionSubtitle",
                schema: "public",
                table: "LandingPageSettings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedProgramsSectionTitle",
                schema: "public",
                table: "LandingPageSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedProgramConfigsJson",
                schema: "public",
                table: "LandingPageSettings",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FeaturedTeachersSectionSubtitle",
                schema: "public",
                table: "LandingPageSettings",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FeaturedTeachersSectionTitle",
                schema: "public",
                table: "LandingPageSettings",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FooterAddressesJson",
                schema: "public",
                table: "LandingPageSettings",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "FooterSocialLinksJson",
                schema: "public",
                table: "LandingPageSettings",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeaturedClassesSectionSubtitle",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedClassesSectionTitle",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedClassConfigsJson",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedProgramsSectionSubtitle",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedProgramsSectionTitle",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedProgramConfigsJson",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedTeachersSectionSubtitle",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FeaturedTeachersSectionTitle",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FooterAddressesJson",
                schema: "public",
                table: "LandingPageSettings");

            migrationBuilder.DropColumn(
                name: "FooterSocialLinksJson",
                schema: "public",
                table: "LandingPageSettings");
        }
    }
}
