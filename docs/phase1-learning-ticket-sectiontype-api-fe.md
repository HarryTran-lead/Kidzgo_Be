# Phase 1 FE API Spec - SectionType + Learning Ticket

## 1) Mỗi role được xem dữ liệu gì

### Admin
- Xem toàn bộ session, attendance, registration, learning ticket balance/ledger.
- Tạo/sửa session, điểm danh, tạo registration, import registration, upgrade tuition plan.

### ManagementStaff
- Xem toàn bộ session, registration, learning ticket balance/ledger.
- Tạo/sửa session, tạo registration, import registration, upgrade tuition plan.
- Không có quyền mark/update attendance.

### Teacher
- Xem session (API open/authorize), xem learning ticket balance/ledger.
- Mark/update attendance.
- Không có quyền tạo/sửa registration hoặc tạo/sửa session.

### Parent, Student
- Không có quyền vào các API mới của learning ticket/session management ở Phase 1.

## 2) Phạm vi dữ liệu (own / department / all)

- `Admin`: `all`
- `ManagementStaff`: `all` (theo branch filtering do FE truyền query nếu cần; hiện chưa cưỡng chế scope ở controller)
- `Teacher`: hiện hành vi là `all` trên các endpoint được cấp role (chưa có lọc `own/department` ở controller cho learning ticket và attendance)
- `Parent/Student`: không áp dụng cho API mới trong tài liệu này

## 3) Các hành động được phép

- `Admin`: `view`, `create`, `edit`, `approve` (ngữ cảnh attendance/operation), `upgrade`
- `ManagementStaff`: `view`, `create`, `edit`, `upgrade`
- `Teacher`: `view`, `edit` (attendance), `create` (mark attendance)
- `Parent/Student`: không có action trong scope API mới

## 4) Danh sách API

## 4.1 Registration (có auto grant learning tickets)

### Endpoint + Method
- `POST /api/registrations`

### Mô tả
- Tạo registration mới.
- Sau khi tạo registration, hệ thống tự động grant `N` tickets theo `tuitionPlan.TotalSessions`.

### Params / Body
- `studentProfileId` (guid, required)
- `branchId` (guid, required)
- `programId` (guid, required)
- `tuitionPlanId` (guid, required)
- `secondaryProgramId` (guid, optional)
- `secondaryProgramSkillFocus` (string, optional)
- `expectedStartDate` (datetime, optional)
- `preferredSchedule` (string, optional)
- `note` (string, optional)

### Response success
- `201 Created` với payload `CreateRegistrationResponse` (id, program/tuition info, pricing snapshot...).
- Lưu ý: response chưa trả trực tiếp ticket balance; ticket đã được tạo ở backend.

### Response error
- `code`: ví dụ `Registration.StudentNotFound`, `Registration.BranchNotFound`, `Registration.ProgramNotFound`, `Registration.TuitionPlanNotFound`, `Registration.AlreadyExists`
- `message`: mô tả validation/business error tương ứng.

---

### Endpoint + Method
- `POST /api/registrations/import-active`

### Mô tả
- Import registration đang học dở.
- Tự grant tổng tickets theo tuition plan, sau đó mark consumed theo `usedSessions` import.

### Params / Body
- `studentProfileId` (guid, required)
- `branchId` (guid, required)
- `programId` (guid, required)
- `tuitionPlanId` (guid, required)
- `expectedStartDate` (datetime, optional)
- `actualStartDate` (datetime, required)
- `preferredSchedule` (string, optional)
- `note` (string, optional)
- `usedSessions` (int, required)
- `remainingSessions` (int, required)

### Response success
- `201 Created` với payload `ImportActiveRegistrationResponse`.

### Response error
- `Registration.ImportSessionCountMismatch` nếu `used + remaining != tuitionPlan.TotalSessions`.
- Các lỗi registration validation khác giống endpoint create.

---

### Endpoint + Method
- `POST /api/registrations/{id}/upgrade?newTuitionPlanId={guid}`

### Mô tả
- Nâng gói học cho registration active.
- Tự grant thêm tickets theo gói mới.

### Params
- Path `id` (guid, required)
- Query `newTuitionPlanId` (guid, required)

### Response success
- `200 OK` với `UpgradeTuitionPlanResponse`.

### Response error
- `Registration.NotFound`
- `Registration.NoActiveRegistrationForUpgrade`
- `Registration.TuitionPlanNotFound`
- `DifferentProgram`

## 4.2 Session (thêm SectionType)

### Endpoint + Method
- `POST /api/sessions`

### Mô tả
- Tạo session thủ công, hỗ trợ `sectionType`.

