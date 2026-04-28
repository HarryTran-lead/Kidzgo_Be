using Kidzgo.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260428113000_AddRegistrationDiscountCampaigns")]
    public partial class AddRegistrationDiscountCampaigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationDiscountCampaigns",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: true),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ApplyForInitialRegistration = table.Column<bool>(type: "boolean", nullable: false),
                    ApplyForRenewal = table.Column<bool>(type: "boolean", nullable: false),
                    ApplyForUpgrade = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationDiscountCampaigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationDiscountCampaigns_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationDiscountCampaigns_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrationDiscountCampaigns_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<decimal>(
                name: "CarryOverCreditAmount",
                schema: "public",
                table: "Registrations",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                schema: "public",
                table: "Registrations",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountCampaignId",
                schema: "public",
                table: "Registrations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountCampaignName",
                schema: "public",
                table: "Registrations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountType",
                schema: "public",
                table: "Registrations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountValue",
                schema: "public",
                table: "Registrations",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FinalTuitionAmount",
                schema: "public",
                table: "Registrations",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalTuitionAmount",
                schema: "public",
                table: "Registrations",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PricingAppliedAt",
                schema: "public",
                table: "Registrations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_DiscountCampaignId",
                schema: "public",
                table: "Registrations",
                column: "DiscountCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_BranchId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_IsActive_StartDate_EndDate_Priority",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                columns: new[] { "IsActive", "StartDate", "EndDate", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_ProgramId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "ProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationDiscountCampaigns_TuitionPlanId",
                schema: "public",
                table: "RegistrationDiscountCampaigns",
                column: "TuitionPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Registrations_RegistrationDiscountCampaigns_DiscountCampaignId",
                schema: "public",
                table: "Registrations",
                column: "DiscountCampaignId",
                principalSchema: "public",
                principalTable: "RegistrationDiscountCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Registrations_RegistrationDiscountCampaigns_DiscountCampaignId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropTable(
                name: "RegistrationDiscountCampaigns",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_Registrations_DiscountCampaignId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "CarryOverCreditAmount",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountCampaignId",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountCampaignName",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountType",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "DiscountValue",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "FinalTuitionAmount",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "OriginalTuitionAmount",
                schema: "public",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "PricingAppliedAt",
                schema: "public",
                table: "Registrations");
        }
    }
}
