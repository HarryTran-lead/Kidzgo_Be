using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(Kidzgo.Infrastructure.Database.ApplicationDbContext))]
    [Migration("20260426093000_RenameKidzgoToRexInPauseNotificationTemplates")]
    public partial class RenameKidzgoToRexInPauseNotificationTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE public."NotificationTemplates"
                SET "Content" = REPLACE("Content", 'KidzGo', 'Rex')
                WHERE "Code" IN ('PAUSE_ENROLLMENT_APPROVED_ZALO', 'PAUSE_ENROLLMENT_REJECTED_ZALO')
                  AND "Content" LIKE '%KidzGo%';
                """);

            migrationBuilder.Sql("""
                UPDATE public."Notifications"
                SET "SenderName" = 'Rex Centre'
                WHERE "Channel" = 'InApp'
                  AND "SenderName" = 'KidzGo Centre';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE public."NotificationTemplates"
                SET "Content" = REPLACE("Content", 'Rex', 'KidzGo')
                WHERE "Code" IN ('PAUSE_ENROLLMENT_APPROVED_ZALO', 'PAUSE_ENROLLMENT_REJECTED_ZALO')
                  AND "Content" LIKE '%Rex%';
                """);

            migrationBuilder.Sql("""
                UPDATE public."Notifications"
                SET "SenderName" = 'KidzGo Centre'
                WHERE "Channel" = 'InApp'
                  AND "SenderName" = 'Rex Centre';
                """);
        }
    }
}
