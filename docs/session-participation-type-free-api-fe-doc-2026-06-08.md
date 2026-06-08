# Session ParticipationType Free API FE Doc - 2026-06-08

## 1. Muc tieu va pham vi

Tai lieu nay mo ta cac API FE can quan tam cho thay doi moi:

- `ParticipationType` co them nghiep vu hoat dong cho case `Free`.
- FE tam thoi chi duoc phep chon `Main` hoac `Free`.
- Cac enum `Makeup`, `ExtraPaid`, `Trial` tam thoi bi an o lookup va bi chan o write API.
- Attendance cua session `Free` khong tieu thu ve.

Tai lieu nay chi cover cac API bi anh huong hoac can FE dong bo de su dung thay doi tren:

- lookup
- session list / detail / create / update / bulk update
- teacher patch `sectionType`
- teacher / student / parent timetable
- attendance read / mark / update

Ngoai pham vi:

- change room
- change teacher
- complete / cancel session
- teaching log
- finance / invoice cho `ExtraPaid`

## 2. Tong quan thay doi moi

### 2.1 Hanh vi moi

- `GET /api/lookups` chi tra `participationType = Main, Free`.
- `POST /api/sessions`, `PUT /api/sessions/{id}`, `PUT /api/sessions/by-class` chi chap nhan `Main` va `Free`.
- Neu gui `Makeup`, `ExtraPaid`, `Trial` vao write API, backend tra `400 Session.InvalidParticipationType`.
- Khi attendance tren session co `participationType = Free`, backend khong consume learning ticket.

### 2.2 Rule nghiep vu quan trong

| Case | Ticket consumed | Consumed quantity | Advance lesson progression |
| --- | --- | --- | --- |
| `ParticipationType = Main`, `AttendanceStatus = Present` | Yes | `1` | Chi `true` khi `SectionType = Normal` |
| `ParticipationType = Main`, `AttendanceStatus = Absent`, `AbsenceType = NoNotice` | Yes | `1` | `false` |
| `ParticipationType = Main`, cac case khac | No | `0` | Theo rule `SectionType = Normal` + `Present` |
| `ParticipationType = Free`, moi attendance status | No | `0` | Van chi `true` khi `SectionType = Normal` va `AttendanceStatus = Present` |

### 2.3 Note ve du lieu legacy

- Domain enum van con `Makeup`, `ExtraPaid`, `Trial`.
- Read API hien tai chua mask du lieu legacy da ton tai trong DB.
- Nghia la:
  - lookup va write API chi dung `Main` / `Free`
  - mot so read API van co the tra `participationType = Makeup | ExtraPaid | Trial` neu session cu da duoc luu truoc do

Khuyen nghi FE:

- dropdown / form create-update chi render `Main`, `Free`
- read-side nen handle unknown/legacy string an toan, khong hard fail UI

## 3. Auth, scope va wrapper chung

Tat ca API trong tai lieu nay deu can Bearer token.

### 3.1 Success wrapper

```json
{
  "isSuccess": true,
  "data": {}
}
```

### 3.2 Error wrapper

#### Validation / not found / conflict

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Session.InvalidParticipationType",
  "status": 400,
  "detail": "Invalid participation type: 'Trial'. Valid values: Main, Free",
  "errors": [
    {
      "code": "Session.InvalidParticipationType",
      "description": "Invalid participation type: 'Trial'. Valid values: Main, Free"
    }
  ]
}
```

#### FluentValidation pipeline

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Class ID is required"
    }
  ]
}
```

### 3.3 HTTP status mapping

| HTTP status | Khi nao xay ra |
| --- | --- |
| `200` | Doc / update thanh cong |
| `201` | Tao session thanh cong |
| `400` | Validation error, enum invalid, rule nghiep vu invalid |
| `401` | Thieu token / token khong hop le |
| `403` | Sai role |
| `404` | Entity khong ton tai |
| `409` | Conflict phong / giao vien / dependency conflict |

## 4. Role, du lieu duoc xem, pham vi va hanh dong

| Role | Du lieu duoc xem trong pham vi tai lieu nay | Scope hien tai | Hanh dong duoc phep trong pham vi tai lieu nay |
| --- | --- | --- | --- |
| `Admin` | lookup, session list/detail, teacher timetable, attendance, student/parent-facing data neu goi dung API tuong ung | `all` | view, create, edit, bulk edit, patch section type, mark attendance, update attendance |
| `ManagementStaff` | lookup, session list/detail, teacher timetable | `all` | view, create, edit, bulk edit, patch section type |
| `Teacher` | lookup, session detail, teacher timetable, session attendance | `own` cho `GET /api/teacher/timetable`; cac session/attendance endpoint khac hien tai chi enforce theo role, chua enforce ownership theo session | view, patch section type, mark attendance, update attendance |
| `Student` | lookup, student timetable | `own` | view |
| `Parent` | lookup, parent timetable | `own linked students` | view |

