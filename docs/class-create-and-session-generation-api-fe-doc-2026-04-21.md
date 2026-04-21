# API FE Doc - Tạo Class và Generate Sessions From Pattern - 2026-04-21

Tài liệu này tập trung vào 2 API:

1. `POST /api/classes`
2. `POST /api/sessions/generate-from-pattern`

Đồng thời có thêm 1 mục tổng hợp các API hiện có để đổi giáo viên/phòng cho các session đã tạo.

## 1. Response format chung

### Success

`POST /api/classes` trả về `201 Created`.

`POST /api/sessions/generate-from-pattern` trả về `200 OK`.

Body thành công theo format:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error

Body lỗi theo `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Class.CodeExists",
  "status": 409,
  "detail": "Class code already exists"
}
```

Nếu lỗi validation từ `FluentValidation`, response có thêm `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Class code is required",
      "type": 2
    }
  ]
}
```

## 2. API Create Class

### Endpoint

- Method: `POST`
- URL: `/api/classes`
- Roles: `Admin`, `ManagementStaff`

### Mục đích

Tạo metadata của class: branch, program, teacher, room, sức chứa, thời gian học, schedule pattern.

Lưu ý:

- API này chỉ tạo `class`, không tự động sinh session.
- Muốn sinh session thì FE gọi thêm `POST /api/sessions/generate-from-pattern`.
- Hiện tại backend **không validate cú pháp RRULE ngay tại bước create class**. Nếu `schedulePattern` sai cú pháp, class vẫn có thể được tạo và sẽ lỗi ở bước generate session sau.

### Request payload

```json
{
  "branchId": "11111111-1111-1111-1111-111111111111",
  "programId": "22222222-2222-2222-2222-222222222222",
  "code": "KID-A1-01",
  "title": "Kids A1 Mon Wed Fri 18h",
  "name": "Kids A1 Mon Wed Fri 18h",
  "roomId": "33333333-3333-3333-3333-333333333333",
  "mainTeacherId": "44444444-4444-4444-4444-444444444444",
  "assistantTeacherId": "55555555-5555-5555-5555-555555555555",
  "startDate": "2026-05-04",
  "endDate": "2026-07-31",
  "capacity": 16,
  "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0;DURATION=90",
  "description": "Lớp tối cho trẻ em"
}
```

### Field mô tả

