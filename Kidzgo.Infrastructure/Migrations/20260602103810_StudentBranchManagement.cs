using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StudentBranchManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PackageCurriculumMappings",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TuitionPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    SyllabusId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                name: "StudentBranchStates",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActiveBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    AllowCrossBranchEnrollment = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LastTransferredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentBranchStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentBranchStates_Branches_ActiveBranchId",
                        column: x => x.ActiveBranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentBranchStates_Branches_HomeBranchId",
                        column: x => x.HomeBranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentBranchStates_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentBranchTransfers",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    KeepCurrentClass = table.Column<bool>(type: "boolean", nullable: false),
                    AllowCrossBranchEnrollment = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentBranchTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentBranchTransfers_Branches_FromBranchId",
                        column: x => x.FromBranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentBranchTransfers_Branches_ToBranchId",
                        column: x => x.ToBranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentBranchTransfers_Profiles_StudentProfileId",
                        column: x => x.StudentProfileId,
                        principalSchema: "public",
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_StudentBranchStates_ActiveBranchId",
                schema: "public",
                table: "StudentBranchStates",
                column: "ActiveBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBranchStates_HomeBranchId",
                schema: "public",
                table: "StudentBranchStates",
                column: "HomeBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBranchStates_StudentProfileId",
                schema: "public",
                table: "StudentBranchStates",
                column: "StudentProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentBranchTransfers_FromBranchId",
                schema: "public",
                table: "StudentBranchTransfers",
                column: "FromBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentBranchTransfers_StudentProfileId_CreatedAt",
                schema: "public",
                table: "StudentBranchTransfers",
                columns: new[] { "StudentProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StudentBranchTransfers_ToBranchId",
                schema: "public",
                table: "StudentBranchTransfers",
                column: "ToBranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PackageCurriculumMappings",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentBranchStates",
                schema: "public");

            migrationBuilder.DropTable(
                name: "StudentBranchTransfers",
                schema: "public");
        }
    }
}
