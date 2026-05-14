using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLearningTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SectionType",
                schema: "public",
                table: "Sessions",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.CreateTable(
                name: "LearningTicketItems",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ConsumedBySessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsumedByAttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTicketItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTicketItems_Attendances_ConsumedByAttendanceId",
                        column: x => x.ConsumedByAttendanceId,
                        principalSchema: "public",
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearningTicketItems_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningTicketItems_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningTicketItems_Sessions_ConsumedBySessionId",
                        column: x => x.ConsumedBySessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LearningTicketLedgers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningTicketItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    AttendanceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTicketLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LearningTicketLedgers_Attendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalSchema: "public",
                        principalTable: "Attendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearningTicketLedgers_LearningTicketItems_LearningTicketIte~",
                        column: x => x.LearningTicketItemId,
                        principalSchema: "public",
                        principalTable: "LearningTicketItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearningTicketLedgers_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningTicketLedgers_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalSchema: "public",
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LearningTicketLedgers_Sessions_SessionId",
                        column: x => x.SessionId,
                        principalSchema: "public",
                        principalTable: "Sessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LearningTicketLedgers_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalSchema: "public",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketItems_ConsumedByAttendanceId",
                schema: "public",
                table: "LearningTicketItems",
                column: "ConsumedByAttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketItems_ConsumedBySessionId",
                schema: "public",
                table: "LearningTicketItems",
                column: "ConsumedBySessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketItems_RegistrationId_Status_CreatedAt",
                schema: "public",
                table: "LearningTicketItems",
                columns: new[] { "RegistrationId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketItems_StudentProfileId",
                schema: "public",
                table: "LearningTicketItems",
                column: "StudentProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketLedgers_AttendanceId",
                schema: "public",
                table: "LearningTicketLedgers",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketLedgers_CreatedByUserId",
                schema: "public",
                table: "LearningTicketLedgers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketLedgers_LearningTicketItemId",
                schema: "public",
                table: "LearningTicketLedgers",
                column: "LearningTicketItemId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketLedgers_RegistrationId_CreatedAt",
                schema: "public",
                table: "LearningTicketLedgers",
                columns: new[] { "RegistrationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketLedgers_SessionId",
                schema: "public",
                table: "LearningTicketLedgers",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_LearningTicketLedgers_StudentProfileId",
                schema: "public",
                table: "LearningTicketLedgers",
                column: "StudentProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LearningTicketLedgers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LearningTicketItems",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "SectionType",
                schema: "public",
                table: "Sessions");
        }
    }
}
