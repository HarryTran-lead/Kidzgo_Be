using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteToQuestionBankItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionBankItems_ProgramId_Level",
                schema: "public",
                table: "QuestionBankItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "public",
                table: "QuestionBankItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_ProgramId_Level_IsDeleted",
                schema: "public",
                table: "QuestionBankItems",
                columns: new[] { "ProgramId", "Level", "IsDeleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_QuestionBankItems_ProgramId_Level_IsDeleted",
                schema: "public",
                table: "QuestionBankItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "public",
                table: "QuestionBankItems");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionBankItems_ProgramId_Level",
                schema: "public",
                table: "QuestionBankItems",
                columns: new[] { "ProgramId", "Level" });
        }
    }
}
