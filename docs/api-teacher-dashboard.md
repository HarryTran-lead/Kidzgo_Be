# API Documentation — Teacher Dashboard

Tất cả endpoints dưới đây đều yêu cầu:

```
Authorization: Bearer <token>
```

---

## 1. GET /api/teacher/dashboard

Lấy tổng quan dashboard dành cho teacher đang đăng nhập: thống kê, danh sách lớp hôm nay, sessions sắp tới, cảnh báo (tickets), hoạt động gần đây và pending tasks.

### Authorization

```
Roles: Teacher
```

### Request

Không có query parameters hay request body.

### Response `200 OK`

```json
{
  "data": {
    "stats": {
      "totalClasses": 5,
      "totalStudents": 72,
      "upcomingSessions": 3,
      "pendingHomeworks": 0,
      "pendingReports": 2,
      "openTickets": 1
    },
    "todayClasses": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "classId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
        "classCode": "ENG101",
        "plannedDatetime": "2025-10-01T08:00:00+07:00",
        "status": "Scheduled",
        "attendanceMarked": false
      }
    ],
    "upcomingClasses": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "classId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
        "classCode": "ENG101",
        "plannedDatetime": "2025-10-01T08:00:00+07:00",
        "status": "Scheduled",
        "attendanceMarked": false
      }
    ],
    "alerts": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
        "title": "Student complaint about homework",
        "status": "Open",
        "createdAt": "2025-09-30T10:00:00Z"
      }
    ],
    "recentActivities": [
      {
        "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afd1",
        "classCode": "ENG101",
        "sessionDate": "2025-09-28T08:00:00+07:00",
        "presentCount": 18,
        "absentCount": 2
      }
    ],
    "pendingTasks": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afe1",
        "studentName": "Nguyễn Văn A",
        "classCode": "ENG101",
        "status": "Draft",
        "reportMonth": "2025-09-01T00:00:00+07:00"
      }
    ]
  }
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `stats.totalClasses` | int | Số lớp teacher đang phụ trách (main hoặc assistant) |
| `stats.totalStudents` | int | Số học viên active trong các lớp đó |
| `stats.upcomingSessions` | int | Số sessions sắp diễn ra (status = Scheduled, thời gian >= hiện tại) |
| `stats.pendingHomeworks` | int | Số homework chưa xử lý (hiện luôn = 0, chưa implement) |
| `stats.pendingReports` | int | Số báo cáo tháng ở trạng thái Draft hoặc Review |
| `stats.openTickets` | int | Số ticket chưa đóng (status ≠ Closed) |
| `todayClasses` | array | Sessions của ngày hôm nay (lọc từ `upcomingClasses`) |
| `upcomingClasses` | array | Tối đa 20 sessions scheduled sắp tới, sắp xếp theo giờ |
| `alerts` | array | Tối đa 5 tickets đang mở, mới nhất trước |
| `recentActivities` | array | Tối đa 20 bản ghi điểm danh trong khoảng ±1 tháng |
| `pendingTasks` | array | Gộp danh sách pending homeworks + pending reports |

#### `upcomingClasses` / `todayClasses` item

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | Session ID |
| `classId` | Guid | Class ID |
| `classCode` | string | Mã lớp |
| `plannedDatetime` | DateTime | Giờ học dự kiến (VN time) |
| `status` | string | `"Scheduled"` |
| `attendanceMarked` | bool | `true` nếu đã điểm danh ít nhất 1 học viên |

#### `recentActivities` item

| Field | Type | Description |
|-------|------|-------------|
| `sessionId` | Guid | Session ID |
| `classCode` | string | Mã lớp |
| `sessionDate` | DateTime | Ngày buổi học (VN time) |
| `presentCount` | int | Số học viên có mặt |
| `absentCount` | int | Số học viên vắng |

#### `pendingTasks` item (Report)

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | Report ID |
| `studentName` | string | Tên học viên |
| `classCode` | string | Mã lớp |
| `status` | string | `"Draft"` hoặc `"Review"` |
| `reportMonth` | DateTime | Tháng của báo cáo (ngày 1 của tháng, VN time) |

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden (không phải Teacher) |

---

## 2. GET /api/teacher/timetable

Lấy lịch dạy của teacher (các sessions teacher được gán là PlannedTeacher hoặc ActualTeacher).

### Authorization

```
Roles: Teacher, Admin, ManagementStaff
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `teacherUserId` | Guid | No | — | Chỉ định teacher cụ thể. Teacher role luôn xem lịch của mình, bỏ qua param này |
| `from` | DateTime | No | — | Lọc sessions có `plannedDatetime >= from` (VN time hoặc UTC) |
| `to` | DateTime | No | — | Lọc sessions có `plannedDatetime <= to` (tính đến cuối ngày VN) |
| `branchId` | Guid | No | — | Lọc theo chi nhánh |
| `classId` | Guid | No | — | Lọc theo lớp |

