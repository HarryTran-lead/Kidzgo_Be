using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260426123000_AddLandingPageFooterContactPhones")]
    public partial class AddLandingPageFooterContactPhones : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FooterContactPhonesJson",
                schema: "public",
                table: "LandingPageSettings",
                type: "text",
                nullable: false,
                defaultValue: "[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FooterContactPhonesJson",
                schema: "public",
                table: "LandingPageSettings");
        }
    }
}
