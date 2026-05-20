# FE API Doc - Create Class

Updated date: 2026-05-20

## 1. Endpoint

### POST `/api/classes`

Tao lop hoc moi.

Auth:

- `Authorization: Bearer <token>`
- Roles: `Admin`, `ManagementStaff`

Headers:

```http
Content-Type: application/json
Authorization: Bearer <token>
```

Success status:

- `201 Created`

Success envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

## 2. Correct Request Body

Payload hop le cho form tao lop:

```json
{
  "branchId": "da316382-35e8-4094-a99b-ce45e5f2627a",
  "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
  "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
  "startModuleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
  "code": "S1",
  "title": "Starters 1",
  "roomId": "44444444-4444-4444-4444-444444444304",
  "mainTeacherId": "3a32f148-d21c-4a01-b346-cdc31df755b2",
  "assistantTeacherId": null,
  "slotTypeId": "0c6018e6-5b70-4a3e-ba89-ca6d3f2a577e",
  "startDate": "2026-05-21",
  "endDate": "2026-08-21",
  "capacity": 10,
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "TU",
      "startTime": "18:00",
      "durationMinutes": 120
    },
    {
      "dayOfWeek": "TH",
      "startTime": "18:00",
      "durationMinutes": 120
    }
  ],
  "description": "Lop Starter S1"
}
```

Important:

- Khong gui `endDate: ""`.
- Neu co `weeklyScheduleSlots`, backend hien tai bat buoc `endDate` phai la date hop le va `>= startDate`.
- Neu chua co ngay ket thuc, FE khong nen submit payload co weekly schedule cho den khi backend cho phep rolling class `endDate = null`.
- `status` trong request create class khong duoc backend dung. Backend tao lop moi voi `status = "Active"`.
- `title` co the gui, hoac gui `name`. Backend lay `title = name ?? title ?? code`.

## 3. Field Contract

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `branchId` | `Guid` | Yes | Branch phai active |
| `programId` | `Guid` | Yes | Program phai active, chua deleted |
| `levelId` | `Guid` | Yes | Level phai ton tai va thuoc `programId` |
| `startModuleId` | `Guid` | Yes | Module bat dau, phai active va thuoc `levelId` |
| `code` | `string` | Yes | Khong rong, max 50, unique |
| `title` | `string?` | Optional | Max 255; neu khong gui thi backend fallback ve `code` |
| `name` | `string?` | Optional | Neu co, backend uu tien lam title |
| `roomId` | `Guid?` | Optional | Neu co, room phai active va cung branch |
| `mainTeacherId` | `Guid?` | Optional | Neu co, user phai role Teacher, active, cung branch |
| `assistantTeacherId` | `Guid?` | Optional | Neu co, phai khac main teacher, role Teacher, active, cung branch |
| `slotTypeId` | `Guid?` | Optional | Neu co, slot type phai active va khong duoc Guid empty |
| `startDate` | `date` | Yes | Format `YYYY-MM-DD` |
| `endDate` | `date?` | Required when weekly schedule exists | Format `YYYY-MM-DD`, phai `>= startDate` |
| `capacity` | `number` | Yes | `> 0` |
| `weeklyScheduleSlots` | `ScheduleSlot[]?` | Optional, but UI tao lich nen gui | Khong duplicate ngay + gio |
| `description` | `string?` | Optional | Ghi chu |

## 4. ScheduleSlot Contract

```json
{
  "dayOfWeek": "TU",
  "startTime": "18:00",
  "durationMinutes": 120
}
```

Rules:

- `dayOfWeek`: chi dung `MO`, `TU`, `WE`, `TH`, `FR`, `SA`, `SU`.
- `startTime`: dung format `HH:mm`, vi du `18:00`.
- `durationMinutes`: integer `> 0`.
- Khong duoc duplicate cung `dayOfWeek + startTime`.

Mapping ngay:

| Code | UI label |
| --- | --- |
| `MO` | Thu 2 |
| `TU` | Thu 3 |
| `WE` | Thu 4 |
| `TH` | Thu 5 |
| `FR` | Thu 6 |
| `SA` | Thu 7 |
| `SU` | Chu nhat |

