using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePolicyContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewStudentPolicyText",
                schema: "public",
                table: "EnrollmentConfirmationPaymentSettings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationPolicyText",
                schema: "public",
                table: "EnrollmentConfirmationPaymentSettings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewStudentPolicyText",
                schema: "public",
                table: "EnrollmentConfirmationPaymentSettings");

            migrationBuilder.DropColumn(
                name: "ReservationPolicyText",
                schema: "public",
                table: "EnrollmentConfirmationPaymentSettings");
        }
    }
}