| Field | Type | Required | Ghi chú |
| --- | --- | --- | --- |
| `branchId` | `guid` | Yes | Branch phải tồn tại và active. |
| `programId` | `guid` | Yes | Program phải tồn tại, không bị xóa, đang active. |
| `code` | `string` | Yes | Unique, max 50 ký tự. |
| `title` | `string?` | Yes, nếu `name` rỗng | Controller ưu tiên `name`, nếu không có thì dùng `title`, nếu cả 2 rỗng thì fallback `code`. Tuy nhiên validator vẫn bắt buộc `Title` sau khi map command. |
| `name` | `string?` | Yes, nếu `title` rỗng | Alias cho FE. |
| `roomId` | `guid?` | No | Phòng mặc định của class. |
| `mainTeacherId` | `guid?` | No | Giáo viên chính mặc định của class. |
| `assistantTeacherId` | `guid?` | No | Giáo viên phụ mặc định của class. |
| `startDate` | `date` | Yes | Không được nhỏ hơn ngày hiện tại theo giờ Việt Nam. |
| `endDate` | `date?` | No | Bắt buộc nếu có `schedulePattern`. Phải `>= startDate`. |
| `capacity` | `int` | Yes | Phải `> 0`. |
| `schedulePattern` | `string?` | No | RRULE lưu cho việc sinh session sau này. |
| `description` | `string?` | No | Mô tả thêm. |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "66666666-6666-6666-6666-666666666666",
    "branchId": "11111111-1111-1111-1111-111111111111",
    "programId": "22222222-2222-2222-2222-222222222222",
    "code": "KID-A1-01",
    "title": "Kids A1 Mon Wed Fri 18h",
    "roomId": "33333333-3333-3333-3333-333333333333",
    "mainTeacherId": "44444444-4444-4444-4444-444444444444",
    "assistantTeacherId": "55555555-5555-5555-5555-555555555555",
    "startDate": "2026-05-04",
    "endDate": "2026-07-31",
    "status": "Planned",
    "capacity": 16,
    "schedulePattern": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0;DURATION=90",
    "description": "Lớp tối cho trẻ em",
    "name": "Kids A1 Mon Wed Fri 18h",
    "scheduleText": "RRULE:FREQ=WEEKLY;BYDAY=MO,WE,FR;BYHOUR=18;BYMINUTE=0;DURATION=90"
  }
}
```

### Error messages FE có thể gặp

| HTTP | Title | Detail |
| --- | --- | --- |
| `400` | `Validation.General` | Lỗi validate field đầu vào. |
| `404` | `Class.BranchNotFound` | `Branch not found or inactive` |
| `404` | `Class.ProgramNotFound` | `Program not found, deleted, or inactive` |
| `409` | `Class.CodeExists` | `Class code already exists` |
| `404` | `Class.MainTeacherNotFound` | `Main teacher not found or is not a teacher` |
| `409` | `Class.MainTeacherBranchMismatch` | `Main teacher must belong to the same branch as the class` |
| `404` | `Class.AssistantTeacherNotFound` | `Assistant teacher not found or is not a teacher` |
| `409` | `Class.AssistantTeacherBranchMismatch` | `Assistant teacher must belong to the same branch as the class` |
| `409` | `Class.TeacherConflict` | `Teacher is already assigned to class '...' at dd/MM/yyyy HH:mm ...` |
| `409` | `Class.AssistantConflict` | `Assistant teacher is already assigned to class '...' at dd/MM/yyyy HH:mm` |

### Validation rules đang active

| Rule | Message |
| --- | --- |
| `branchId` bắt buộc | `Branch ID is required` |
| `programId` bắt buộc | `Program ID is required` |
| `code` bắt buộc | `Class code is required` |
| `code` tối đa 50 ký tự | `Class code must not exceed 50 characters` |
| `title` bắt buộc sau khi map | `Class title is required` |
| `title` tối đa 255 ký tự | `Class title must not exceed 255 characters` |
| `startDate` bắt buộc | `Start date is required` |
| `startDate` không ở quá khứ | `Start date cannot be in the past` |
| `endDate` bắt buộc nếu có schedule pattern | `End date is required when schedule pattern is provided` |
| `endDate >= startDate` | `End date must be greater than or equal to start date` |
| `endDate` không ở quá khứ | `End date cannot be in the past` |
| `capacity > 0` | `Capacity must be greater than 0` |

## 3. API Generate Sessions From Pattern

### Endpoint

- Method: `POST`
- URL: `/api/sessions/generate-from-pattern`
- Roles: `Admin`, `ManagementStaff`

### Mục đích

Sinh session từ `schedulePattern` đã lưu trong class.

### Request payload

```json
{
  "classId": "66666666-6666-6666-6666-666666666666",
  "onlyFutureSessions": true
}
```

### Field mô tả

| Field | Type | Required | Default | Ghi chú |
| --- | --- | --- | --- | --- |
| `classId` | `guid` | Yes | - | Class cần generate sessions. |
| `onlyFutureSessions` | `bool` | No | `true` | `true`: chỉ sinh từ thời điểm hiện tại trở đi. `false`: sinh từ `startDate` của class. Session trùng datetime đã tồn tại sẽ bị bỏ qua, không bị tạo lại. |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "createdSessionsCount": 12
  }
}
```

### Error messages FE có thể gặp

| HTTP | Title | Detail |
| --- | --- | --- |
| `404` | `Class.NotFound` | `Class with Id = '...' was not found` |
| `400` | `Session.InvalidClassStatus` | `Sessions can only be created for Planned or Active classes` |
| `400` | `Session.MissingClassEndDate` | `Class '...' must have an end date before generating sessions from schedule pattern` |
| `400` | `Session.MissingSchedulePattern` | `Class '...' does not have a schedule pattern` |
| `400` | `SchedulePattern.Empty` | `Schedule pattern cannot be empty` |
| `400` | `SchedulePattern.Invalid` | `Invalid RRULE pattern: ...` |
| `400` | `Session.InvalidDuration` | `Duration phải lớn hơn 0. Giá trị hiện tại: ...` |
| `400` | `Session.InvalidBranch` | `Branch với ID ... không tồn tại hoặc không active` |
| `400` | `Session.InvalidRoom` | `Room với ID ... không tồn tại hoặc không thuộc branch này` |
| `400` | `Session.InvalidTeacher` | `Main Teacher với ID ... không tồn tại, không phải Teacher role, hoặc không thuộc branch này` |
| `400` | `Session.InvalidAssistant` | `Assistant Teacher với ID ... không tồn tại, không phải Teacher role, hoặc không thuộc branch này` |
| `409` | `Class.RoomConflict` | `Room is already booked by class '...' at dd/MM/yyyy HH:mm` |
| `409` | `Class.TeacherConflict` | `Teacher is already assigned to class '...' at dd/MM/yyyy HH:mm ...` |
| `409` | `Class.AssistantConflict` | `Assistant teacher is already assigned to class '...' at dd/MM/yyyy HH:mm` |
| `400` | `Session.SaveFailed` | Lỗi khi save sessions vào DB. |

### Hành vi quan trọng cho FE

