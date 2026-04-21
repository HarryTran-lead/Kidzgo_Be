using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedProfileCreatedEmailTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                schema: "public",
                table: "Profiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "public",
                table: "Profiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "public",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                schema: "public",
                table: "Profiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ZaloId",
                schema: "public",
                table: "Profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "public",
                table: "LeadChildren",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Dob",
                schema: "public",
                table: "LeadChildren",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.InsertData(
                schema: "public",
                table: "EmailTemplates",
                columns: new[] { "Id", "Body", "Code", "CreatedAt", "IsActive", "IsDeleted", "Placeholders", "Subject", "UpdatedAt" },
                values: new object[] { new Guid("b9f6c8a1-3f57-45c6-8f4b-9f0c2b7d7f10"), "<div style=\"margin:0;padding:0;background:#f4f7fb;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#1f2937;\">\n  <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"background:#f4f7fb;padding:24px 12px;\">\n    <tr>\n      <td align=\"center\">\n        <table role=\"presentation\" width=\"640\" cellspacing=\"0\" cellpadding=\"0\" style=\"max-width:640px;background:#ffffff;border-radius:18px;overflow:hidden;box-shadow:0 10px 30px rgba(15,23,42,.08);\">\n          <tr>\n            <td style=\"padding:0;background:linear-gradient(135deg,#0ea5e9 0%,#2563eb 100%);\">\n              <div style=\"padding:28px 30px 24px 30px;color:#ffffff;\">\n                <p style=\"margin:0 0 8px 0;font-size:13px;letter-spacing:.08em;text-transform:uppercase;opacity:.9;\">KidzGo Learning Center</p>\n                <h1 style=\"margin:0;font-size:28px;line-height:1.3;font-weight:700;\">Ho so moi da san sang</h1>\n                <p style=\"margin:10px 0 0 0;font-size:15px;line-height:1.6;opacity:.95;\">\n                  Xin chao {{profile_name}}, tai khoan {{profile_type}} cua ban da duoc tao va san sang cho buoc xac minh.\n                </p>\n              </div>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:26px 30px 12px 30px;\">\n              <p style=\"margin:0 0 14px 0;font-size:14px;line-height:1.7;color:#475569;\">\n                Vui long kiem tra thong tin ben duoi. Neu can chinh sua, ban co the cap nhat nhanh chi voi 1 click.\n              </p>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:0 30px 20px 30px;\">\n              <table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border:1px solid #e2e8f0;border-radius:12px;background:#f8fafc;\">\n                <tr>\n                  <td style=\"padding:16px 18px;\">\n                    <p style=\"margin:0 0 8px 0;font-size:13px;color:#64748b;\">Thong tin ho so</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Ten hien thi:</strong> {{profile_name}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Ho ten:</strong> {{full_name}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Loai ho so:</strong> {{profile_type}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Gioi tinh:</strong> {{gender}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Ngay sinh:</strong> {{birth_day}}</p>\n                    <p style=\"margin:0 0 6px 0;font-size:14px;\"><strong>Email:</strong> {{email}}</p>\n                    <p style=\"margin:0 0 0 0;font-size:14px;\"><strong>So dien thoai:</strong> {{phone}}</p>\n                  </td>\n                </tr>\n              </table>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:6px 30px 28px 30px;\">\n              <table role=\"presentation\" cellspacing=\"0\" cellpadding=\"0\">\n                <tr>\n                  <td style=\"padding-right:10px;\">\n                    <a href=\"{{verify_link}}\" style=\"display:inline-block;background:#2563eb;color:#ffffff;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;\">Xac minh ho so</a>\n                  </td>\n                  <td>\n                    <a href=\"{{update_link}}\" style=\"display:inline-block;background:#ffffff;color:#1d4ed8;text-decoration:none;font-weight:600;font-size:14px;padding:12px 18px;border-radius:10px;border:1px solid #bfdbfe;\">Cap nhat thong tin</a>\n                  </td>\n                </tr>\n              </table>\n            </td>\n          </tr>\n          <tr>\n            <td style=\"padding:18px 30px;background:#f8fafc;border-top:1px solid #e2e8f0;\">\n              <p style=\"margin:0;font-size:12px;line-height:1.7;color:#64748b;\">\n                Neu ban khong thuc hien thao tac nay, vui long bo qua email hoac lien he bo phan ho tro cua KidzGo.\n              </p>\n            </td>\n          </tr>\n        </table>\n      </td>\n    </tr>\n  </table>\n</div>", "PROFILE_CREATED", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, false, "[\"profile_name\",\"profile_type\",\"email\",\"phone\",\"full_name\",\"gender\",\"birth_day\",\"verify_link\",\"update_link\"]", "KidzGo | Ho so {{profile_name}} da duoc tao thanh cong", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "public",
                table: "EmailTemplates",
                keyColumn: "Id",
                keyValue: new Guid("b9f6c8a1-3f57-45c6-8f4b-9f0c2b7d7f10"));

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                schema: "public",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ZaloId",
                schema: "public",
                table: "Profiles");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "public",
                table: "LeadChildren",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Dob",
                schema: "public",
                table: "LeadChildren",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);
        }
    }
}
