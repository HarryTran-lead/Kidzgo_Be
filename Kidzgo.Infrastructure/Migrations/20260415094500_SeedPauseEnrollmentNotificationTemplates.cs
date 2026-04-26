using System.Linq;
using Kidzgo.Domain.Notifications;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kidzgo.Infrastructure.Migrations
{
    [DbContext(typeof(Kidzgo.Infrastructure.Database.ApplicationDbContext))]
    [Migration("20260415094500_SeedPauseEnrollmentNotificationTemplates")]
    public partial class SeedPauseEnrollmentNotificationTemplates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Code = "PAUSE_ENROLLMENT_APPROVED_EMAIL",
                Channel = NotificationChannel.Email,
                Title = "KidzGo | Lịch bảo lưu của {{student_name}} đã được xác nhận",
                Content = """
                          <div style="margin:0;padding:0;background:#eef6ff;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#10233d;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#eef6ff;padding:24px 12px;">
                              <tr>
                                <td align="center">
                                  <table role="presentation" width="640" cellspacing="0" cellpadding="0" style="max-width:640px;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 14px 36px rgba(15,23,42,.12);">
                                    <tr>
                                      <td style="padding:0;background:linear-gradient(135deg,#1d4ed8 0%,#0ea5e9 100%);">
                                        <div style="padding:30px 32px 28px 32px;color:#ffffff;">
                                          <p style="margin:0 0 8px 0;font-size:12px;letter-spacing:.14em;text-transform:uppercase;opacity:.88;">KidzGo Learning Center</p>
                                          <h1 style="margin:0;font-size:30px;line-height:1.25;font-weight:700;">Bảo lưu đã được duyệt</h1>
                                          <p style="margin:12px 0 0 0;font-size:15px;line-height:1.7;opacity:.96;">
                                            Trung tâm đã xác nhận lịch bảo lưu cho <strong>{{student_name}}</strong>. Trong khoảng thời gian này, lịch học sẽ được tạm dừng để phụ huynh dễ theo dõi.
                                          </p>
                                        </div>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:28px 32px 8px 32px;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #bfdbfe;border-radius:18px;background:#f8fbff;">
                                          <tr>
                                            <td style="padding:18px 20px;">
                                              <p style="margin:0 0 12px 0;font-size:13px;color:#2563eb;text-transform:uppercase;letter-spacing:.08em;">Thông tin bảo lưu</p>
                                              <p style="margin:0 0 8px 0;font-size:15px;"><strong>Học sinh:</strong> {{student_name}}</p>
                                              <p style="margin:0 0 8px 0;font-size:15px;"><strong>Bắt đầu:</strong> {{pause_from}}</p>
                                              <p style="margin:0;font-size:15px;"><strong>Kết thúc:</strong> {{pause_to}}</p>
                                            </td>
                                          </tr>
                                        </table>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:8px 32px 28px 32px;">
                                        <div style="padding:16px 18px;border-radius:16px;background:#eff6ff;color:#1e3a8a;">
                                          <p style="margin:0;font-size:14px;line-height:1.7;">
                                            Nếu outcome sau bảo lưu cho phép quay lại lớp cũ, hệ thống sẽ tự động mở lại lịch học khi giai đoạn bảo lưu kết thúc.
                                          </p>
                                        </div>
                                        <p style="margin:18px 0 0 0;font-size:14px;line-height:1.7;color:#475569;">
                                          Cần hỗ trợ thêm? Phụ huynh có thể liên hệ trung tâm để được tư vấn nhanh hơn.
                                        </p>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:18px 32px;background:#f8fafc;border-top:1px solid #e2e8f0;">
                                        <p style="margin:0;font-size:12px;line-height:1.7;color:#64748b;">Email này được gửi tự động từ hệ thống KidzGo.</p>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>
                            </table>
                          </div>
                          """,
                Placeholders = """["student_name","pause_from","pause_to"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Code = "PAUSE_ENROLLMENT_REJECTED_EMAIL",
                Channel = NotificationChannel.Email,
                Title = "KidzGo | Yêu cầu bảo lưu của {{student_name}} chưa được thông qua",
                Content = """
                          <div style="margin:0;padding:0;background:#fff5f5;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#3b1010;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#fff5f5;padding:24px 12px;">
                              <tr>
                                <td align="center">
                                  <table role="presentation" width="640" cellspacing="0" cellpadding="0" style="max-width:640px;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 14px 36px rgba(127,29,29,.10);">
                                    <tr>
                                      <td style="padding:0;background:linear-gradient(135deg,#b91c1c 0%,#ef4444 100%);">
                                        <div style="padding:30px 32px 28px 32px;color:#ffffff;">
                                          <p style="margin:0 0 8px 0;font-size:12px;letter-spacing:.14em;text-transform:uppercase;opacity:.88;">KidzGo Learning Center</p>
                                          <h1 style="margin:0;font-size:30px;line-height:1.25;font-weight:700;">Yêu cầu bảo lưu chưa được duyệt</h1>
                                          <p style="margin:12px 0 0 0;font-size:15px;line-height:1.7;opacity:.96;">
                                            Trung tâm đã xem xét yêu cầu bảo lưu của <strong>{{student_name}}</strong>, nhưng hiện tại chưa thể xác nhận cho khoảng thời gian đã đăng ký.
                                          </p>
                                        </div>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:28px 32px 8px 32px;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #fecaca;border-radius:18px;background:#fff7f7;">
                                          <tr>
                                            <td style="padding:18px 20px;">
                                              <p style="margin:0 0 12px 0;font-size:13px;color:#dc2626;text-transform:uppercase;letter-spacing:.08em;">Khoảng thời gian đã gửi</p>
                                              <p style="margin:0 0 8px 0;font-size:15px;"><strong>Học sinh:</strong> {{student_name}}</p>
                                              <p style="margin:0 0 8px 0;font-size:15px;"><strong>Bắt đầu:</strong> {{pause_from}}</p>
                                              <p style="margin:0;font-size:15px;"><strong>Kết thúc:</strong> {{pause_to}}</p>
                                            </td>
                                          </tr>
                                        </table>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:8px 32px 28px 32px;">
                                        <div style="padding:16px 18px;border-radius:16px;background:#fef2f2;color:#991b1b;">
                                          <p style="margin:0;font-size:14px;line-height:1.7;">
                                            Phụ huynh vui lòng liên hệ staff để được hướng dẫn phương án phù hợp hơn cho lịch học hiện tại.
                                          </p>
                                        </div>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:18px 32px;background:#fefcfc;border-top:1px solid #fee2e2;">
                                        <p style="margin:0;font-size:12px;line-height:1.7;color:#7f1d1d;">Email này được gửi tự động từ hệ thống KidzGo.</p>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>
                            </table>
                          </div>
                          """,
                Placeholders = """["student_name","pause_from","pause_to"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Code = "PAUSE_ENROLLMENT_OUTCOME_EMAIL",
                Channel = NotificationChannel.Email,
                Title = "KidzGo | Kết quả bảo lưu mới của {{student_name}}",
                Content = """
                          <div style="margin:0;padding:0;background:#f3fbf7;font-family:Segoe UI,Roboto,Arial,sans-serif;color:#113126;">
                            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:#f3fbf7;padding:24px 12px;">
                              <tr>
                                <td align="center">
                                  <table role="presentation" width="640" cellspacing="0" cellpadding="0" style="max-width:640px;background:#ffffff;border-radius:24px;overflow:hidden;box-shadow:0 14px 36px rgba(21,128,61,.12);">
                                    <tr>
                                      <td style="padding:0;background:linear-gradient(135deg,#059669 0%,#22c55e 100%);">
                                        <div style="padding:30px 32px 28px 32px;color:#ffffff;">
                                          <p style="margin:0 0 8px 0;font-size:12px;letter-spacing:.14em;text-transform:uppercase;opacity:.88;">KidzGo Learning Center</p>
                                          <h1 style="margin:0;font-size:30px;line-height:1.25;font-weight:700;">Kết quả bảo lưu đã được cập nhật</h1>
                                          <p style="margin:12px 0 0 0;font-size:15px;line-height:1.7;opacity:.96;">
                                            Trung tâm vừa cập nhật outcome xử lý bảo lưu cho <strong>{{student_name}}</strong>. Phụ huynh có thể xem nhanh phần tóm tắt dưới đây.
                                          </p>
                                        </div>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:28px 32px 10px 32px;">
                                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #bbf7d0;border-radius:18px;background:#f7fef9;">
                                          <tr>
                                            <td style="padding:18px 20px;">
                                              <p style="margin:0 0 12px 0;font-size:13px;color:#15803d;text-transform:uppercase;letter-spacing:.08em;">Outcome hiện tại</p>
                                              <p style="margin:0 0 8px 0;font-size:15px;"><strong>Học sinh:</strong> {{student_name}}</p>
                                              <p style="margin:0 0 8px 0;font-size:15px;"><strong>Kết quả:</strong> {{outcome}}</p>
                                              <p style="margin:0;font-size:15px;"><strong>Ghi chú:</strong> {{outcome_note}}</p>
                                            </td>
                                          </tr>
                                        </table>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:10px 32px 28px 32px;">
                                        <p style="margin:0;font-size:14px;line-height:1.8;color:#475569;">
                                          Nếu cần xác nhận lại kế hoạch học tiếp theo sau bảo lưu, phụ huynh vui lòng trao đổi trực tiếp với staff phụ trách.
                                        </p>
                                      </td>
                                    </tr>
                                    <tr>
                                      <td style="padding:18px 32px;background:#f8fafc;border-top:1px solid #dcfce7;">
                                        <p style="margin:0;font-size:12px;line-height:1.7;color:#64748b;">Email này được gửi tự động từ hệ thống KidzGo.</p>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>
                            </table>
                          </div>
                          """,
                Placeholders = """["student_name","outcome","outcome_note"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Code = "PAUSE_ENROLLMENT_APPROVED_PUSH",
                Channel = NotificationChannel.Push,
                Title = "Bảo lưu đã được duyệt",
                Content = "{{student_name}} sẽ tạm dừng lịch học từ {{pause_from}} đến {{pause_to}}.",
                Placeholders = """["student_name","pause_from","pause_to"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Code = "PAUSE_ENROLLMENT_REJECTED_PUSH",
                Channel = NotificationChannel.Push,
                Title = "Yêu cầu bảo lưu chưa được duyệt",
                Content = "Yêu cầu bảo lưu của {{student_name}} cho giai đoạn {{pause_from}} - {{pause_to}} đã bị từ chối.",
                Placeholders = """["student_name","pause_from","pause_to"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Code = "PAUSE_ENROLLMENT_OUTCOME_PUSH",
                Channel = NotificationChannel.Push,
                Title = "Kết quả bảo lưu mới",
                Content = "{{student_name}} có outcome bảo lưu mới: {{outcome}}.",
                Placeholders = """["student_name","outcome"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Code = "PAUSE_ENROLLMENT_APPROVED_ZALO",
                Channel = NotificationChannel.ZaloOa,
                Title = "Bảo lưu đã được xác nhận",
                Content = "Rex đã duyệt bảo lưu cho {{student_name}} từ {{pause_from}} đến {{pause_to}}.",
                Placeholders = """["student_name","pause_from","pause_to"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"),
                Code = "PAUSE_ENROLLMENT_REJECTED_ZALO",
                Channel = NotificationChannel.ZaloOa,
                Title = "Yêu cầu bảo lưu chưa được duyệt",
                Content = "Rex chưa thể duyệt yêu cầu bảo lưu của {{student_name}} từ {{pause_from}} đến {{pause_to}}.",
                Placeholders = """["student_name","pause_from","pause_to"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"),
                Code = "PAUSE_ENROLLMENT_OUTCOME_ZALO",
                Channel = NotificationChannel.ZaloOa,
                Title = "Kết quả bảo lưu mới",
                Content = "{{student_name}} có outcome bảo lưu: {{outcome}}. {{outcome_note}}",
                Placeholders = """["student_name","outcome","outcome_note"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("10101010-1010-1010-1010-101010101010"),
                Code = "PAUSE_ENROLLMENT_STAFF_REASSIGN_INAPP",
                Channel = NotificationChannel.InApp,
                Title = "Cần xếp lớp thay thế sau bảo lưu",
                Content = "Học sinh {{student_name}} có outcome ReassignEquivalentClass cho giai đoạn {{pause_from}} - {{pause_to}}. {{outcome_note}} Vui lòng kiểm tra sức chứa và enroll lại vào lớp tương đương phù hợp.",
                Placeholders = """["student_name","pause_from","pause_to","outcome_note"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });

            UpsertNotificationTemplate(migrationBuilder, new NotificationTemplate
            {
                Id = Guid.Parse("20202020-2020-2020-2020-202020202020"),
                Code = "PAUSE_ENROLLMENT_STAFF_TUTORING_INAPP",
                Channel = NotificationChannel.InApp,
                Title = "Cần tư vấn gói học kèm sau bảo lưu",
                Content = "Học sinh {{student_name}} có outcome ContinueWithTutoring cho giai đoạn {{pause_from}} - {{pause_to}}. {{outcome_note}} Vui lòng liên hệ phụ huynh để chốt gói và khóa học kèm phù hợp.",
                Placeholders = """["student_name","pause_from","pause_to","outcome_note"]""",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = seedDate,
                UpdatedAt = seedDate
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var codes = new[]
            {
                "PAUSE_ENROLLMENT_APPROVED_EMAIL",
                "PAUSE_ENROLLMENT_REJECTED_EMAIL",
                "PAUSE_ENROLLMENT_OUTCOME_EMAIL",
                "PAUSE_ENROLLMENT_APPROVED_PUSH",
                "PAUSE_ENROLLMENT_REJECTED_PUSH",
                "PAUSE_ENROLLMENT_OUTCOME_PUSH",
                "PAUSE_ENROLLMENT_APPROVED_ZALO",
                "PAUSE_ENROLLMENT_REJECTED_ZALO",
                "PAUSE_ENROLLMENT_OUTCOME_ZALO",
                "PAUSE_ENROLLMENT_STAFF_REASSIGN_INAPP",
                "PAUSE_ENROLLMENT_STAFF_TUTORING_INAPP"
            };

            var inClause = string.Join(", ", codes.Select(SqlLiteral));

            migrationBuilder.Sql($"""
                DELETE FROM public."NotificationTemplates"
                WHERE "Code" IN ({inClause});
                """);
        }

        private static void UpsertNotificationTemplate(MigrationBuilder migrationBuilder, NotificationTemplate template)
        {
            migrationBuilder.Sql(BuildNotificationTemplateUpsertSql(template));
        }

        private static string BuildNotificationTemplateUpsertSql(NotificationTemplate template)
        {
            return $"""
                INSERT INTO public."NotificationTemplates" (
                    "Id", "Code", "Channel", "Title", "Content", "Placeholders",
                    "IsActive", "IsDeleted", "CreatedAt", "UpdatedAt")
                VALUES (
                    {SqlLiteral(template.Id)},
                    {SqlLiteral(template.Code)},
                    {SqlLiteral(template.Channel.ToString())},
                    {SqlLiteral(template.Title)},
                    {SqlLiteral(template.Content)},
                    {SqlLiteral(template.Placeholders)},
                    {SqlLiteral(template.IsActive)},
                    {SqlLiteral(template.IsDeleted)},
                    {SqlLiteral(template.CreatedAt)},
                    {SqlLiteral(template.UpdatedAt)})
                ON CONFLICT ("Code") DO UPDATE
                SET
                    "Channel" = EXCLUDED."Channel",
                    "Title" = EXCLUDED."Title",
                    "Content" = EXCLUDED."Content",
                    "Placeholders" = EXCLUDED."Placeholders",
                    "IsActive" = EXCLUDED."IsActive",
                    "IsDeleted" = EXCLUDED."IsDeleted",
                    "UpdatedAt" = EXCLUDED."UpdatedAt";
                """;
        }

        private static string SqlLiteral(Guid value) => $"'{value}'";

        private static string SqlLiteral(bool value) => value ? "TRUE" : "FALSE";

        private static string SqlLiteral(DateTime value) => $"TIMESTAMPTZ '{value:O}'";

        private static string SqlLiteral(string? valueOrNull)
        {
            return valueOrNull is null
                ? "NULL"
                : $"'{valueOrNull.Replace("'", "''")}'";
        }
    }
}