## 5. Response

Response `201 Created`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "f3cfc6e0-59d0-4303-8bf7-d8b56b61d7e8",
    "branchId": "da316382-35e8-4094-a99b-ce45e5f2627a",
    "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
    "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
    "startModuleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
    "currentModuleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
    "code": "S1",
    "title": "Starters 1",
    "roomId": "44444444-4444-4444-4444-444444444304",
    "mainTeacherId": "3a32f148-d21c-4a01-b346-cdc31df755b2",
    "assistantTeacherId": null,
    "slotTypeId": "0c6018e6-5b70-4a3e-ba89-ca6d3f2a577e",
    "slotTypeCode": "STANDARD",
    "startDate": "2026-05-21",
    "endDate": "2026-08-21",
    "status": "Active",
    "capacity": 10,
    "weeklyScheduleSlots": [
      {
        "dayOfWeek": "TU",
        "startTime": "18:00",
        "durationMinutes": 120
      },
      {
        "dayOfWeek": "TH",
        "startTime": "18:00",
        "durationMinutes": 120
      }
    ],
    "description": "Lop Starter S1",
    "name": "Starters 1",
    "scheduleText": "Thu 3 18:00-20:00, Thu 5 18:00-20:00"
  }
}
```

## 6. Common FE Mistakes

### Mistake 1: `endDate` la chuoi rong

Sai:

```json
{
  "startDate": "2026-05-21",
  "endDate": "",
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TU", "startTime": "18:00", "durationMinutes": 120 }
  ]
}
```

Dung:

```json
{
  "startDate": "2026-05-21",
  "endDate": "2026-08-21",
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TU", "startTime": "18:00", "durationMinutes": 120 }
  ]
}
```

Neu field optional khong co gia tri, FE gui `null` hoac bo field, khong gui empty string. Rieng create class co `weeklyScheduleSlots` thi `endDate` phai co date hop le theo validator hien tai.

### Mistake 2: Gui `status`

`status` trong create request bi ignore. Backend set lop moi la `Active`.

### Mistake 3: Thieu `levelId` hoac `startModuleId`

Backend require ca 2 field nay. `startModuleId` nen la module dau tien cua level, hoac module admin chon de bat dau lop.

### Mistake 4: DayOfWeek dung label UI

Sai:

```json
{ "dayOfWeek": "Thứ 3", "startTime": "18:00", "durationMinutes": 120 }
```

Dung:

```json
{ "dayOfWeek": "TU", "startTime": "18:00", "durationMinutes": 120 }
```

## 7. Error Response

Validation/model binding error co dang:

```json
{
  "success": false,
  "isSuccess": false,
  "data": null,
  "message": "One or more validation errors occurred"
}
```

Business error thuong co dang Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Class.CodeExists",
  "status": 409,
  "detail": "Class code already exists"
}
```

Common errors:

