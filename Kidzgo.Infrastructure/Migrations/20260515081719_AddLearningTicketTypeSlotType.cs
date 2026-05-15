using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningTicketTypeSlotType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SlotTypeId",
                schema: "public",
                table: "Sessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LearningTicketTypeId",
                schema: "public",
                table: "LearningTicketItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SlotTypeId",
                schema: "public",
                table: "Classes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LearningTicketTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTicketTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SlotTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypeCompatibilities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningTicketTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCompatible = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTypeCompatibilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTypeCompatibilities_LearningTicketTypes_LearningTicke~",
                        column: x => x.LearningTicketTypeId,
                        principalSchema: "public",
                        principalTable: "LearningTicketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTypeCompatibilities_SlotTypes_SlotTypeId",
                        column: x => x.SlotTypeId,
                        principalSchema: "public",
                        principalTable: "SlotTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans",
                column: "LearningTicketTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_SlotTypeId",
                schema: "public",
                table: "Sessions",
                column: "SlotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketItems_LearningTicketTypeId",
                schema: "public",
                table: "LearningTicketItems",
                column: "LearningTicketTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketItems_RegistrationId_LearningTicketTypeId_Sta~",
                schema: "public",
                table: "LearningTicketItems",
                columns: new[] { "RegistrationId", "LearningTicketTypeId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_SlotTypeId",
                schema: "public",
                table: "Classes",
                column: "SlotTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketTypes_Code",
                schema: "public",
                table: "LearningTicketTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SlotTypes_Code",
                schema: "public",
                table: "SlotTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeCompatibilities_LearningTicketTypeId_SlotTypeId",
                schema: "public",
                table: "TicketTypeCompatibilities",
                columns: new[] { "LearningTicketTypeId", "SlotTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketTypeCompatibilities_SlotTypeId",
                schema: "public",
                table: "TicketTypeCompatibilities",
                column: "SlotTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_SlotTypes_SlotTypeId",
                schema: "public",
                table: "Classes",
                column: "SlotTypeId",
                principalSchema: "public",
                principalTable: "SlotTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LearningTicketItems_LearningTicketTypes_LearningTicketTypeId",
                schema: "public",
                table: "LearningTicketItems",
                column: "LearningTicketTypeId",
                principalSchema: "public",
                principalTable: "LearningTicketTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessions_SlotTypes_SlotTypeId",
                schema: "public",
                table: "Sessions",
                column: "SlotTypeId",
                principalSchema: "public",
                principalTable: "SlotTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionPlans_LearningTicketTypes_LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans",
                column: "LearningTicketTypeId",
                principalSchema: "public",
                principalTable: "LearningTicketTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_SlotTypes_SlotTypeId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_LearningTicketItems_LearningTicketTypes_LearningTicketTypeId",
                schema: "public",
                table: "LearningTicketItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Sessions_SlotTypes_SlotTypeId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_LearningTicketTypes_LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropTable(
                name: "TicketTypeCompatibilities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LearningTicketTypes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "SlotTypes",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_Sessions_SlotTypeId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropIndex(
                name: "IX_LearningTicketItems_LearningTicketTypeId",
                schema: "public",
                table: "LearningTicketItems");

            migrationBuilder.DropIndex(
                name: "IX_LearningTicketItems_RegistrationId_LearningTicketTypeId_Sta~",
                schema: "public",
                table: "LearningTicketItems");

            migrationBuilder.DropIndex(
                name: "IX_Classes_SlotTypeId",
                schema: "public",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropColumn(
                name: "SlotTypeId",
                schema: "public",
                table: "Sessions");

            migrationBuilder.DropColumn(
                name: "LearningTicketTypeId",
                schema: "public",
                table: "LearningTicketItems");

            migrationBuilder.DropColumn(
                name: "SlotTypeId",
                schema: "public",
                table: "Classes");
        }
    }
}
