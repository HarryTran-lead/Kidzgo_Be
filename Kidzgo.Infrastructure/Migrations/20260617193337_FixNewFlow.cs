using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNewFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_Modules_ModuleId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropTable(
                name: "PackageCurriculumMappings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TicketTypeCompatibilities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TuitionPlanModuleSelections",
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
                name: "IX_TuitionPlans_ModuleId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_ProgramId_LevelId_ModuleId_TotalSessions_Name",
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
                name: "ModuleId",
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

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ProgramId_LevelId_TotalSessions_Name",
                schema: "public",
                table: "TuitionPlans",
                columns: new[] { "ProgramId", "LevelId", "TotalSessions", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_ProgramId_LevelId_TotalSessions_Name",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.AddColumn<Guid>(
                name: "LearningTicketTypeId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
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
                    AllowedDayGroups = table.Column<int>(type: "integer", nullable: false),
                    AllowedTeacherTypes = table.Column<int>(type: "integer", nullable: false),
                    AllowedTimeBands = table.Column<int>(type: "integer", nullable: false),
                    AllowedUsageTypes = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompatibilityMode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningTicketTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PackageCurriculumMappings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageCurriculumMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageCurriculumMappings_Syllabuses_SyllabusId",
                        column: x => x.SyllabusId,
                        principalSchema: "public",
                        principalTable: "Syllabuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageCurriculumMappings_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SlotTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DayGroup = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TeacherType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TimeBand = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsageType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SlotTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TuitionPlanModuleSelections",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TuitionPlanModuleSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TuitionPlanModuleSelections_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalSchema: "public",
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TuitionPlanModuleSelections_TuitionPlans_TuitionPlanId",
                        column: x => x.TuitionPlanId,
                        principalSchema: "public",
                        principalTable: "TuitionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketTypeCompatibilities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LearningTicketTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompatible = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "IX_TuitionPlans_ModuleId",
                schema: "public",
                table: "TuitionPlans",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_ProgramId_LevelId_ModuleId_TotalSessions_Name",
                schema: "public",
                table: "TuitionPlans",
                columns: new[] { "ProgramId", "LevelId", "ModuleId", "TotalSessions", "Name" },
                unique: true);

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
                name: "IX_PackageCurriculumMappings_SyllabusId",
                schema: "public",
                table: "PackageCurriculumMappings",
                column: "SyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCurriculumMappings_TuitionPlanId_IsActive",
                schema: "public",
                table: "PackageCurriculumMappings",
                columns: new[] { "TuitionPlanId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageCurriculumMappings_TuitionPlanId_SyllabusId",
                schema: "public",
                table: "PackageCurriculumMappings",
                columns: new[] { "TuitionPlanId", "SyllabusId" },
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

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlanModuleSelections_ModuleId",
                schema: "public",
                table: "TuitionPlanModuleSelections",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlanModuleSelections_TuitionPlanId_ModuleId",
                schema: "public",
                table: "TuitionPlanModuleSelections",
                columns: new[] { "TuitionPlanId", "ModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlanModuleSelections_TuitionPlanId_OrderIndex",
                schema: "public",
                table: "TuitionPlanModuleSelections",
                columns: new[] { "TuitionPlanId", "OrderIndex" },
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionPlans_Modules_ModuleId",
                schema: "public",
                table: "TuitionPlans",
                column: "ModuleId",
                principalSchema: "public",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