- Nếu class đã có session trùng `plannedDatetime`, backend sẽ `skip`, không báo lỗi.
- Nếu `onlyFutureSessions = true` và tất cả occurrence nằm trong quá khứ, response vẫn thành công với `createdSessionsCount = 0`.
- Session được sinh sẽ lấy mặc định:
  - `plannedRoomId = class.roomId`
  - `plannedTeacherId = class.mainTeacherId`
  - `plannedAssistantId = class.assistantTeacherId`
  - `status = Scheduled`
  - `participationType = Main`

## 4. API hiện có cho việc đổi giáo viên/phòng trên các session đã tạo

### Đã có API

#### A. Đổi giáo viên chính, giáo viên phụ, phòng cho 1 session

- Method: `PUT`
- URL: `/api/sessions/{sessionId}`
- Roles: `Admin`, `ManagementStaff`

Request body:

```json
{
  "plannedDatetime": "2026-05-05T18:00:00+07:00",
  "durationMinutes": 90,
  "plannedRoomId": "33333333-3333-3333-3333-333333333333",
  "plannedTeacherId": "44444444-4444-4444-4444-444444444444",
  "plannedAssistantId": "55555555-5555-5555-5555-555555555555",
  "participationType": "Main"
}
```

Ghi chú:

- API này sửa được `plannedTeacherId`, `plannedAssistantId`, `plannedRoomId`.
- API này **không sửa `classId`** của session.
- API này block khi session đang `Cancelled` hoặc `Completed`.
- API này có check conflict nhưng theo code hiện tại conflict chỉ để log, **không chặn update**.
- Vì là `PUT`, FE phải gửi đầy đủ các field bắt buộc như `plannedDatetime`, `durationMinutes`.

#### B. Đổi giáo viên chính, giáo viên phụ, phòng cho nhiều session trong 1 class

- Method: `PUT`
- URL: `/api/sessions/by-class`
- Roles: `Admin`, `ManagementStaff`

Request body:

```json
{
  "classId": "66666666-6666-6666-6666-666666666666",
  "sessionIds": [
    "77777777-7777-7777-7777-777777777777",
    "88888888-8888-8888-8888-888888888888"
  ],
  "fromDate": "2026-05-01T00:00:00+07:00",
  "plannedRoomId": "33333333-3333-3333-3333-333333333333",
  "plannedTeacherId": "44444444-4444-4444-4444-444444444444",
  "plannedAssistantId": "55555555-5555-5555-5555-555555555555"
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "updatedSessionsCount": 2,
    "updatedSessionIds": [
      "77777777-7777-7777-7777-777777777777",
      "88888888-8888-8888-8888-888888888888"
    ],
    "skippedSessionIds": [],
    "errors": []
  }
}
```

Ghi chú:

- API này sửa được `plannedTeacherId`, `plannedAssistantId`, `plannedRoomId` cho nhiều session.
- API này **không sửa `classId`** của session.
- API này chỉ update session không phải `Cancelled` và `Completed`.
- API này có thể filter theo `sessionIds`, `filterByStatus`, `fromDate`.
- Theo code hiện tại, field nullable trong bulk update chỉ có nghĩa "có update hay không". Nghĩa là FE **không clear về `null`** được `plannedTeacherId`, `plannedAssistantId`, `plannedRoomId` bằng API này.

#### C. Đổi teacher mặc định ở cấp class

- Method: `PATCH`
- URL: `/api/classes/{id}/assign-teacher`
- Roles: `Admin`, `ManagementStaff`

Request body:

```json
{
  "mainTeacherId": "44444444-4444-4444-4444-444444444444",
  "assistantTeacherId": "55555555-5555-5555-5555-555555555555"
}
```

Ghi chú quan trọng:

- API này chỉ đổi `MainTeacherId` và `AssistantTeacherId` của bảng `Classes`.
- API này **không propagate** teacher mới xuống các session đã tạo trước đó.
- Nếu FE muốn đổi teacher trên các session đã sinh, phải gọi `PUT /api/sessions/{sessionId}` hoặc `PUT /api/sessions/by-class`.

### Chưa thấy API

- Chưa thấy API nào để đổi `classId` của một session sang class khác.
- Chưa thấy API dedicated nào để "migrate" toàn bộ session đã tạo từ class A sang class B.

## 5. FE flow để xài an toàn

### Flow tạo class và sinh session

1. Gọi `POST /api/classes`.
2. Nếu user muốn sinh session ngay, gọi tiếp `POST /api/sessions/generate-from-pattern`.
3. Nếu cần đổi teacher/phòng cho các session đã sinh:
   - 1 session: `PUT /api/sessions/{sessionId}`
   - nhiều session cùng 1 class: `PUT /api/sessions/by-class`

### Recommendation cho FE

- Sau khi `create class`, nếu có `schedulePattern` thì nên hiện nút hoặc flow `Generate sessions`.
- Không nên assume đổi teacher ở class sẽ tự đổi teacher ở session.
- Nếu business muốn chuyển 1 session sang class khác, backend hiện tại chưa có API sẵn cho việc đó.