### 4.1 Ghi chu quan trong ve scope thuc te

- `GET /api/sessions/{sessionId}` hien tai chi can authenticated user, khong co scope filter them theo role.
- `GET /api/attendance/{sessionId}`, `POST /api/attendance/{sessionId}`, `PUT /api/attendance/{sessionId}/students/{studentProfileId}` chi cho `Admin,Teacher`, nhung hien tai chua co teacher ownership check theo session.
- FE nen ap scope nghiep vu o UI, nhung backend hien tai chua enforce day du o cac endpoint tren.

## 5. Permission matrix theo role

| Endpoint | Admin | ManagementStaff | Teacher | Student | Parent |
| --- | --- | --- | --- | --- | --- |
| `GET /api/lookups` | Yes | Yes | Yes | Yes | Yes |
| `GET /api/sessions` | Yes | Yes | No | No | No |
| `GET /api/sessions/{sessionId}` | Yes | Yes | Yes | Yes | Yes |
| `POST /api/sessions` | Yes | Yes | No | No | No |
| `PUT /api/sessions/{sessionId}` | Yes | Yes | No | No | No |
| `PUT /api/sessions/by-class` | Yes | Yes | No | No | No |
| `PATCH /api/sessions/{sessionId}/section-type` | Yes | Yes | Yes | No | No |
| `GET /api/teacher/timetable` | Yes | Yes | Yes | No | No |
| `GET /api/students/timetable` | No | No | No | Yes | No |
| `GET /api/parent/timetable` | No | No | No | No | Yes |
| `GET /api/attendance/{sessionId}` | Yes | No | Yes | No | No |
| `POST /api/attendance/{sessionId}` | Yes | No | Yes | No | No |
| `PUT /api/attendance/{sessionId}/students/{studentProfileId}` | Yes | No | Yes | No | No |

## 6. Status / enum definition

### 6.1 ParticipationType

#### FE-selectable values hien tai

| Value | Y nghia |
| --- | --- |
| `Main` | Session hoc chuan, co the consume ticket theo attendance rule |
| `Free` | Session mien phi, khong consume ticket |

#### Hidden / temporarily unsupported values

| Value | Trang thai hien tai |
| --- | --- |
| `Makeup` | Van ton tai trong domain enum, nhung khong duoc tra ve o lookup va khong duoc chap nhan o write API |
| `ExtraPaid` | Tuong tu |
| `Trial` | Tuong tu |

### 6.2 SessionStatus

| Value | Y nghia |
| --- | --- |
| `Scheduled` | Session chua dien ra hoac dang cho xu ly |
| `Completed` | Session da hoan thanh |
| `Cancelled` | Session da huy |

#### Luong chuyen trang thai

`Scheduled -> Completed`

`Scheduled -> Cancelled`

Rule:

- Session `Completed` hoac `Cancelled` khong duoc update bang session update API.
- Session `Completed` hoac `Cancelled` khong duoc patch `sectionType`.

### 6.3 SectionType

| Value | Y nghia FE can biet |
| --- | --- |
| `Normal` | Buoi hoc thuong; `Present` moi advance lesson progression |
| `Review` | Buoi on tap |
| `Makeup` | Loai session phan loai nghiep vu, khac voi `ParticipationType.Makeup` |
| `Remedial` | Buoi phu dao |
| `Assessment` | Buoi danh gia |

### 6.4 AttendanceStatus

| Value | Y nghia |
| --- | --- |
| `Present` | Di hoc |
| `Absent` | Vang hoc |
| `Makeup` | Makeup / approved leave presentation trong mot so read API |
| `NotMarked` | Chua diem danh |

### 6.5 AbsenceType

| Value | Y nghia |
| --- | --- |
| `WithNotice24H` | Bao truoc tu 24h |
| `Under24H` | Bao truoc duoi 24h |
| `NoNotice` | Khong bao truoc |
| `LongTerm` | Vang dai han |

## 7. Danh sach API chi tiet

## 7.1 GET /api/lookups

### Muc dich

Lay enum lookup va danh sach `slotType` active de FE render filter / dropdown.

### Role va scope

- Role: tat ca user da dang nhap
- Scope: khong ap scope du lieu

