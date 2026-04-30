# Main Flow Statuses

Tài liệu này tổng hợp các `status` của các flow chính trong project, đối chiếu theo code domain và một phần logic handler để phản ánh flow thực tế.

## 1. Homework

Nguồn:
- `Kidzgo.Domain/Homework/HomeworkStatus.cs`

### Status list

| Status | Ý nghĩa |
|---|---|
| `Assigned` | Đã giao bài cho học sinh |
| `Submitted` | Học sinh đã nộp bài đúng hạn |
| `Graded` | Bài đã được chấm |
| `Late` | Học sinh nộp trễ hạn |
| `Missing` | Quá hạn nhưng chưa nộp |

### Flow thực tế

`Assigned -> Submitted -> Graded`

`Assigned -> Missing`

`Assigned -> Late -> Graded`

`Missing -> Late -> Graded`

Ghi chú:
- Khi học sinh submit sau deadline hoặc submit lại từ trạng thái `Missing` thì hệ thống chuyển sang `Late`.
- Teacher có thể grade từ `Submitted` hoặc re-grade khi đang là `Graded`.

## 2. Attendance

Nguồn:
- `Kidzgo.Domain/Sessions/AttendanceStatus.cs`

### Status list

| Status | Ý nghĩa |
|---|---|
| `Present` | Có mặt |
| `Absent` | Vắng mặt |
| `Makeup` | Học bù |
| `NotMarked` | Chưa điểm danh |

### Ghi chú

- Đây là attendance state của từng học sinh trong từng session.
- `NotMarked` được dùng khi chưa có bản ghi điểm danh hoặc chưa cập nhật trạng thái.

## 3. Report

Flow report trong project hiện có 3 lớp status khác nhau.

### 3.1 Report lifecycle

Nguồn:
- `Kidzgo.Domain/Reports/ReportStatus.cs`

| Status | Ý nghĩa |
|---|---|
| `Draft` | Bản nháp |
| `Review` | Đã submit để chờ duyệt |
| `Approved` | Đã được duyệt |
| `Rejected` | Bị từ chối |
| `Published` | Đã publish cho người xem cuối |

### Flow thực tế

`Draft -> Review -> Approved -> Published`

`Draft -> Review -> Rejected -> Draft`

Ghi chú:
- Cả `SessionReport` và `MonthlyReport` đang dùng chung `ReportStatus`.
- Khi update report ở trạng thái `Rejected`, code đưa về lại `Draft`.

### 3.2 Report request lifecycle

Nguồn:
- `Kidzgo.Domain/Reports/ReportRequestStatus.cs`

| Status | Ý nghĩa |
|---|---|
| `Requested` | Yêu cầu mới được tạo |
| `InProgress` | Đang xử lý |
| `Submitted` | Đã nộp kết quả xử lý |
| `Approved` | Yêu cầu đã được duyệt |
| `Rejected` | Yêu cầu bị từ chối |
| `Cancelled` | Yêu cầu bị hủy |

### 3.3 Monthly report job lifecycle

Nguồn:
- `Kidzgo.Domain/Reports/MonthlyReportJobStatus.cs`

| Status | Ý nghĩa |
|---|---|
| `Pending` | Job mới tạo, chờ chạy |
| `Generating` | Đang aggregate/generate dữ liệu |
| `Done` | Hoàn tất |
| `Failed` | Thất bại |

## 4. Gamification

Flow gamification hiện có ít nhất 2 nhóm status chính.

### 4.1 Mission progress

Nguồn:
- `Kidzgo.Domain/Gamification/MissionProgressStatus.cs`

| Status | Ý nghĩa |
|---|---|
| `Assigned` | Mission đã được gán |
| `InProgress` | Đang thực hiện |
| `Completed` | Đã hoàn thành |
| `Expired` | Hết hạn |

### Flow thực tế

`Assigned -> InProgress -> Completed`

`Assigned -> Expired`

`InProgress -> Expired`

### 4.2 Reward redemption

Nguồn:
- `Kidzgo.Domain/Gamification/RedemptionStatus.cs`

| Status | Ý nghĩa |
|---|---|
| `Requested` | Học sinh đã request đổi quà |
| `Approved` | Request đã được duyệt |
| `Delivered` | Quà đã được giao |
| `Received` | Người nhận đã xác nhận nhận quà |
| `Cancelled` | Request bị hủy |

### Flow thực tế

`Requested -> Approved -> Delivered -> Received`

`Requested -> Cancelled`

`Approved -> Cancelled`

## 5. Leave Request

Nguồn:
- `Kidzgo.Domain/Sessions/LeaveRequestStatus.cs`

### Status list

| Status | Ý nghĩa |
|---|---|
| `Pending` | Chờ duyệt |
| `Approved` | Đã duyệt |
| `Rejected` | Từ chối |
| `Cancelled` | Đã hủy |

### Flow thực tế

`Pending -> Approved`

`Pending -> Rejected`

`Pending -> Cancelled`

`Approved -> Cancelled`

### Ghi chú

- Khi tạo leave request, nếu thời gian báo trước >= 24 giờ thì hệ thống auto set `Approved`.
- Nếu báo trước < 24 giờ thì khởi tạo ở `Pending`.

## 6. Pause Enrollment Request

Nguồn:
- `Kidzgo.Domain/Classes/PauseEnrollmentRequestStatus.cs`

### Status list

| Status | Ý nghĩa |
|---|---|
| `Pending` | Chờ duyệt |
| `Approved` | Đã duyệt |
| `Rejected` | Từ chối |
| `Cancelled` | Đã hủy |

### Flow thực tế

`Pending -> Approved`

`Pending -> Rejected`

`Pending -> Cancelled`

### Ghi chú

- Khi `PauseEnrollmentRequest` được `Approved`, enrollment liên quan sẽ được chuyển sang `Paused` trong flow enrollment, nhưng đó là status của enrollment chứ không phải status của pause request.

## 7. Tóm tắt nhanh

| Flow | Status |
|---|---|
| Homework | `Assigned`, `Submitted`, `Graded`, `Late`, `Missing` |
| Attendance | `Present`, `Absent`, `Makeup`, `NotMarked` |
| Report | `Draft`, `Review`, `Approved`, `Rejected`, `Published` |
| Report Request | `Requested`, `InProgress`, `Submitted`, `Approved`, `Rejected`, `Cancelled` |
| Monthly Report Job | `Pending`, `Generating`, `Done`, `Failed` |
| Mission Progress | `Assigned`, `InProgress`, `Completed`, `Expired` |
| Reward Redemption | `Requested`, `Approved`, `Delivered`, `Received`, `Cancelled` |
| Leave Request | `Pending`, `Approved`, `Rejected`, `Cancelled` |
| Pause Enrollment Request | `Pending`, `Approved`, `Rejected`, `Cancelled` |