| HTTP | Code / message | Nguyen nhan |
| --- | --- | --- |
| `400` | `Branch ID is required` | `branchId` empty/missing |
| `400` | `Program ID is required` | `programId` empty/missing |
| `400` | `Level ID is required` | `levelId` empty/missing |
| `400` | `Start module ID is required` | `startModuleId` empty/missing |
| `400` | `Class code is required` | `code` empty |
| `400` | `Class title is required` | `name`, `title`, va `code` fallback deu rong |
| `400` | `End date is required when weekly schedule is provided` | Co `weeklyScheduleSlots` nhung `endDate` null/missing |
| `400` | `End date must be greater than or equal to start date` | `endDate < startDate` |
| `400` | `Capacity must be greater than 0` | `capacity <= 0` |
| `400` | `SchedulePattern.InvalidDayOfWeek` | `dayOfWeek` khong phai `MO..SU` |
| `400` | `SchedulePattern.InvalidStartTime` | `startTime` khong dung format |
| `400` | `SchedulePattern.InvalidDuration` | `durationMinutes <= 0` |
| `400` | `SchedulePattern.DuplicateSlot` | Trung ngay + gio |
| `404` | `Class.BranchNotFound` | Branch inactive/not found |
| `404` | `Class.ProgramNotFound` | Program inactive/not found/deleted |
| `404` | `Class.LevelNotFound` | Level not found |
| `400` | `Class.LevelProgramMismatch` | Level khong thuoc program |
| `404` | `Class.StartModuleNotFound` | Start module not found |
| `400` | `Class.StartModuleLevelMismatch` | Start module khong thuoc level |
| `404` | `Class.RoomNotFound` | Room inactive/not found |
| `409` | `Class.RoomBranchMismatch` | Room khong thuoc branch |
| `404` | `Class.MainTeacherNotFound` | Main teacher invalid |
| `409` | `Class.MainTeacherBranchMismatch` | Main teacher khong thuoc branch |
| `404` | `Class.AssistantTeacherNotFound` | Assistant teacher invalid |
| `409` | `Class.AssistantTeacherBranchMismatch` | Assistant teacher khong thuoc branch |
| `400` | `Class.TeacherAndAssistantMustDiffer` | Main va assistant cung user |
| `400` | `Class.SlotTypeNotFound` | Slot type inactive/not found |
| `409` | `Class.CodeExists` | Code class da ton tai |
| `409` | `Class.RoomConflict` | Room trung lich |
| `409` | `Class.TeacherConflict` | Main teacher trung lich |
| `409` | `Class.AssistantConflict` | Assistant teacher trung lich |

## 8. FE Payload Builder

```ts
type ScheduleSlot = {
  dayOfWeek: "MO" | "TU" | "WE" | "TH" | "FR" | "SA" | "SU";
  startTime: string;
  durationMinutes: number;
};

type CreateClassForm = {
  branchId: string;
  programId: string;
  levelId: string;
  startModuleId: string;
  code: string;
  title: string;
  roomId?: string | null;
  mainTeacherId?: string | null;
  assistantTeacherId?: string | null;
  slotTypeId?: string | null;
  startDate: string;
  endDate?: string | null;
  capacity: number;
  weeklyScheduleSlots: ScheduleSlot[];
  description?: string | null;
};

function emptyToNull(value?: string | null) {
  return value && value.trim() !== "" ? value : null;
}

function buildCreateClassPayload(form: CreateClassForm) {
  return {
    branchId: form.branchId,
    programId: form.programId,
    levelId: form.levelId,
    startModuleId: form.startModuleId,
    code: form.code.trim(),
    title: form.title.trim(),
    roomId: emptyToNull(form.roomId),
    mainTeacherId: emptyToNull(form.mainTeacherId),
    assistantTeacherId: emptyToNull(form.assistantTeacherId),
    slotTypeId: emptyToNull(form.slotTypeId),
    startDate: form.startDate,
    endDate: emptyToNull(form.endDate),
    capacity: form.capacity,
    weeklyScheduleSlots: form.weeklyScheduleSlots,
    description: emptyToNull(form.description)
  };
}
```

FE pre-submit guard:

```ts
function validateBeforeSubmit(payload: ReturnType<typeof buildCreateClassPayload>) {
  if (payload.weeklyScheduleSlots.length > 0 && !payload.endDate) {
    throw new Error("Vui long chon ngay ket thuc lop khi da chon lich hoc.");
  }
}
```

## 9. Example Matching Current UI

Payload user dang co can sua `endDate` tu `""` thanh date hop le:

```json
{
  "branchId": "da316382-35e8-4094-a99b-ce45e5f2627a",
  "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
  "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
  "startModuleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
  "code": "S1",
  "title": "Starters 1",
  "description": "Lop Starter S1",
  "startDate": "2026-05-21",
  "endDate": "2026-08-21",
  "capacity": 10,
  "roomId": "44444444-4444-4444-4444-444444444304",
  "mainTeacherId": "3a32f148-d21c-4a01-b346-cdc31df755b2",
  "assistantTeacherId": null,
  "slotTypeId": "0c6018e6-5b70-4a3e-ba89-ca6d3f2a577e",
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "TU",
      "startTime": "18:00",
      "durationMinutes": 120
    },
    {
      "dayOfWeek": "TH",
      "startTime": "18:00",
      "durationMinutes": 120
    }
  ]
}
```