### Response `200 OK`

```json
{
  "sessions": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "color": "#FF5733",
      "classId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
      "classCode": "ENG101",
      "classTitle": "English for Kids - Level 1",
      "plannedDatetime": "2025-10-01T08:00:00+07:00",
      "actualDatetime": null,
      "durationMinutes": 90,
      "participationType": "Main",
      "status": "Scheduled",
      "plannedRoomId": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
      "plannedRoomName": "Room A1",
      "actualRoomId": null,
      "actualRoomName": null,
      "plannedTeacherId": "3fa85f64-5717-4562-b3fc-2c963f66afd1",
      "plannedTeacherName": "Nguyễn Văn A",
      "actualTeacherId": null,
      "actualTeacherName": null,
      "plannedAssistantId": null,
      "plannedAssistantName": null,
      "lessonPlanId": null,
      "lessonPlanLink": null,
      "attendanceStatus": null,
      "sessionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "className": "English for Kids - Level 1",
      "plannedDate": "2025-10-01",
      "startTime": "08:00:00",
      "endTime": "09:30:00",
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
      "roomName": "Room A1",
      "teacherId": "3fa85f64-5717-4562-b3fc-2c963f66afd1",
      "teacherName": "Nguyễn Văn A",
      "statusText": "Scheduled"
    }
  ]
}
```

#### Session item fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | Session ID |
| `color` | string? | Màu hiển thị trên lịch |
| `classId` | Guid | Class ID |
| `classCode` | string | Mã lớp |
| `classTitle` | string | Tên lớp |
| `plannedDatetime` | DateTime | Giờ dự kiến (VN time) |
| `actualDatetime` | DateTime? | Giờ thực tế (VN time), `null` nếu chưa có |
| `durationMinutes` | int | Thời lượng tính bằng phút |
| `participationType` | string | `Main`, `Makeup`, `ExtraPaid`, `Free`, `Trial` |
| `status` | string | `Scheduled`, `Completed`, `Cancelled` |
| `plannedRoomId` | Guid? | ID phòng dự kiến |
| `plannedRoomName` | string? | Tên phòng dự kiến |
| `actualRoomId` | Guid? | ID phòng thực tế |
| `actualRoomName` | string? | Tên phòng thực tế |
| `plannedTeacherId` | Guid? | ID giáo viên dự kiến |
| `plannedTeacherName` | string? | Tên giáo viên dự kiến |
| `actualTeacherId` | Guid? | ID giáo viên thực tế |
| `actualTeacherName` | string? | Tên giáo viên thực tế |
| `plannedAssistantId` | Guid? | ID trợ giảng dự kiến |
| `plannedAssistantName` | string? | Tên trợ giảng dự kiến |
| `lessonPlanId` | Guid? | ID giáo án |
| `lessonPlanLink` | string? | Link `/api/lesson-plans/{id}` |
| `attendanceStatus` | string? | Trạng thái điểm danh |
| `plannedDate` | DateOnly | Ngày học (VN) — computed |
| `startTime` | TimeOnly | Giờ bắt đầu (VN) — computed |
| `endTime` | TimeOnly | Giờ kết thúc (VN) — computed |
| `roomId` | Guid? | `actualRoomId ?? plannedRoomId` — computed |
| `roomName` | string? | `actualRoomName ?? plannedRoomName` — computed |
| `teacherId` | Guid? | `actualTeacherId ?? plannedTeacherId` — computed |
| `teacherName` | string? | `actualTeacherName ?? plannedTeacherName` — computed |

### Behavior

- Sessions bị `Cancelled` không được trả về.
- Teacher role chỉ xem được lịch của chính mình (param `teacherUserId` bị bỏ qua).
- Admin/ManagementStaff có thể truy vấn lịch của bất kỳ teacher nào qua `teacherUserId`.

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden |

---

## 3. GET /api/teacher/classes

Lấy danh sách lớp mà teacher đang phụ trách (main hoặc assistant), kèm lịch học tuần.

### Authorization

