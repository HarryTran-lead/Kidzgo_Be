# Các quyết định đã chốt

Tài liệu này ghi lại những rule nghiệp vụ đã chốt để tiếp tục implement mà không phải hỏi lại.

Chi tiết API FE đã được tách sang: [fe-api-spec.md](./fe-api-spec.md)

| Hạng mục | Quyết định |
|---|---|
| TuitionPlan | Gói là package độc lập, không còn gắn syllabus/module. |
| Attendance - absent | `absent` sẽ trừ buổi. Báo trước >=24h thì không trừ ngay; trong vòng 24h thì chờ staff duyệt. |
| Attendance - leave approved | Vẫn trừ gói chính, sau đó chuyển thành `makeup credit`. Phụ huynh có thể chọn ngày bù trong hệ thống. |
| Makeup credit | Nếu chưa chọn buổi bù thì credit được giữ lại. Không auto hoàn tiền buổi bù. |
| Makeup class | Phụ huynh chọn bù vào lớp cùng trình độ. Không cần tạo lớp riêng chỉ để bù, trừ khi có yêu cầu vận hành đặc biệt. |
| Waitlist | Đủ 7 học viên thì mở lớp. Admin và ManagementStaff nhận notification và có flow mở lớp từ waitlist. |
| Branch / teacher | Backend enforce cứng, FE chỉ filter là chưa đủ. |
| Hết 24 buổi | Không tự động lên level. Muốn lên level thì làm placement test / assessment. |
| Curriculum vận hành | Trung tâm không dạy theo curriculum cứng; admin set template trình tự buổi, teacher note nội dung thực tế theo template đó. |
| Legacy module | `TicketTypeCompatibility` / `SlotTypes` / `LearningTicketTypes` đã bỏ khỏi runtime contract. |
| `24+4` | Chỉ là ví dụ nghiệp vụ. Hiểu là mua/upgrade gói mới + rollover makeup credit/bonus, không phải field API riêng. |

## Ý nghĩa cho implement

- `TuitionPlan` là package bán ra, không dùng để suy ra tiến độ curriculum.
- `Attendance` là nơi quyết định trừ buổi, tạo makeup credit hoặc chờ duyệt.
- `MakeupCredit` là cơ chế lưu quyền bù, không phải refund.
- `Class` và `Session` chỉ cần hỗ trợ lịch học và template trình tự dạy.
- `Promotion` là flow riêng, không tự chạy khi hết package.