### Query params

Khong co.

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "lookups": {
      "participationType": [
        { "value": "Main", "displayName": "Main" },
        { "value": "Free", "displayName": "Free" }
      ],
      "sectionType": [
        { "value": "Normal", "displayName": "Normal" },
        { "value": "Review", "displayName": "Review" },
        { "value": "Makeup", "displayName": "Makeup" },
        { "value": "Remedial", "displayName": "Remedial" },
        { "value": "Assessment", "displayName": "Assessment" }
      ],
      "sessionStatus": [
        { "value": "Scheduled", "displayName": "Scheduled" },
        { "value": "Completed", "displayName": "Completed" },
        { "value": "Cancelled", "displayName": "Cancelled" }
      ],
      "slotType": [
        { "value": "guid", "displayName": "ST-001" }
      ]
    }
  }
}
```

### Rule / validation

- `participationType` chi con `Main`, `Free`.
- `sectionType` van tra day du 5 values.
- `slotType` chi tra slot type dang `IsActive`.

### Error cases

| HTTP | Case |
| --- | --- |
| `401` | Chua dang nhap |
| `403` | Bi chan boi auth middleware |

## 7.2 GET /api/sessions

### Muc dich

Lay danh sach session cho man hinh admin / staff.

### Role va scope

- Role: `Admin`, `ManagementStaff`
- Scope: `all`

### Query params

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `classId` | `guid` | No | Loc theo lop |
| `branchId` | `guid` | No | Loc theo chi nhanh |
| `status` | `string` | No | `Scheduled`, `Completed`, `Cancelled` |
| `from` | `datetime` | No | Backend normalize sang UTC |
| `to` | `datetime` | No | Backend lay den cuoi ngay Vietnam |
| `pageNumber` | `int` | No | Default `1` |
| `pageSize` | `int` | No | Default `10` |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "sessions": {
      "items": [
        {
          "id": "session-guid",
          "classId": "class-guid",
          "classCode": "CLS-001",
          "classTitle": "Kidzgo Starter",
          "plannedDatetime": "2026-06-08T18:00:00",
          "actualDatetime": null,
          "durationMinutes": 90,
          "participationType": "Free",
          "sectionType": "Normal",
          "slotTypeId": "slot-guid",
          "slotTypeCode": "ST-EVE",
          "status": "Scheduled",
          "plannedRoomId": "room-guid",
          "plannedRoomName": "Room A",
          "plannedTeacherId": "teacher-guid",
          "plannedTeacherName": "Teacher A",
          "plannedAssistantId": "assistant-guid",
          "plannedAssistantName": "Assistant A"
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

### FE can dung cac field nao

- `participationType`: hien badge / chip `Main` hoac `Free`
- `sectionType`: hien loai buoi hoc
- `slotTypeId`, `slotTypeCode`: hien slot type
- `status`: enable / disable action button

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `401` | auth middleware | Unauthorized |
| `403` | auth middleware | Forbidden |

## 7.3 GET /api/sessions/{sessionId}

### Muc dich

Lay chi tiet 1 session.

### Role va scope

- Role: moi authenticated user
- Scope hien tai: khong co server-side filter them theo role

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Yes |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "session": {
      "id": "session-guid",
      "classId": "class-guid",
      "classCode": "CLS-001",
      "classTitle": "Kidzgo Starter",
      "plannedDatetime": "2026-06-08T18:00:00",
      "actualDatetime": null,
      "durationMinutes": 90,
      "participationType": "Free",
      "sectionType": "Normal",
      "slotTypeId": "slot-guid",
      "slotTypeCode": "ST-EVE",
      "status": "Scheduled",
      "attendanceSummary": {
        "totalStudents": 12,
        "presentCount": 5,
        "absentCount": 1,
        "makeupCount": 0,
        "notMarkedCount": 6
      }
    }
  }
}
```

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `404` | `Session.NotFound` | `Session with Id = '{sessionId}' was not found` |

## 7.4 POST /api/sessions

### Muc dich

Tao session thu cong.

### Role va scope

- Role: `Admin`, `ManagementStaff`
- Scope: `all`

### Request body

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `classId` | `guid` | Yes | Class phai ton tai |
| `plannedDatetime` | `datetime` | Yes | Khong duoc o qua khu |
| `durationMinutes` | `int` | Yes | `> 0` |
| `plannedRoomId` | `guid?` | No | Neu co phai active, dung branch |
| `plannedTeacherId` | `guid?` | No | Neu co phai la teacher active, dung branch |
| `plannedAssistantId` | `guid?` | No | Neu co phai la teacher active, dung branch |
| `slotTypeId` | `guid?` | No | Neu omit se fallback `Class.SlotTypeId` |
| `participationType` | `string` | No | Omit => `Main`; chap nhan `Main`, `Free` |
| `sectionType` | `string` | No | Omit => `Normal`; parse sai cung fallback `Normal` |

