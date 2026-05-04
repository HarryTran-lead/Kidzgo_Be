# Session Change Room / Teacher API FE Doc

## Muc tieu

Tai lieu nay mo ta flow FE tu buoc lay danh sach session theo `classId`, sau do goi 2 API moi:

- Doi phong hoc cho mot hoac nhieu session dang dien ra / trong tuong lai.
- Doi giao vien chinh hoac giao vien phu cho mot hoac nhieu session dang dien ra / trong tuong lai.

Backend giu nguyen gio hoc, duration, participation type va cac field khac. Chi update field phong hoac giao vien tuong ung.

## Auth / Role

Tat ca API trong tai lieu nay can Bearer token.

Role duoc phep:

- `Admin`
- `ManagementStaff`

Neu token thieu / sai role, API tra `401` hoac `403` theo middleware auth hien tai.

## Wrapper chung

### Success

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Problem Details

Validator / domain error tra ve Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Session.InvalidRoom",
  "status": 400,
  "detail": "Room with ID 11111111-1111-1111-1111-111111111111 does not exist, is inactive, or does not belong to this branch"
}
```

Validator pipeline co them `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "At least one session ID is required"
    }
  ]
}
```

Luu y: voi 2 API bulk moi, loi tren tung session nhu session qua khu, conflict, session not found se nam trong `data.errors` va API van co the tra `200` neu request hop le. FE can doc `updatedSessionIds`, `skippedSessionIds`, `errors`.

## Flow FE de doi phong / giao vien

1. Goi `GET /api/sessions?classId={classId}&status=Scheduled&pageNumber=1&pageSize=100`.
2. Hien thi danh sach session cho user chon mot hoac nhieu buoi.
3. Neu doi phong: user chon `roomId`, goi `PATCH /api/sessions/change-room`.
4. Neu doi giao vien: user chon `teacherId` va role `MainTeacher` hoac `Assistant`, goi `PATCH /api/sessions/change-teacher`.
5. Sau khi thanh cong, reload lai `GET /api/sessions?classId={classId}` de dong bo UI.

## 1. Get sessions by class id

### Endpoint

`GET /api/sessions`

### Query params

| Param | Type | Required | Note |
| --- | --- | --- | --- |
| `classId` | `guid` | No | Dung de lay session cua mot lop. |
| `branchId` | `guid` | No | Loc theo branch. |
| `status` | `string` | No | `Scheduled`, `Completed`, `Cancelled`. |
| `from` | `datetime` | No | FE gui gio local Vietnam hoac ISO; BE normalize ve UTC. |
| `to` | `datetime` | No | BE lay den cuoi ngay Vietnam. |
| `pageNumber` | `number` | No | Default `1`. |
| `pageSize` | `number` | No | Default `10`. |

### Example request

```http
GET /api/sessions?classId=aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa&status=Scheduled&pageNumber=1&pageSize=100
Authorization: Bearer {token}
```

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "sessions": {
      "items": [
        {
          "id": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
          "color": "#4F46E5",
          "classId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
          "classCode": "KZ-001",
          "classTitle": "Kidzgo Starter",
          "branchId": "cccccccc-cccc-cccc-cccc-cccccccccccc",
          "branchName": "Kidzgo Branch 1",
          "plannedDatetime": "2026-05-06T18:00:00",
          "actualDatetime": null,
          "durationMinutes": 90,
          "participationType": "Main",
          "status": "Scheduled",
          "plannedRoomId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
          "plannedRoomName": "Room A",
          "actualRoomId": null,
          "actualRoomName": null,
          "plannedTeacherId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
          "plannedTeacherName": "Teacher A",
          "actualTeacherId": null,
          "actualTeacherName": null,
          "plannedAssistantId": "ffffffff-ffff-ffff-ffff-ffffffffffff",
          "plannedAssistantName": "Assistant A",
          "actualAssistantId": null,
          "actualAssistantName": null
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  }
}
```

### FE fields can dung

- `id`: truyen vao `sessionId` hoac `sessionIds`.
- `plannedDatetime`, `durationMinutes`: hien thi gio hoc; 2 API moi khong can gui lai cac field nay.
- `status`: FE nen chi cho chon `Scheduled`.
- `plannedRoomId`, `plannedRoomName`: phong hien tai.
- `plannedTeacherId`, `plannedTeacherName`: giao vien chinh hien tai.
- `plannedAssistantId`, `plannedAssistantName`: giao vien phu hien tai.

## 2. Change room for sessions

### Endpoint

`PATCH /api/sessions/change-room`

### Request body

```json
{
  "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "sessionIds": [
    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "99999999-9999-9999-9999-999999999999"
  ],
  "roomId": "dddddddd-dddd-dddd-dddd-dddddddddddd"
}
```

Rules:

- FE co the truyen `sessionId` hoac `sessionIds`.
- Neu truyen ca hai, BE gom distinct.
- `roomId` bat buoc.
- Chi update session chua `Cancelled/Completed` va session chua ket thuc.
- Gio hoc giu nguyen.
- BE check phong cung branch, active, va check conflict theo slot hien tai.

### Single session payload

```json
{
  "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "roomId": "dddddddd-dddd-dddd-dddd-dddddddddddd"
}
```

### Bulk payload

