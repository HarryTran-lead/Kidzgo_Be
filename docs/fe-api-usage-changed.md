# FE API Usage - Holiday & Makeup Changes

Scope: cac API thay doi trong tab nay, khong bao gom `ResumePausedEnrollmentsJob`.

## Common Format

Auth: gui `Authorization: Bearer <token>`.

Success response:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error response dang `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Error.Code",
  "status": 400,
  "detail": "Message for FE/user"
}
```

Date format: `yyyy-MM-dd`. DateTime response duoc format theo gio Viet Nam, vi du `2026-03-24T22:22:24+07:00`.

## Holiday APIs

Holiday ap dung toan he thong, khong con `branchId`. Logic sinh session se bo qua cac ngay nam trong holiday dang active.

### GET `/api/holidays`

Role: `Admin`, `ManagementStaff`

Query:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `isActive` | boolean | No | Loc trang thai active/inactive |
| `from` | date | No | Loc holiday giao voi khoang ngay |
| `to` | date | No | Loc holiday giao voi khoang ngay |

Response `200`:

```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "4bde79fd-0df8-44a8-8f58-62726d57c8f2",
      "name": "Tet Nguyen Dan",
      "startDate": "2026-02-16",
      "endDate": "2026-02-20",
      "description": "Nghi Tet",
      "isActive": true,
      "createdAt": "2026-04-29T10:00:00+07:00",
      "updatedAt": "2026-04-29T10:00:00+07:00"
    }
  ]
}
```

### GET `/api/holidays/{id}`

Role: `Admin`, `ManagementStaff`

Response `200`: `data` la mot `HolidayResponse` nhu tren.

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 404 | `Holiday.NotFound` | `Holiday with Id = '{id}' was not found` |

### POST `/api/holidays`

Role: `Admin`, `ManagementStaff`

Request:

```json
{
  "name": "Tet Nguyen Dan",
  "startDate": "2026-02-16",
  "endDate": "2026-02-20",
  "description": "Nghi Tet",
  "isActive": true
}
```

Response `201`: `data` la `HolidayResponse`.

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 400 | `Holiday.NameRequired` | `Holiday name is required` |
| 400 | `Holiday.InvalidDateRange` | `EndDate must be greater than or equal to StartDate` |

### PUT `/api/holidays/{id}`

Role: `Admin`, `ManagementStaff`

Request: giong `POST /api/holidays`.

Response `200`: `data` la `HolidayResponse` sau khi update.

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 400 | `Holiday.NameRequired` | `Holiday name is required` |
| 400 | `Holiday.InvalidDateRange` | `EndDate must be greater than or equal to StartDate` |
| 404 | `Holiday.NotFound` | `Holiday with Id = '{id}' was not found` |

### PATCH `/api/holidays/{id}/toggle-status`

Role: `Admin`, `ManagementStaff`

Request: no body.

Response `200`: `data` la `HolidayResponse` voi `isActive` da dao trang thai.

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 404 | `Holiday.NotFound` | `Holiday with Id = '{id}' was not found` |

### DELETE `/api/holidays/{id}`

Role: `Admin`

Request: no body.