### Example request

```json
{
  "classId": "class-guid",
  "plannedDatetime": "2026-06-10T18:00:00",
  "durationMinutes": 90,
  "plannedRoomId": "room-guid",
  "plannedTeacherId": "teacher-guid",
  "plannedAssistantId": "assistant-guid",
  "slotTypeId": "slot-guid",
  "participationType": "Free",
  "sectionType": "Normal"
}
```

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "session-guid",
    "classId": "class-guid",
    "branchId": "branch-guid",
    "moduleId": "module-guid",
    "lessonPlanTemplateId": "template-guid",
    "sessionIndexInModule": 3,
    "plannedDatetime": "2026-06-10T18:00:00",
    "durationMinutes": 90,
    "sectionType": "Normal",
    "slotTypeId": "slot-guid",
    "slotTypeCode": "ST-EVE"
  }
}
```

### Luu y cho FE

- Response create khong echo lai `participationType`.
- Neu FE can confirm session vua tao dang la `Free`, FE nen reload `GET /api/sessions/{id}` hoac `GET /api/sessions`.

### Validation rules

- Chi tao duoc session cho class dang `Planned`, `Recruiting`, `Active`.
- `teacherId` va `assistantId` khong duoc trung nhau.
- Backend check conflict phong / teacher / assistant tai khung gio.
- `slotTypeId` neu sau fallback ma co gia tri thi phai ton tai va active.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | `Validation.General` | `PlannedDatetime is required` |
| `400` | `Validation.General` | `PlannedDatetime cannot be in the past` |
| `400` | `Validation.General` | `DurationMinutes must be greater than 0` |
| `400` | `Validation.General` | `SlotTypeId must not be empty` |
| `400` | `Session.InvalidParticipationType` | `Invalid participation type: '{value}'. Valid values: Main, Free` |
| `400` | `Session.InvalidClassStatus` | `Sessions can only be created for Planned, Recruiting, or Active classes` |
| `400` | `Session.SlotTypeNotFound` | `Slot type '{slotTypeId}' was not found or inactive.` |
| `400` | `Session.InvalidRoom` | Room invalid / inactive / wrong branch |
| `400` | `Session.InvalidTeacher` | Teacher invalid / inactive / wrong branch |
| `400` | `Session.InvalidAssistant` | Assistant invalid / inactive / wrong branch |
| `400` | `Session.TeacherAndAssistantMustDiffer` | Main teacher and assistant teacher must be different users |
| `404` | `Class.NotFound` | `Class with Id = '{classId}' was not found` |
| `409` | `Session.RoomOccupied` | Room conflict |
| `409` | `Session.TeacherOccupied` | Teacher conflict |
| `409` | `Session.AssistantOccupied` | Assistant conflict |

## 7.5 PUT /api/sessions/{sessionId}

### Muc dich

Cap nhat 1 session.

### Role va scope

- Role: `Admin`, `ManagementStaff`
- Scope: `all`

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Yes |

### Request body

Giong `POST /api/sessions`, nhung `classId` khong co trong body.

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `plannedDatetime` | `datetime` | Yes | Khong duoc o qua khu |
| `durationMinutes` | `int` | Yes | `> 0` |
| `plannedRoomId` | `guid?` | No | Co the set `null` de clear |
| `plannedTeacherId` | `guid?` | No | Co the set `null` de clear |
| `plannedAssistantId` | `guid?` | No | Co the set `null` de clear |
| `slotTypeId` | `guid?` | No | Omit => giu session slot, neu session cung null thi fallback class slot |
| `participationType` | `string` | No | Omit => `Main`; chap nhan `Main`, `Free` |
| `sectionType` | `string` | No | Omit => `Normal`; parse sai cung fallback `Normal` |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "session-guid",
    "plannedDatetime": "2026-06-10T18:00:00",
    "durationMinutes": 90,
    "sectionType": "Normal",
    "slotTypeId": "slot-guid",
    "slotTypeCode": "ST-EVE"
  }
}
```

### Luu y cho FE

- Response update khong echo `participationType`.
- FE nen reload detail/list sau update.

### Validation rules

