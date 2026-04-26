using System;
using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260426103000_AddLandingPageSettings")]
    public partial class AddLandingPageSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LandingPageSettings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FooterAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FooterContactPhone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FooterContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    FeaturedProgramIdsJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedClassIdsJson = table.Column<string>(type: "text", nullable: false),
                    FeaturedTeacherIdsJson = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandingPageSettings", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LandingPageSettings",
                schema: "public");
        }
    }
}