```json
{
  "sessionIds": [
    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "99999999-9999-9999-9999-999999999999"
  ],
  "roomId": "dddddddd-dddd-dddd-dddd-dddddddddddd"
}
```

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "updatedSessionsCount": 1,
    "updatedSessionIds": [
      "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
    ],
    "skippedSessionIds": [
      "99999999-9999-9999-9999-999999999999"
    ],
    "errors": [
      "Session 99999999-9999-9999-9999-999999999999: Room is already occupied by class 'KZ-002 - Kidzgo Advanced' at 06/05/2026 18:00"
    ]
  }
}
```

### Possible `data.errors`

| Case | Message |
| --- | --- |
| Session khong ton tai | `Session {sessionId}: Session with Id = '{sessionId}' was not found` |
| Session cancelled/completed | `Session {sessionId}: Session with Id = '{sessionId}' cannot be changed because it is cancelled or completed` |
| Session da ket thuc | `Session {sessionId}: Session with Id = '{sessionId}' cannot be changed because it has already ended` |
| Room khong hop le | `Session {sessionId}: Room with ID {roomId} does not exist, is inactive, or does not belong to this branch` |
| Room conflict | `Session {sessionId}: Room is already occupied by class '{classCode} - {classTitle}' at dd/MM/yyyy HH:mm` |

### Problem Details errors

| Status | Title | Detail |
| --- | --- | --- |
| `400` | `Validation.General` | `At least one session ID is required` |
| `400` | `Validation.General` | `Session ID cannot be empty` |
| `400` | `Validation.General` | `Room ID is required` |

## 3. Change teacher for sessions

### Endpoint

`PATCH /api/sessions/change-teacher`

### Request body

```json
{
  "sessionId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
  "sessionIds": [
    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
    "99999999-9999-9999-9999-999999999999"
  ],
  "teacherId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "role": "MainTeacher"
}
```

Rules:

- FE co the truyen `sessionId` hoac `sessionIds`.
- Neu truyen ca hai, BE gom distinct.
- `teacherId` bat buoc.
- `role` bat buoc ve nghia FE, default backend la `MainTeacher` neu field bi omit trong JSON model binding.
- Role chap nhan:
  - `MainTeacher`
  - `main`
  - `teacher`
  - `Assistant`
  - `AssistantTeacher`
- Chi update session chua `Cancelled/Completed` va session chua ket thuc.
- Gio hoc, phong, duration va participation type giu nguyen.
- BE check teacher active, role `Teacher`, cung branch.
- BE khong cho giao vien chinh va giao vien phu trung nhau.
- BE check conflict theo slot hien tai.

### Change main teacher payload

```json
{
  "sessionIds": [
    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
  ],
  "teacherId": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "role": "MainTeacher"
}
```

### Change assistant teacher payload

```json
{
  "sessionIds": [
    "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
  ],
  "teacherId": "ffffffff-ffff-ffff-ffff-ffffffffffff",
  "role": "Assistant"
}
```

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "updatedSessionsCount": 1,
    "updatedSessionIds": [
      "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"
    ],
    "skippedSessionIds": [
      "99999999-9999-9999-9999-999999999999"
    ],
    "errors": [
      "Session 99999999-9999-9999-9999-999999999999: Teacher is already assigned to class 'KZ-002 - Kidzgo Advanced' at 06/05/2026 18:00"
    ]
  }
}
```

### Possible `data.errors`

| Case | Message |
| --- | --- |
| Session khong ton tai | `Session {sessionId}: Session with Id = '{sessionId}' was not found` |
| Session cancelled/completed | `Session {sessionId}: Session with Id = '{sessionId}' cannot be changed because it is cancelled or completed` |
| Session da ket thuc | `Session {sessionId}: Session with Id = '{sessionId}' cannot be changed because it has already ended` |
| Main teacher khong hop le | `Session {sessionId}: Main teacher with ID {teacherId} does not exist, is inactive, is not a teacher, or does not belong to this branch` |
| Assistant khong hop le | `Session {sessionId}: Assistant teacher with ID {teacherId} does not exist, is inactive, is not a teacher, or does not belong to this branch` |
| Trung main/assistant | `Session {sessionId}: Main teacher and assistant teacher must be different users` |
| Main teacher conflict | `Session {sessionId}: Teacher is already assigned to class '{classCode} - {classTitle}' at dd/MM/yyyy HH:mm` |
| Assistant conflict | `Session {sessionId}: Assistant teacher is already assigned to class '{classCode} - {classTitle}' at dd/MM/yyyy HH:mm` |

### Problem Details errors

| Status | Title | Detail |
| --- | --- | --- |
| `400` | `Validation.General` | `At least one session ID is required` |
| `400` | `Validation.General` | `Session ID cannot be empty` |
| `400` | `Validation.General` | `Teacher ID is required` |
| `400` | `Session.InvalidTeacherRole` | `Invalid teacher role: '{role}'. Valid values: MainTeacher, Assistant` |

## FE handling suggestion

- Neu `updatedSessionsCount > 0`: show success count.
- Neu `errors.length > 0`: show warning/toast va list skipped sessions.
- Neu `updatedSessionsCount == 0` va `errors.length > 0`: xem nhu failed bulk action ve mat UX.
- Sau moi action, reload lai `GET /api/sessions?classId={classId}`.
- FE nen disable checkbox cho session co `status != "Scheduled"`.
- FE nen dung `plannedDatetime + durationMinutes` de an hanh dong voi buoi da ket thuc neu muon validate truoc, nhung backend van la source of truth.