- Session phai ton tai.
- Chi update duoc session `Scheduled`.
- Conflict / room / teacher / assistant rule giong create.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | `Session.InvalidParticipationType` | Invalid participation type |
| `400` | `Session.InvalidStatus` | `Only sessions with Scheduled status can be updated` |
| `400` | `Session.SlotTypeNotFound` | Slot type not found / inactive |
| `400` | `Session.InvalidRoom` | Room invalid |
| `400` | `Session.InvalidTeacher` | Teacher invalid |
| `400` | `Session.InvalidAssistant` | Assistant invalid |
| `400` | `Session.TeacherAndAssistantMustDiffer` | Main teacher and assistant must differ |
| `404` | `Session.NotFound` | Session not found |
| `409` | `Session.RoomOccupied` | Room conflict |
| `409` | `Session.TeacherOccupied` | Teacher conflict |
| `409` | `Session.AssistantOccupied` | Assistant conflict |

## 7.6 PUT /api/sessions/by-class

### Muc dich

Bulk update nhieu session trong cung 1 class.

### Role va scope

- Role: `Admin`, `ManagementStaff`
- Scope: `all`

### Request body

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `classId` | `guid` | Yes | Class phai ton tai |
| `sessionIds` | `guid[]?` | No | Neu co thi chi update cac session nay |
| `filterByStatus` | `string?` | No | Parse dung: `Scheduled`, `Completed`, `Cancelled`; parse sai => bo qua filter |
| `fromDate` | `datetime?` | No | Chi lay session tu ngay nay tro di |
| `plannedDatetime` | `datetime?` | No | Neu co thi khong duoc trong qua khu |
| `durationMinutes` | `int?` | No | Neu co thi FE nen gui `> 0` |
| `plannedRoomId` | `guid?` | No | Neu co thi phai valid |
| `plannedTeacherId` | `guid?` | No | Neu co thi phai valid |
| `plannedAssistantId` | `guid?` | No | Neu co thi phai valid |
| `slotTypeId` | `guid?` | No | Neu co thi phai active |
| `participationType` | `string?` | No | Chap nhan `Main`, `Free`; invalid => `400` |
| `sectionType` | `string?` | No | Parse sai => khong update field nay |

### Example request

```json
{
  "classId": "class-guid",
  "sessionIds": ["session-1", "session-2"],
  "participationType": "Free",
  "sectionType": "Normal"
}
```

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "updatedSessionsCount": 2,
    "updatedSessionIds": ["session-1", "session-2"],
    "skippedSessionIds": [],
    "errors": []
  }
}
```

### Rule / validation

- Backend tu dong bo qua session `Completed` va `Cancelled`.
- Neu sau filter khong con session nao, API van tra `200` voi:
  - `updatedSessionsCount = 0`
  - `errors = ["No sessions were found for update"]`
- Bulk update hien tai khong co co che clear field ve `null` cho room / teacher / assistant / slotType.
- Chi khi field co `HasValue` thi moi update.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | `Validation.General` | `Class ID is required` |
| `400` | `Validation.General` | `PlannedDatetime cannot be in the past` |
| `400` | `Validation.General` | `SlotTypeId must not be empty` |
| `400` | `Session.InvalidParticipationType` | Invalid participation type |
| `400` | `Session.SlotTypeNotFound` | Slot type not found / inactive |
| `400` | `Session.InvalidRoom` | Room invalid |
| `400` | `Session.InvalidTeacher` | Teacher invalid |
| `400` | `Session.InvalidAssistant` | Assistant invalid |
| `400` | `Session.TeacherAndAssistantMustDiffer` | Main teacher and assistant must differ |
| `404` | `Class.NotFound` | Class not found |

### Session-level skip trong `data.errors`

Session conflict trong bulk khong tra `409` cho tung item. Backend ghi vao `data.errors`, vi du:

```json
{
  "isSuccess": true,
  "data": {
    "updatedSessionsCount": 1,
    "updatedSessionIds": ["session-1"],
    "skippedSessionIds": ["session-2"],
    "errors": [
      "Session session-2: conflict - Teacher: CLS-002 - Kidzgo Advanced at 10/06/2026 18:00"
    ]
  }
}
```

## 7.7 PATCH /api/sessions/{sessionId}/section-type

### Muc dich

Patch nhanh `sectionType` cho 1 session.

### Role va scope

- Role: `Admin`, `ManagementStaff`, `Teacher`
- Scope:
  - `Admin`, `ManagementStaff`: `all`
  - `Teacher`: theo role; hien tai khong co teacher ownership check theo session

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Yes |

### Request body

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `sectionType` | `string` | Yes | Phai la 1 trong 5 enum hop le |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "id": "session-guid",
    "sectionType": "Review",
    "updatedAt": "2026-06-08T11:00:00Z"
  }
}
```

