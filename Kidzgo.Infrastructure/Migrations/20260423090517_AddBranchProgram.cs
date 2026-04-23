using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchProgram : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Branches_BranchId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropForeignKey(
                name: "FK_Programs_Classes_DefaultMakeupClassId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Programs_BranchId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropIndex(
                name: "IX_Programs_DefaultMakeupClassId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "public",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "DefaultMakeupClassId",
                schema: "public",
                table: "Programs");

            migrationBuilder.CreateTable(
                name: "BranchPrograms",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProgramId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultMakeupClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchPrograms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BranchPrograms_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "public",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchPrograms_Classes_DefaultMakeupClassId",
                        column: x => x.DefaultMakeupClassId,
                        principalSchema: "public",
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchPrograms_Programs_ProgramId",
                        column: x => x.ProgramId,
                        principalSchema: "public",
                        principalTable: "Programs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BranchPrograms_BranchId_ProgramId",
                schema: "public",
                table: "BranchPrograms",
                columns: new[] { "BranchId", "ProgramId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BranchPrograms_DefaultMakeupClassId",
                schema: "public",
                table: "BranchPrograms",
                column: "DefaultMakeupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchPrograms_ProgramId",
                schema: "public",
                table: "BranchPrograms",
                column: "ProgramId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BranchPrograms",
                schema: "public");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "public",
                table: "Programs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultMakeupClassId",
                schema: "public",
                table: "Programs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Programs_BranchId",
                schema: "public",
                table: "Programs",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Programs_DefaultMakeupClassId",
                schema: "public",
                table: "Programs",
                column: "DefaultMakeupClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Branches_BranchId",
                schema: "public",
                table: "Programs",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Programs_Classes_DefaultMakeupClassId",
                schema: "public",
                table: "Programs",
                column: "DefaultMakeupClassId",
                principalSchema: "public",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
