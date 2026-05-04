using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBrandIdInTuitionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TuitionPlans_Branches_BranchId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropIndex(
                name: "IX_TuitionPlans_BranchId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "public",
                table: "TuitionPlans");

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("c1f73d87-8d4c-45c2-bf8f-3d79e2f4b6a1"),
                column: "Body",
                value: "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\r\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\r\n    <tr>\r\n      <td align=\"center\">\r\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\r\n          <tr>\r\n            <td style=\"padding:0;background:linear-gradient(135deg,#f97316 0%,#ea580c 100%);\">\r\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\r\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\r\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Đặt lại PIN phụ huynh</h1>\r\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\r\n                  Xin chào {{user_name}}, chúng tôi đã nhận được yêu cầu đặt lại PIN cho hồ sơ {{profile_name}}.\r\n                </p>\r\n              </div>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:26px 30px 12px 30px;\">\r\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\r\n                Để tiếp tục, vui lòng bấm vào nút bên dưới. Liên kết này chỉ có hiệu lực trong 1 giờ.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n          <tr>\r\n            <td style=\"padding:8px 30px 28px 30px;\">\r\n              <a href=\"{{reset_link}}\" style=\"display:inline-block;background:#ea580c;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Đặt lại PIN</a>\r\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\r\n                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\r\n              </p>\r\n            </td>\r\n          </tr>\r\n        </table>\r\n      </td>\r\n    </tr>\r\n  </table>\r\n</div>");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                schema: "public",
                table: "TuitionPlans",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("c1f73d87-8d4c-45c2-bf8f-3d79e2f4b6a1"),
                column: "Body",
                value: "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\n    <tr>\n      <td align=\"center\">\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\n          <tr>\n            <td style=\"padding:0;background:linear-gradient(135deg,#f97316 0%,#ea580c 100%);\">\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Đặt lại PIN phụ huynh</h1>\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\n                  Xin chào {{user_name}}, chúng tôi đã nhận được yêu cầu đặt lại PIN cho hồ sơ {{profile_name}}.\n                </p>\n              </div>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:26px 30px 12px 30px;\">\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\n                Để tiếp tục, vui lòng bấm vào nút bên dưới. Liên kết này chỉ có hiệu lực trong 1 giờ.\n              </p>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:8px 30px 28px 30px;\">\n              <a href=\"{{reset_link}}\" style=\"display:inline-block;background:#ea580c;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Đặt lại PIN</a>\n              <p style=\"margin:14px 0 0 0;font-size:12px;line-height:1.6;color:#64748b;\">\n                Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ bộ phận hỗ trợ của KidzGo.\n              </p>\n            </td>\n          </tr>\n        </table>\n      </td>\n    </tr>\n  </table>\n</div>");

            migrationBuilder.CreateIndex(
                name: "IX_TuitionPlans_BranchId",
                schema: "public",
                table: "TuitionPlans",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_TuitionPlans_Branches_BranchId",
                schema: "public",
                table: "TuitionPlans",
                column: "BranchId",
                principalSchema: "public",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