```
Roles: Teacher
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `pageNumber` | int | No | 1 | Trang hiện tại |
| `pageSize` | int | No | 10 | Số lớp mỗi trang |
| `teachingDate` | DateOnly | No | — | Chỉ trả về lớp có session vào ngày đó (`YYYY-MM-DD`) |

### Response `200 OK`

```json
{
  "classes": {
    "items": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
        "branchName": "Kidzgo Quận 1",
        "programId": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
        "programName": "English Program",
        "code": "ENG101",
        "title": "English for Kids - Level 1",
        "mainTeacherId": "3fa85f64-5717-4562-b3fc-2c963f66afd1",
        "mainTeacherName": "Nguyễn Văn A",
        "assistantTeacherId": null,
        "assistantTeacherName": null,
        "startDate": "2025-09-01",
        "endDate": "2025-12-31",
        "status": "Active",
        "capacity": 20,
        "currentEnrollmentCount": 15,
        "weeklyScheduleSlots": [
          {
            "dayOfWeek": "Monday",
            "startTime": "08:00",
            "endTime": "09:30"
          }
        ],
        "role": "MainTeacher"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 5,
    "totalPages": 1
  }
}
```

#### Class item fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | Guid | Class ID |
| `branchId` | Guid | Chi nhánh |
| `branchName` | string | Tên chi nhánh |
| `programId` | Guid | Chương trình học |
| `programName` | string | Tên chương trình |
| `code` | string | Mã lớp |
| `title` | string | Tên lớp |
| `mainTeacherId` | Guid? | ID giáo viên chính |
| `mainTeacherName` | string? | Tên giáo viên chính |
| `assistantTeacherId` | Guid? | ID trợ giảng |
| `assistantTeacherName` | string? | Tên trợ giảng |
| `startDate` | DateOnly | Ngày bắt đầu lớp |
| `endDate` | DateOnly? | Ngày kết thúc lớp |
| `status` | string | `Planned`, `Recruiting`, `Active`, `Completed`, `Cancelled` |
| `capacity` | int | Sĩ số tối đa |
| `currentEnrollmentCount` | int | Số học viên active hiện tại |
| `weeklyScheduleSlots` | array | Lịch học tuần (day + giờ bắt đầu/kết thúc) |
| `role` | string | `"MainTeacher"` hoặc `"AssistantTeacher"` |

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden |

---

## 4. GET /api/teacher/classes/{classId}/students

Lấy danh sách học viên active của một lớp cụ thể mà teacher phụ trách.

### Authorization

```
Roles: Teacher
```

### URL Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `classId` | Guid | Yes | ID của lớp |

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `searchTerm` | string | No | — | Tìm theo tên học viên |
| `pageNumber` | int | No | 1 | Trang hiện tại |
| `pageSize` | int | No | 10 | Số học viên mỗi trang |

### Response `200 OK`

```json
{
  "students": {
    "items": [
      {
        "enrollmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
        "studentUserId": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
        "studentName": "Trần Thị B",
        "studentEmail": "student.b@email.com",
        "enrollDate": "2025-09-01",
        "status": "Active"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15,
    "totalPages": 2
  }
}
```

### Error Responses

| HTTP | Code | Description |
|------|------|-------------|
| 401 | — | Unauthorized |
| 403 | — | Forbidden |
| 404 | `Class.NotFound` | Lớp không tồn tại hoặc teacher không phụ trách lớp này |

---

## 5. GET /api/teacher/students

Lấy danh sách tất cả học viên active từ tất cả các lớp teacher phụ trách (có thể lọc theo lớp).

### Authorization

```
Roles: Teacher
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `classId` | Guid | No | — | Lọc theo lớp cụ thể |
| `searchTerm` | string | No | — | Tìm theo tên/email học viên |
| `pageNumber` | int | No | 1 | Trang hiện tại |
| `pageSize` | int | No | 10 | Số học viên mỗi trang |

### Response `200 OK`

```json
{
  "students": {
    "items": [
      {
        "studentProfileId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
        "studentUserId": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
        "studentName": "Trần Thị B",
        "studentEmail": "student.b@email.com"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 72,
    "totalPages": 8
  }
}
```

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden |

---

## 6. GET /api/teacher/profile

Lấy thông tin profile của teacher đang đăng nhập kèm thống kê dạy học.

### Authorization

```
Roles: Teacher
```

### Request

Không có query parameters hay request body.

### Response `200 OK`

```json
{
  "data": {
    "basicInfo": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afd1",
      "fullName": "Nguyễn Văn A",
      "email": "teacher.a@kidzgo.vn",
      "phoneNumber": "0901234567",
      "avatarUrl": null,
      "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
      "branchName": "Kidzgo Quận 1"
    },
    "bio": null,
    "skills": [],
    "certificates": [],
    "teachingStats": {
      "totalClasses": 5,
      "totalStudents": 72,
      "upcomingSessions": 3,
      "pendingHomeworks": 0,
      "pendingReports": 2,
      "openTickets": 1
    }
  }
}
```

> **Lưu ý:** `bio`, `skills`, `certificates` chưa được implement, luôn trả về `null` / `[]`.

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden |

---

## 7. PUT /api/teacher/profile

Cập nhật thông tin cơ bản của teacher đang đăng nhập.

### Authorization

```
Roles: Teacher
```

### Request Body

```json
{
  "fullName": "Nguyễn Văn A",
  "email": "teacher.a@kidzgo.vn",
  "phoneNumber": "0901234567",
  "avatarUrl": "https://cdn.kidzgo.vn/avatars/abc.png",
  "profiles": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afe1",
      "displayName": "Bé A"
    }
  ]
}
```

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `fullName` | string? | No | Tên đầy đủ |
| `email` | string? | No | Email đăng nhập |
| `phoneNumber` | string? | No | Số điện thoại |
| `avatarUrl` | string? | No | URL ảnh đại diện |
| `profiles` | array? | No | Cập nhật display name cho các student profile liên kết |

### Response `200 OK`

Trả về thông tin user sau khi cập nhật (cùng format với response của các user queries hiện có).

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden |

---

## 8. GET /api/teacher/timesheet

Lấy bảng tổng hợp giờ dạy và thu nhập theo tháng của teacher.

### Authorization

```
Roles: Teacher, Admin, ManagementStaff
```

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `teacherUserId` | Guid | No | — | Chỉ Admin/ManagementStaff mới dùng được. Teacher luôn xem của mình |
| `year` | int | No | — | Lọc theo năm (ví dụ: `2025`). Không truyền = tất cả năm |

### Response `200 OK`

```json
{
  "data": {
    "teacherCompensationType": "VietnameseTeacher",
    "standardSessionDurationMinutes": 90,
    "defaultRates": {
      "foreignTeacher": 500000.00,
      "vietnameseTeacher": 200000.00,
      "assistant": 100000.00
    },
    "monthlyData": [
      {
        "month": "2025-09",
        "hours": 36.00,
        "income": 4800000.00,
        "rate": 200000.00,
        "classCount": 24,
        "status": "Open"
      },
      {
        "month": "2025-10",
        "hours": 18.00,
        "income": 2400000.00,
        "rate": 200000.00,
        "classCount": 12,
        "status": "Locked"
      }
    ],
    "yearlySummary": {
      "totalHours": 54.00,
      "totalIncome": 7200000.00,
      "averagePerMonth": 3600000.00,
      "totalClasses": 36
    }
  }
}
```

#### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `teacherCompensationType` | string | `ForeignTeacher`, `VietnameseTeacher`, hoặc `Assistant` |
| `standardSessionDurationMinutes` | int | Thời lượng session chuẩn để tính thu nhập (mặc định: 90) |
| `defaultRates.foreignTeacher` | decimal | Thù lao mỗi session tiêu chuẩn cho foreign teacher |
| `defaultRates.vietnameseTeacher` | decimal | Thù lao mỗi session tiêu chuẩn cho Vietnamese teacher |
| `defaultRates.assistant` | decimal | Thù lao mỗi session tiêu chuẩn cho trợ giảng |
| `monthlyData[].month` | string | Tháng định dạng `"YYYY-MM"` |
| `monthlyData[].hours` | decimal | Tổng giờ dạy trong tháng |
| `monthlyData[].income` | decimal | Thu nhập tính được trong tháng |
| `monthlyData[].rate` | decimal | Thù lao trung bình mỗi session trong tháng |
| `monthlyData[].classCount` | int | Số sessions đã dạy trong tháng |
| `monthlyData[].status` | string | `"Locked"` (đã chốt) hoặc `"Open"` (chưa chốt) |
| `yearlySummary.totalHours` | decimal | Tổng giờ cả năm |
| `yearlySummary.totalIncome` | decimal | Tổng thu nhập cả năm |
| `yearlySummary.averagePerMonth` | decimal | Thu nhập trung bình mỗi tháng |
| `yearlySummary.totalClasses` | int | Tổng số sessions cả năm |

#### Thu nhập được tính như thế nào

1. Tra session role override nếu có (cấu hình riêng cho từng session).
2. Nếu không, dùng `defaultRates` theo `teacherCompensationType`.
3. Nếu không có rate mặc định, fallback sang `contract.hourlyRate × standardSessionDurationMinutes / 60`.
4. Thu nhập được prorated theo thời lượng thực tế: `rate × actualDurationMinutes / standardSessionDurationMinutes`.
5. Cộng thêm `allowance` nếu session role override có thiết lập.

### Error Responses

| HTTP | Description |
|------|-------------|
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Teacher not found |