### Validation / rule

- Khac voi create/update, endpoint nay parse sai `sectionType` se fail ngay.
- Teacher chi duoc doi `sectionType` trong dung ngay session theo ngay Vietnam.
- Session `Completed` / `Cancelled` khong duoc patch.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | `Session.InvalidSectionType` | Invalid section type |
| `400` | `Session.InvalidStatus` | Session khong con `Scheduled` |
| `400` | `Session.TeacherCanOnlyChangeSectionTypeOnSessionDate` | Teacher patch sai ngay session |
| `404` | `Session.NotFound` | Session not found |

## 7.8 GET /api/teacher/timetable

### Muc dich

Lay timetable cho teacher, co hien `participationType`.

### Role va scope

- Role: `Teacher`, `Admin`, `ManagementStaff`
- Scope:
  - `Teacher`: `own`
  - `Admin`, `ManagementStaff`: co the xem teacher bat ky bang `teacherUserId`; neu omit se fallback current user id

### Query params

| Field | Type | Required | Note |
| --- | --- | --- | --- |
| `teacherUserId` | `guid?` | No | Nen gui khi user la admin/staff |
| `from` | `datetime?` | No | Loc tu ngay |
| `to` | `datetime?` | No | Loc den cuoi ngay Vietnam |
| `branchId` | `guid?` | No | Loc branch |
| `classId` | `guid?` | No | Loc class |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "session-guid",
        "classId": "class-guid",
        "classCode": "CLS-001",
        "classTitle": "Kidzgo Starter",
        "plannedDatetime": "2026-06-08T18:00:00",
        "actualDatetime": null,
        "durationMinutes": 90,
        "participationType": "Free",
        "status": "Scheduled",
        "plannedRoomId": "room-guid",
        "plannedRoomName": "Room A"
      }
    ]
  }
}
```

### Luu y cho FE

- JSON enum duoc serialize thanh string.
- Read API nay van co the tra legacy `participationType` neu DB da co du lieu cu.

### Error cases

| HTTP | Case |
| --- | --- |
| `401` | Unauthorized |
| `403` | Forbidden |

## 7.9 GET /api/students/timetable

### Muc dich

Lay timetable cua student hien tai.

### Role va scope

- Role: `Student`
- Scope: `own`

### Query params

| Field | Type | Required |
| --- | --- | --- |
| `from` | `datetime?` | No |
| `to` | `datetime?` | No |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "session-guid",
        "studentProfileId": "student-guid",
        "studentDisplayName": "Student A",
        "classId": "class-guid",
        "classCode": "CLS-001",
        "classTitle": "Kidzgo Starter",
        "plannedDatetime": "2026-06-08T18:00:00",
        "durationMinutes": 90,
        "participationType": "Free",
        "sectionType": "Normal",
        "slotTypeId": "slot-guid",
        "slotTypeCode": "ST-EVE",
        "status": "Scheduled",
        "isMakeup": false,
        "attendanceStatus": null,
        "absenceType": null,
        "attendanceMarkedAt": null
      }
    ]
  }
}
```

### Rule / validation

- Backend tu resolve student tu token.
- Neu khong resolve duoc student profile, API tra `404`.
- Read API van co the tra legacy `participationType` neu session cu ton tai.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `404` | `Profile.StudentNotFound` | Student profile not found |
| `404` | `Profile.NotFound` | The profile with the Id = '{studentId}' was not found |

## 7.10 GET /api/parent/timetable

### Muc dich

Lay timetable cua cac student linked voi parent hien tai.

### Role va scope

- Role: `Parent`
- Scope: `own linked students`

### Query params

| Field | Type | Required |
| --- | --- | --- |
| `from` | `datetime?` | No |
| `to` | `datetime?` | No |

### Success response

Format giong `GET /api/students/timetable`, nhung co the tra nhieu student trong cung response.

```json
{
  "isSuccess": true,
  "data": {
    "sessions": [
      {
        "id": "session-guid",
        "studentProfileId": "student-guid",
        "studentDisplayName": "Child A",
        "participationType": "Free",
        "sectionType": "Normal",
        "status": "Scheduled"
      }
    ]
  }
}
```

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `404` | `ParentProfile` | `Parent profile not found` |

## 7.11 GET /api/attendance/{sessionId}

### Muc dich

Lay danh sach attendance cua 1 session kem summary.

