# API Documentation

## 1. GET /api/sessions/availability

Trả về danh sách **teachers** và **rooms** kèm trạng thái `isAvailable` dựa trên xung đột với các class sessions và placement tests trong cùng khoảng thời gian.

### Authorization

```
Bearer <token>
Roles: Admin, ManagementStaff
```

### Query Parameters

| Parameter          | Type     | Required | Default | Description                                           |
|--------------------|----------|----------|---------|-------------------------------------------------------|
| `scheduledAt`      | DateTime | Yes      | —       | Thời điểm bắt đầu session (ISO 8601, VN time hoặc UTC) |
| `durationMinutes`  | int      | No       | 60      | Thời lượng tính bằng phút                            |
| `branchId`         | Guid     | No       | —       | Lọc teachers và rooms theo chi nhánh                 |
| `excludeSessionId` | Guid     | No       | —       | Loại trừ session này khỏi conflict check (dùng khi edit session) |
| `includeUnavailable` | bool   | No       | false   | `true` = trả về cả teachers/rooms không khả dụng kèm danh sách conflicts |

### Response `200 OK`

```json
{
  "scheduledAt": "2025-10-01T08:00:00Z",
  "endAt": "2025-10-01T09:30:00Z",
  "durationMinutes": 90,
  "teachers": [
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "name": "Nguyễn Văn A",
      "email": "teacher.a@kidzgo.vn",
      "role": "Teacher",
      "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "isAvailable": true,
      "conflicts": []
    },
    {
      "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa8",
      "name": "Trần Thị B",
      "email": "teacher.b@kidzgo.vn",
      "role": "Teacher",
      "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "isAvailable": false,
      "conflicts": [
        {
          "type": "ClassSession",
          "referenceId": "3fa85f64-5717-4562-b3fc-2c963f66afb1",
          "title": "ENG101 - English for Kids",
          "startAt": "2025-10-01T08:30:00Z",
          "endAt": "2025-10-01T10:00:00Z"
        }
      ]
    }
  ],
  "rooms": [
    {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afc1",
      "name": "Room A1",
      "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "capacity": 20,
      "isAvailable": true,
      "conflicts": []
    },
    {
      "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afc2",
      "name": "Room B2",
      "branchId": "3fa85f64-5717-4562-b3fc-2c963f66afa7",
      "capacity": 15,
      "isAvailable": false,
      "conflicts": [
        {
          "type": "PlacementTest",
          "referenceId": "3fa85f64-5717-4562-b3fc-2c963f66afd1",
          "title": "Placement test",
          "startAt": "2025-10-01T08:00:00Z",
          "endAt": "2025-10-01T09:00:00Z"
        }
      ]
    }
  ]
}
```

#### Conflict `type` values

| Value          | Ý nghĩa                                      |
|----------------|----------------------------------------------|
| `ClassSession` | Xung đột với một class session đang scheduled |
| `PlacementTest`| Xung đột với một placement test đang scheduled |

### Behavior

- **Mặc định** (`includeUnavailable=false`): chỉ trả về teachers/rooms có `isAvailable = true`, trường `conflicts` luôn là `[]`.
- **`includeUnavailable=true`**: trả về tất cả teachers/rooms. `conflicts` chứa danh sách xung đột đầy đủ.
- Teachers được lọc theo `UserRole = Teacher`.
- Rooms được lọc theo `IsActive = true`.
- Conflict check dùng thời gian `ActualDatetime ?? PlannedDatetime` của session.

### Error Responses

| HTTP | Code | Message |
|------|------|---------|
| 400  | `Session.ScheduledAtRequired` | ScheduledAt is required |
| 400  | `Session.InvalidDuration` | DurationMinutes must be greater than 0 |
| 401  | — | Unauthorized |
| 403  | — | Forbidden (wrong role) |

---

## 2. GET /api/pause-enrollment-requests/settings

Lấy cài đặt hệ thống cho tính năng tạm dừng học phí (pause enrollment).

### Authorization

```
Bearer <token>
Roles: Admin, ManagementStaff
```

### Request

Không có query parameters hay request body.

### Response `200 OK`

```json
{
  "reservationLimitMonths": 3,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-09-15T08:30:00Z"
}
```

#### Response Fields

| Field                   | Type      | Description                                                                 |
|-------------------------|-----------|-----------------------------------------------------------------------------|
| `reservationLimitMonths`| int       | Số tháng tối đa học viên được phép tạm hoãn enrollment. Mặc định: **3**   |
| `createdAt`             | DateTime  | Thời điểm cài đặt được tạo lần đầu (UTC)                                  |
| `updatedAt`             | DateTime? | Thời điểm cập nhật gần nhất (UTC), `null` nếu chưa từng cập nhật           |

### Behavior

- Nếu chưa có settings trong DB, hệ thống tự tạo mới với `reservationLimitMonths = 3` (default).

### Error Responses

| HTTP | Description |
|------|-------------|
| 401  | Unauthorized |
| 403  | Forbidden (wrong role) |

---

## 3. PUT /api/pause-enrollment-requests/settings

Cập nhật cài đặt hệ thống cho tính năng tạm dừng học phí.

### Authorization

```
Bearer <token>
Roles: Admin, ManagementStaff
```

### Request Body

```json
{
  "reservationLimitMonths": 6
}
```

| Field                   | Type | Required | Description                                      |
|-------------------------|------|----------|--------------------------------------------------|
| `reservationLimitMonths`| int  | Yes      | Số tháng tối đa cho phép tạm hoãn. Phải > 0    |

### Response `200 OK`

Trả về settings sau khi cập nhật, cùng format với GET:

```json
{
  "reservationLimitMonths": 6,
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-10-01T10:00:00Z"
}
```

### Error Responses

| HTTP | Code | Message |
|------|------|---------|
| 400  | `PauseEnrollmentSettings.InvalidReservationLimitMonths` | Reservation limit months must be greater than 0 |
| 401  | — | Unauthorized |
| 403  | — | Forbidden (wrong role) |