### Params / Body
- `classId` (guid, required)
- `plannedDatetime` (datetime, required)
- `durationMinutes` (int, required)
- `plannedRoomId` (guid, optional)
- `plannedTeacherId` (guid, optional)
- `plannedAssistantId` (guid, optional)
- `participationType` (string, optional, default `Main`)
- `sectionType` (string, optional, default `Normal`)

### Response success
- `201 Created`
- `CreateSessionResponse` có thêm `sectionType`.

### Response error
- `Class.NotFound`
- `Session.InvalidClassStatus`
- `Session.InvalidRoom|InvalidTeacher|InvalidAssistant`
- các lỗi conflict room/teacher/assistant.

---

### Endpoint + Method
- `PUT /api/sessions/{sessionId}`

### Mô tả
- Cập nhật session, hỗ trợ update `sectionType`.

### Params / Body
- `plannedDatetime` (datetime, required)
- `durationMinutes` (int, required)
- `plannedRoomId` (guid, optional)
- `plannedTeacherId` (guid, optional)
- `plannedAssistantId` (guid, optional)
- `participationType` (string, optional)
- `sectionType` (string, optional)

### Response success
- `200 OK`
- `UpdateSessionResponse` có thêm `sectionType`.

### Response error
- `Session.NotFound`
- `Session.InvalidStatus`
- các lỗi validate resource/conflict.

---

### Endpoint + Method
- `PUT /api/sessions/by-class`

### Mô tả
- Bulk update sessions theo class, hỗ trợ `sectionType`.

### Params / Body
- `classId` (guid, required)
- `sessionIds` (guid[], optional)
- `filterByStatus` (string, optional)
- `fromDate` (datetime, optional)
- `plannedDatetime` (datetime, optional)
- `durationMinutes` (int, optional)
- `plannedRoomId` (guid, optional)
- `plannedTeacherId` (guid, optional)
- `plannedAssistantId` (guid, optional)
- `participationType` (string, optional)
- `sectionType` (string, optional)

### Response success
- `200 OK` với `UpdateSessionsByClassResponse`.

### Response error
- trả danh sách `errors` theo từng session nếu conflict/invalid.

---

### Endpoint + Method
- `GET /api/sessions`
- `GET /api/sessions/{sessionId}`

### Mô tả
- Trả danh sách/chi tiết session; payload đã có `sectionType`.

### Response success
- `GetSessionsResponse.SessionListItemDto.sectionType`
- `GetSessionByIdResponse.SessionDetailDto.sectionType`

## 4.3 Attendance (response có ticket info)

### Endpoint + Method
- `POST /api/attendance/{sessionId}`

### Mô tả
- Mark attendance nhiều học viên cho 1 session.
- Mỗi item response có thông tin ticket consume và advance lesson.

### Params / Body
- `attendances` (array, required)
- Mỗi phần tử:
  - `studentProfileId` (guid, required)
  - `attendanceStatus` (enum: `Present|Absent|Makeup|NotMarked`, required)
  - `note` (string, optional)

### Response success
- `200 OK`, `MarkAttendanceResponse.results[]` gồm:
  - `ticketConsumed` (bool)
  - `consumedQuantity` (int)
  - `advanceLessonProgression` (bool)
  - `ticketBalance` (int?, nullable)

### Response error
- `Attendance.NotFound`
- `Attendance.FutureSessionNotAllowed`
- `Attendance.StudentNotAssigned`
- các lỗi leave/permission liên quan attendance.

---

### Endpoint + Method
- `PUT /api/attendance/{sessionId}/students/{studentProfileId}`

### Mô tả
- Cập nhật attendance của 1 học viên trong session.

### Params / Body
- `attendanceStatus` (enum, required)
- `note` (string, optional)

### Response success
- `200 OK`, `UpdateAttendanceResponse` có:
  - `ticketConsumed`, `consumedQuantity`, `advanceLessonProgression`, `ticketBalance`

### Response error
- `Attendance.NotFoundForSessionStudent`
- `Attendance.FutureSessionNotAllowed`
- `Attendance.UpdateWindowClosed`
- `Attendance.ApprovedLeaveLocked`

## 4.4 Learning Ticket APIs mới

### Endpoint + Method
- `GET /api/students/{studentProfileId}/tickets/balance`

### Mô tả
- Lấy số dư ticket học của học viên.

### Params
- Path `studentProfileId` (guid, required)

### Response success
- `200 OK`
```json
{
  "studentProfileId": "guid",
  "available": 23,
  "consumed": 1,
  "totalGranted": 24
}
```

### Response error
- chủ yếu lỗi auth/forbidden theo role.

---

### Endpoint + Method
- `GET /api/students/{studentProfileId}/tickets/ledger`

### Mô tả
- Lấy lịch sử cộng/trừ ticket.

### Params
- Path `studentProfileId` (guid, required)

