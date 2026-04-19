using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PDFCase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservationExpiresOn",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservationSnapshotAt",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedSessionCount",
                schema: "public",
                table: "PauseEnrollmentRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReservedSessionCount",
                schema: "public",
                table: "PauseEnrollmentRequestHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationExpiresOn",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "ReservationSnapshotAt",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "ReservedSessionCount",
                schema: "public",
                table: "PauseEnrollmentRequests");

            migrationBuilder.DropColumn(
                name: "ReservedSessionCount",
                schema: "public",
                table: "PauseEnrollmentRequestHistories");
        }
    }
}