### Role va scope

- Role: `Admin`, `Teacher`
- Scope hien tai: role-based, chua co teacher ownership filter

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Yes |

### Success response

```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "session-guid",
    "sessionName": "Kidzgo Starter",
    "date": "2026-06-08",
    "startTime": "18:00:00",
    "endTime": "19:30:00",
    "summary": {
      "totalStudents": 12,
      "presentCount": 5,
      "absentCount": 1,
      "makeupCount": 0,
      "notMarkedCount": 6
    },
    "attendances": [
      {
        "id": "attendance-guid",
        "studentProfileId": "student-guid",
        "studentName": "Student A",
        "registrationId": "registration-guid",
        "track": "primary",
        "isMakeup": false,
        "attendanceStatus": "NotMarked",
        "absenceType": null,
        "hasMakeupCredit": false,
        "hasApprovedLeave": false,
        "note": null,
        "markedAt": null
      }
    ]
  }
}
```

### Rule / validation

- Approved leave student co the duoc expose la `attendanceStatus = Makeup` o read response.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `404` | `Session.NotFound` | Session not found |

## 7.12 POST /api/attendance/{sessionId}

### Muc dich

Mark attendance hang loat cho 1 session.

### Role va scope

- Role: `Admin`, `Teacher`
- Scope hien tai:
  - `Admin`: `all`
  - `Teacher`: role-based, chua co teacher ownership filter

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Yes |

### Request body

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `attendances` | `array` | Yes | Danh sach hoc sinh can diem danh |
| `attendances[].studentProfileId` | `guid` | Yes | Student phai duoc assign vao session |
| `attendances[].attendanceStatus` | `enum string` | Yes | `Present`, `Absent`, `Makeup`, `NotMarked` |
| `attendances[].note` | `string?` | No | Ghi chu |

### Example request

```json
{
  "attendances": [
    {
      "studentProfileId": "student-guid",
      "attendanceStatus": "Present",
      "note": "On time"
    }
  ]
}
```

### Success response cho session `Free`

```json
{
  "isSuccess": true,
  "data": {
    "results": [
      {
        "id": "attendance-guid",
        "sessionId": "session-guid",
        "studentProfileId": "student-guid",
        "attendanceStatus": "Present",
        "absenceType": null,
        "markedAt": "2026-06-08T18:05:00",
        "note": "On time",
        "ticketConsumed": false,
        "consumedQuantity": 0,
        "advanceLessonProgression": true,
        "ticketBalance": null,
        "ticketCompatibilityPassed": null,
        "ticketCompatibilityReason": null
      }
    ]
  }
}
```

### Rule / validation

- Teacher khong duoc mark future session.
- Teacher khong duoc mark session khac ngay hien tai theo ngay Vietnam.
- Admin duoc bypass 2 rule tren.
- Student khong assigned vao session => fail request.
- Approved leave student:
  - teacher mark se duoc tra ve result `AttendanceStatus = Makeup`
  - khong consume ticket
- Neu session la `Free`:
  - `ticketConsumed = false`
  - `consumedQuantity = 0`
  - `ticketBalance = null`
  - `ticketCompatibilityPassed = null`
  - `ticketCompatibilityReason = null`

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | `Attendance.FutureSessionNotAllowed` | Teacher mark future session |
| `400` | `Attendance.SessionDateClosed` | Teacher mark session khac ngay |
| `400` | `Attendance.StudentNotAssigned` | Student khong assigned vao session |
| `404` | `Attendance.NotFound` | Session attendance target not found |

## 7.13 PUT /api/attendance/{sessionId}/students/{studentProfileId}

### Muc dich

Cap nhat attendance cho 1 hoc sinh trong 1 session.

### Role va scope

- Role: `Admin`, `Teacher`
- Scope hien tai:
  - `Admin`: `all`
  - `Teacher`: role-based, chua co teacher ownership filter

### Path params

| Field | Type | Required |
| --- | --- | --- |
| `sessionId` | `guid` | Yes |
| `studentProfileId` | `guid` | Yes |

### Request body

| Field | Type | Required |
| --- | --- | --- |
| `attendanceStatus` | `enum string` | Yes |
| `note` | `string?` | No |

### Success response cho session `Free`

```json
{
  "isSuccess": true,
  "data": {
    "id": "attendance-guid",
    "sessionId": "session-guid",
    "studentProfileId": "student-guid",
    "attendanceStatus": "Absent",
    "absenceType": "NoNotice",
    "note": "Late notice",
    "ticketConsumed": false,
    "consumedQuantity": 0,
    "advanceLessonProgression": false,
    "ticketBalance": null,
    "ticketCompatibilityPassed": null,
    "ticketCompatibilityReason": null,
    "updatedAt": "2026-06-08T18:10:00"
  }
}
```