### Response success
- `200 OK`
```json
{
  "items": [
    {
      "id": "guid",
      "transactionType": "Grant|Consume|Refund|Void|Adjustment",
      "quantity": 24,
      "reason": "Purchase Starters 24",
      "sessionId": "guid|null",
      "attendanceId": "guid|null",
      "createdAt": "2026-05-14T12:00:00"
    }
  ]
}
```

### Response error
- chủ yếu lỗi auth/forbidden theo role.

## 4.5 Lookup bổ sung

### Endpoint + Method
- `GET /api/lookups`

### Mô tả
- Trả thêm `lookups.sectionType` để FE bind dropdown.

### Response success
- `lookups.sectionType = [Normal, Review, Makeup, Remedial, Assessment]`

## 5) Status definition

### 5.1 SectionType
- `Normal`: buổi học chuẩn theo lesson progression.
- `Review`: buổi ôn tập.
- `Makeup`: buổi học bù.
- `Remedial`: buổi phụ đạo.
- `Assessment`: buổi đánh giá.

### 5.2 AttendanceStatus (đang dùng)
- `Present`
- `Absent`
- `Makeup`
- `NotMarked`

### 5.3 LearningTicketItemStatus
- `Available`
- `Consumed`
- `Expired`
- `Voided`

### 5.4 LearningTicketTransactionType
- `Grant`
- `Consume`
- `Refund`
- `Void`
- `Adjustment`

### 5.5 LearningTicketSource
- `Purchase`
- `FreeGrant`
- `Adjustment`
- `Import`

## 6) Luồng chuyển trạng thái

### 6.1 Learning ticket item
- `Available -> Consumed` khi attendance consume.
- `Consumed -> Available` khi rollback attendance consume.
- `Expired/Voided` chưa được kích hoạt trong flow Phase 1 hiện tại.

### 6.2 Consume rule hiện tại (MVP)
- `Present`: consume `1`.
- `Absent + NoNotice`: consume `1`.
- các trạng thái còn lại: consume `0`.
- `AdvanceLessonProgression = true` chỉ khi `SectionType = Normal` và `AttendanceStatus = Present`.

## 7) Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student |
|---|---|---|---|---|---|
| POST `/api/registrations` | Y | Y | N | N | N |
| POST `/api/registrations/import-active` | Y | Y | N | N | N |
| POST `/api/registrations/{id}/upgrade` | Y | Y | N | N | N |
| POST `/api/sessions` | Y | Y | N | N | N |
| PUT `/api/sessions/{id}` | Y | Y | N | N | N |
| PUT `/api/sessions/by-class` | Y | Y | N | N | N |
| GET `/api/sessions` | Y | Y | N | N | N |
| GET `/api/sessions/{id}` | Y | Y | Y | Y | Y |
| POST `/api/attendance/{sessionId}` | Y | N | Y | N | N |
| PUT `/api/attendance/{sessionId}/students/{studentId}` | Y | N | Y | N | N |
| GET `/api/students/{studentId}/tickets/balance` | Y | Y | Y | N | N |
| GET `/api/students/{studentId}/tickets/ledger` | Y | Y | Y | N | N |
| GET `/api/lookups` | Y | Y | Y | Y | Y |

## 8) Validation rule

### 8.1 Session + SectionType
- `sectionType` không parse được sẽ fallback `Normal` (không ném lỗi).
- validate room/teacher/assistant theo branch.
- kiểm tra conflict schedule khi create/update.

### 8.2 Registration grant ticket
- tạo registration thành công sẽ tạo ticket items + ledger grant.
- import registration: `usedSessions + remainingSessions` phải bằng `tuitionPlan.TotalSessions`.

### 8.3 Attendance consume
- consume theo policy service (MVP rule ở mục 6.2).
- update cache `Registration.UsedSessions/RemainingSessions` sau consume/refund.

## 9) Các trường hợp trả lỗi

- `401 Unauthorized`: thiếu token.
- `403 Forbidden`: role không đủ quyền.
- `404 NotFound`: session/registration/student không tồn tại.
- `400 Validation`: sai business rule (conflict lịch, class status, import mismatch, attendance update window...).
- Hiện tại nếu không còn ticket available khi consume attendance: hệ thống **không throw hard error**, trả kết quả không consume (`ticketConsumed = false`, balance không giảm).

## 10) Ghi chú tích hợp FE

- FE nên dùng `GET /api/lookups` để lấy `sectionType` thay vì hard-code.
- Màn attendance nên hiển thị thêm 4 field mới: `ticketConsumed`, `consumedQuantity`, `advanceLessonProgression`, `ticketBalance`.
- Màn student detail/package detail có thể đọc từ 2 API mới:
  - `.../tickets/balance`
  - `.../tickets/ledger`