Response `200`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "4bde79fd-0df8-44a8-8f58-62726d57c8f2",
    "deleted": true
  }
}
```

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 404 | `Holiday.NotFound` | `Holiday with Id = '{id}' was not found` |

## Makeup Settings APIs

Dung de cau hinh han su dung mac dinh cua makeup credit tren toan he thong. Default hien tai: `7` ngay.

### GET `/api/makeup-credits/settings`

Role: `Admin`, `ManagementStaff`

Response `200`:

```json
{
  "isSuccess": true,
  "data": {
    "creditExpiryDays": 7,
    "createdAt": "2026-04-29T10:00:00+07:00",
    "updatedAt": null
  }
}
```

Note: neu chua co settings trong DB, BE se tu tao row default `creditExpiryDays = 7`.

### PUT `/api/makeup-credits/settings`

Role: `Admin`, `ManagementStaff`

Request:

```json
{
  "creditExpiryDays": 7
}
```

Response `200`: `data` la `MakeupSettingsResponse`.

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 400 | `MakeupSettings.InvalidCreditExpiryDays` | `CreditExpiryDays must be greater than 0.` |

## Makeup Credit APIs With Changed Rule

### POST `/api/makeup-credits/{id}/use`

Role: authenticated user. Parent/student flow van duoc resolve theo current user; staff/admin cung co the goi neu co quyen auth theo rule hien tai cua controller.

Business thay doi:

- Khong hard-code target session phai la weekend.
- Target session bat buoc thuoc class co `program.isMakeup = true`.
- Neu credit da duoc dung va session cu la ngay hom nay/qua khu thi khong duoc doi lich bu.
- Neu reschedule credit da dung, target moi phai cung makeup program voi allocation hien tai.

Request:

```json
{
  "studentProfileId": "f407429b-8425-48d6-97f2-28e2441baf5e",
  "classId": "2e5c1aa3-e847-4067-a2da-47035c963a71",
  "targetSessionId": "86a3ea56-362b-467d-88a2-e4e20f2fe208"
}
```

Field notes:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `studentProfileId` | guid | Required for parent | Parent bat buoc gui; student user co the omit |
| `classId` | guid | Yes | Class cua target session |
| `targetSessionId` | guid | Yes | Session can xep bu |

Response `200`:

```json
{
  "isSuccess": true,
  "data": null
}
```

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 400 | `MakeupCredit.NotAvailable` | `Makeup credit '{id}' is not available for allocation.` |
| 400 | `MakeupCredit.Expired` | `Makeup credit '{id}' is expired.` |
| 400 | `MakeupCredit.NotBelongToStudent` | `This makeup credit does not belong to the specified student.` |
| 400 | `MakeupCredit.TargetClassMustBeMakeupProgram` | `Target session must belong to a makeup program.` |
| 400 | `MakeupCredit.MustBeFutureWeek` | `Makeup session must be in the weeks after the missed week.` |
| 400 | `MakeupCredit.CannotUsePastDate` | `Cannot use makeup credit for past dates.` |
| 400 | `MakeupCredit.CannotChangeAllocatedPastSession` | `Cannot change makeup session because the allocated session is today or has already passed.` |
| 400 | `MakeupCredit.ParentMustProvideStudentProfileId` | `Parent must provide StudentProfileId.` |
| 400 | `MakeupCredit.StudentNotBelongToParent` | `Student does not belong to this parent.` |
| 400 | `MakeupCredit.SessionNotBelongToClass` | `Target session does not belong to the specified class.` |
| 400 | `MakeupCredit.MustStayInCurrentMakeupProgram` | `Target session must belong to the same makeup program as the current allocation.` |
| 400 | `MakeupCredit.StudentAlreadyInTargetSession` | `Student is already assigned to the target session.` |
| 400 | `MakeupCredit.TargetSessionConflict` | `Target session conflicts with another session already assigned to the student.` |
| 404 | `MakeupCredit.NotFound` | `The makeup credit with Id = '{id}' was not found.` |
| 409 | `MakeupCredit.TargetSessionFull` | `Target makeup session has no available slot.` |

### GET `/api/makeup-credits/{id}/parent/get-available-sessions`

Role: authenticated user.

Business thay doi:

- Danh sach goi y chi tra ve session cua class co `program.isMakeup = true`.
- FE khong can tu filter weekend nua; lich cuoi tuan do trung tam tu cau hinh class/program bu.

Query:

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `fromDate` | date | No | Ngay bat dau loc |
| `toDate` | date | No | Ngay ket thuc loc |
| `timeOfDay` | string | No | Theo rule cu cua endpoint hien co |

Response `200`:

```json
{
  "isSuccess": true,
  "data": [
    {
      "sessionId": "86a3ea56-362b-467d-88a2-e4e20f2fe208",
      "classId": "2e5c1aa3-e847-4067-a2da-47035c963a71",
      "classCode": "MK-001",
      "classTitle": "Makeup Weekend",
      "programName": "Makeup Program",
      "programCode": "MAKEUP",
      "branchId": "dd4f4f66-3b2f-42df-b687-76684edc8f0e",
      "plannedDatetime": "2026-05-02T08:00:00+07:00",
      "plannedEndDatetime": "2026-05-02T09:30:00+07:00"
    }
  ]
}
```

Errors:

| HTTP | Code | Message |
| --- | --- | --- |
| 404 | `MakeupCredit.NotFound` | `The makeup credit with Id = '{id}' was not found.` |

## Related Behavior FE Should Expect

- Makeup credit sinh tu leave da approve se co `expiresAt` mac dinh bang cuoi ngay Viet Nam sau `creditExpiryDays` tinh tu ngay source session. Default la 7 ngay neu admin chua cau hinh.
- Holiday active ap dung toan he thong cho cac flow sinh session hoc. Session roi vao holiday se bi skip va BE se sinh bu them slot khac de du so buoi cua class.
- Class khong co `endDate` se duoc BE maintain theo rolling window 8 tuan, tranh sinh lich vo han.