### Rule / validation

- Teacher khong duoc update future session.
- Teacher khong duoc update session khac ngay hien tai theo ngay Vietnam.
- Teacher khong duoc sua attendance cua student co approved leave.
- Admin bypass rule ngay va approved leave lock.
- Neu session la `Free`, response ticket fields giong case mark attendance.

### Error cases

| HTTP | Code | Detail |
| --- | --- | --- |
| `400` | `Attendance.FutureSessionNotAllowed` | Teacher update future session |
| `400` | `Attendance.SessionDateClosed` | Teacher update session khac ngay |
| `400` | `Attendance.ApprovedLeaveLocked` | Attendance bi lock do approved leave |
| `404` | `Attendance.NotFound` | Attendance not found for session + student |

## 8. Validation rule tong hop cho FE

### 8.1 Session create/update

- FE dropdown `participationType` chi cho:
  - `Main`
  - `Free`
- FE dropdown `sectionType` cho 5 gia tri:
  - `Normal`
  - `Review`
  - `Makeup`
  - `Remedial`
  - `Assessment`
- `plannedDatetime` khong gui gia tri qua khu.
- `durationMinutes > 0`.
- `slotTypeId` neu co thi khong gui `Guid.Empty`.
- `plannedTeacherId` va `plannedAssistantId` khong duoc trung nhau.
- Neu FE muon xoa room / teacher / assistant o single update, co the gui `null`.
- Neu FE bulk update:
  - `participationType = null` nghia la khong doi field nay
  - room / teacher / assistant / slotType hien tai khong clear ve `null` duoc qua bulk API

### 8.2 SectionType parse nuance

- `POST /api/sessions` va `PUT /api/sessions/{id}`:
  - `sectionType` omit hoac parse sai => backend silently fallback `Normal`
- `PUT /api/sessions/by-class`:
  - `sectionType` parse sai => backend bo qua, khong update field
- `PATCH /api/sessions/{id}/section-type`:
  - `sectionType` parse sai => backend tra `400 Session.InvalidSectionType`

### 8.3 ParticipationType parse nuance

- `POST /api/sessions`, `PUT /api/sessions/{id}`:
  - omit => `Main`
  - invalid => `400 Session.InvalidParticipationType`
- `PUT /api/sessions/by-class`:
  - omit => khong update field
  - invalid => `400 Session.InvalidParticipationType`

## 9. Cac truong hop tra loi FE can map ro

| Group | Case | Goi y xu ly FE |
| --- | --- | --- |
| Lookup | `participationType` chi con `Main`, `Free` | An `Makeup`, `ExtraPaid`, `Trial` khoi dropdown |
| Session write | `Session.InvalidParticipationType` | Show inline error o field `participationType` |
| Session write | `Session.SlotTypeNotFound` | Reload lookup slot type / bat user chon lai |
| Session write | `Session.InvalidRoom` / `InvalidTeacher` / `InvalidAssistant` | Show inline error field lien quan |
| Session write | conflict `RoomOccupied` / `TeacherOccupied` / `AssistantOccupied` | Show toast + giu form de user chon slot khac |
| Bulk update | `data.errors` khong rong du API tra `200` | Xu ly nhu partial success, show danh sach skipped |
| Attendance free | `ticketConsumed = false` | Khong show modal tru ve / khong tru remaining session |
| Legacy read data | `participationType` tra ve `Trial` / `ExtraPaid` / `Makeup` | Hien badge `Legacy` hoac fallback text, khong crash UI |

## 10. Goi y FE implementation

- Session create/update form:
  - dropdown `participationType` chi render `Main`, `Free` tu `GET /api/lookups`
  - sau create/update thanh cong, reload session detail/list vi response khong echo `participationType`
- Session list / detail / timetable:
  - render `participationType` nhu read-only chip
  - handle safe fallback cho legacy values
- Attendance UI:
  - neu session la `Free`, co the hien helper text: `Khong tru ve`
  - sau mark/update, trust response field `ticketConsumed` va `consumedQuantity`
  - khong tu suy luan consume ticket chi dua vao `attendanceStatus`
- Teacher UI:
  - chi cho teacher patch `sectionType` trong ngay session
  - mac du backend chua enforce ownership day du o attendance/session detail, FE nen chi expose session thuoc teacher do phu trach

