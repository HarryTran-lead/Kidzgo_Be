# Attendance API Usage (FE)

## Base
- Base path: `/api/attendance`
- Auth: Bearer token
- Success wrapper:
```json
{
  "isSuccess": true,
  "data": {}
}
```

## Role Matrix

| API | Admin | Teacher | Parent | Student |
|---|---|---|---|---|
| `POST /api/attendance/{sessionId}` (mark) | Yes | Yes | No | No |
| `PUT /api/attendance/{sessionId}/students/{studentProfileId}` (update) | Yes | Yes | No | No |
| `GET /api/attendance/{sessionId}` (session attendance list) | Yes | Yes | No | No |
| `GET /api/attendance/students` (my history) | Yes* | Yes* | Yes* | Yes* |

\* `GET /students` đọc `studentId` từ token context của user hiện tại.

## Business Time Rules (current code)
- `Admin`: không bị chặn theo ngày.
- `Teacher`:
  - Không được mark/update cho session tương lai.
  - Không được mark/update nếu đã sang ngày hôm sau.
  - Chỉ thao tác được khi `sessionDate == today` (mốc theo ngày, không theo giờ bắt đầu/kết thúc session).

## Enums

### AttendanceStatus
- `Present`
- `Absent`
- `Makeup`
- `NotMarked`

### AbsenceType (response)
- `WithNotice24H`
- `Under24H`
- `NoNotice`
- `LongTerm`

## 1) Mark Attendance
`POST /api/attendance/{sessionId}`

Request:
```json
{
  "attendances": [
    {
      "studentProfileId": "11111111-1111-1111-1111-111111111111",
      "attendanceStatus": "Present",
      "note": "On time"
    },
    {
      "studentProfileId": "22222222-2222-2222-2222-222222222222",
      "attendanceStatus": "Absent",
      "note": "Sick leave"
    }
  ]
}
```

Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "results": [
      {
        "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "studentProfileId": "11111111-1111-1111-1111-111111111111",
        "attendanceStatus": "Present",
        "absenceType": null,
        "markedAt": "2026-05-15T03:30:00Z",
        "note": "On time",
        "ticketConsumed": true,
        "consumedQuantity": 1,
        "advanceLessonProgression": true,
        "ticketBalance": 9
      }
    ]
  }
}
```

## 2) Update Attendance
`PUT /api/attendance/{sessionId}/students/{studentProfileId}`

Request:
```json
{
  "attendanceStatus": "Absent",
  "note": "Updated by teacher"
}
```

Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "studentProfileId": "11111111-1111-1111-1111-111111111111",
    "attendanceStatus": "Absent",
    "absenceType": "NoNotice",
    "note": "Updated by teacher",
    "ticketConsumed": true,
    "consumedQuantity": 1,
    "advanceLessonProgression": false,
    "ticketBalance": 8,
    "updatedAt": "2026-05-15T03:45:00Z"
  }
}
```

## 3) Get Session Attendance
`GET /api/attendance/{sessionId}`

Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "sessionName": "Starters A",
    "date": "2026-05-15",
    "startTime": "18:00:00",
    "endTime": "19:30:00",
    "summary": {
      "totalStudents": 20,
      "presentCount": 16,
      "absentCount": 2,
      "makeupCount": 1,
      "notMarkedCount": 1
    },
    "attendances": [
      {
        "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "studentProfileId": "11111111-1111-1111-1111-111111111111",
        "studentName": "Nguyen Van A",
        "studentAvatarUrl": "https://cdn.example.com/avatar/a.jpg",
        "registrationId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
        "track": "Primary",
        "isMakeup": false,
        "attendanceStatus": "Present",
        "absenceType": null,
        "hasMakeupCredit": false,
        "hasApprovedLeave": false,
        "note": "On time",
        "markedAt": "2026-05-15T03:30:00Z"
      }
    ]
  }
}
```

## 4) Get Student Attendance History
`GET /api/attendance/students?pageNumber=1&pageSize=10`

Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "sessionDateTime": "2026-05-15T18:00:00+07:00",
        "attendanceStatus": "Present",
        "absenceType": null,
        "note": "On time"
      }
    ],
    "pageNumber": 1,
    "totalPages": 3,
    "totalCount": 23
  }
}
```

## Error Format (ProblemDetails)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Attendance.SessionDateClosed",
  "status": 400,
  "detail": "Attendance for session '...' can only be marked or updated on the session date.",
  "errors": [
    {
      "code": "Attendance.SessionDateClosed",
      "description": "Attendance for session '...' can only be marked or updated on the session date."
    }
  ]
}
```

## Attendance Error Codes for FE

| HTTP | Code | Meaning |
|---|---|---|
| 400 | `Attendance.FutureSessionNotAllowed` | Teacher thao tác session tương lai |
| 400 | `Attendance.SessionDateClosed` | Teacher thao tác khi đã sang ngày hôm sau |
| 400 | `Attendance.ApprovedLeaveLocked` | Attendance bị khóa do leave request đã approved |
| 404 | `Attendance.NotFound` | Không tìm thấy attendance/session-student record |
| 404 | `Session.NotFound` | Không tìm thấy session |
| 401/403 | N/A | Token invalid hoặc không đủ role |

## FE Notes
- FE nên disable/hide nút Mark/Update khi user role không phải `Admin/Teacher`.
- Với Teacher, FE nên check theo local date của VN để chặn sớm UI, nhưng backend vẫn là nguồn quyết định cuối.
