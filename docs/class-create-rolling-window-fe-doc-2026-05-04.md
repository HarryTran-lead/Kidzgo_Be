# Flow Tao Class Rolling Window - FE Doc - 2026-05-04

Tai lieu nay mo ta flow tao class theo co che rolling window sinh session. Rolling window trong BE hien tai la `8 tuan`: moi lan generate, BE chi tao session trong khoang toi da 8 tuan tinh tu ngay bat dau generate, tranh tao lich vo han cho class khong co ngay ket thuc.

## 1. Khai niem FE can nam

Rolling window ap dung cho class co lich hoc lap lai hang tuan va `endDate = null`.

- `startDate`: ngay class bat dau.
- `endDate`: de `null` de class chay theo rolling window.
- `weeklyScheduleSlots`: lich hoc tuan, vi du thu 2 va thu 4 luc 18:00.
- Session generation window: toi da 8 tuan moi lan sinh session.
- Quartz job `MaintainRollingSessionWindowJob` se dinh ky goi lai generator de bo sung session moi trong cua so 8 tuan tiep theo.

Ghi chu quan trong:

- API tao class `POST /api/classes` chi tao class, khong tu dong tra ve `createdSessionsCount`.
- De sinh session ngay sau khi tao class, FE goi them `POST /api/sessions/generate-from-pattern`.
- BE generator tu bo qua session da ton tai, nen goi lai generate nhieu lan khong tao trung session cung thoi diem.
- Session roi vao holiday active se bi skip. Neu class khong co `endDate`, BE co co che sinh bu them slot sau rolling window de bu so session bi skip do holiday.

## 2. Caveat theo code hien tai

Trong `CreateClassCommandValidator` va `UpdateClassCommandValidator` hien tai dang co rule:

```text
End date is required when weekly schedule is provided
```

Rule nay mau thuan voi flow rolling window vi rolling window can `endDate = null` va van can `weeklyScheduleSlots`. Neu FE gui payload rolling window ma gap loi validation tren, BE can sua validator truoc:

- Cho phep `endDate = null` khi `weeklyScheduleSlots` co du lieu.
- Chi validate `endDate >= startDate` khi FE that su gui `endDate`.

Doc ben duoi mo ta contract FE nen dung cho rolling window sau khi BE cho phep `endDate = null`.

## 3. Flow tong quat

1. FE cho Admin/ManagementStaff nhap thong tin class.
2. FE chon mode `Rolling window` thay vi `Fixed end date`.
3. FE gui `endDate: null`.
4. FE gui `weeklyScheduleSlots` gom day/time/duration.
5. BE tao class status `Active`.
6. FE goi generate session cho class vua tao.
7. BE sinh session trong 8 tuan tinh tu `startDate` hoac ngay hien tai, tuy `onlyFutureSessions`.
8. Quartz job tiep tuc maintain rolling window ve sau.

## 4. API tao class

### POST `/api/classes`

Roles:

- `Admin`
- `ManagementStaff`

Headers:

```http
Content-Type: application/json
Authorization: Bearer <accessToken>
```

Request payload rolling window:

```json
{
  "branchId": "0fdc3c81-9824-4d9c-a647-6ed5e49b4a20",
  "programId": "9c0cfa62-9f5f-48cc-93aa-447d216f8883",
  "code": "KIDS-RW-001",
  "name": "Kids Rolling Window 001",
  "title": "Kids Rolling Window 001",
  "roomId": "835e2452-cc7e-4b66-81d0-819905785a45",
  "mainTeacherId": "2d59b730-936a-4a75-8855-71eb3cd38428",
  "assistantTeacherId": null,
  "startDate": "2026-05-04",
  "endDate": null,
  "capacity": 12,
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "Monday",
      "startTime": "18:00",
      "durationMinutes": 90
    },
    {
      "dayOfWeek": "Wednesday",
      "startTime": "18:00",
      "durationMinutes": 90
    }
  ],
  "description": "Class rolling window, sessions maintained 8 weeks ahead."
}
```

Field notes:

| Field | Type | Required | Rolling window note |
| --- | --- | --- | --- |
| `branchId` | `guid` | Yes | Branch active |
| `programId` | `guid` | Yes | Program active, not deleted |
| `code` | `string` | Yes | Unique, max 50 |
| `name` / `title` | `string` | Yes | BE dung `name ?? title ?? code` |
| `roomId` | `guid?` | No | Neu co, room phai active va cung branch |
| `mainTeacherId` | `guid?` | No | Neu co, user phai role `Teacher`, active, cung branch |
| `assistantTeacherId` | `guid?` | No | Khac `mainTeacherId`, role `Teacher`, active, cung branch |
| `startDate` | `date` | Yes | Ngay bat dau class |
| `endDate` | `date?` | Rolling mode: `null` | `null` de dung rolling window |
| `capacity` | `number` | Yes | `> 0` |
| `weeklyScheduleSlots` | `array` | Yes | Lich lap lai hang tuan |
| `description` | `string?` | No | Ghi chu |

`weeklyScheduleSlots`:

| Field | Type | Required | Rule |
| --- | --- | --- | --- |
| `dayOfWeek` | `string` | Yes | Ten thu hop le, vi du `Monday`, `Tuesday`, `Wednesday` |
| `startTime` | `string` | Yes | Format `HH:mm` |
| `durationMinutes` | `number` | Yes | `> 0` |

Success response `201 Created`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "384d9d4f-612a-49ea-92de-2fa7ea2f450c",
    "branchId": "0fdc3c81-9824-4d9c-a647-6ed5e49b4a20",
    "programId": "9c0cfa62-9f5f-48cc-93aa-447d216f8883",
    "code": "KIDS-RW-001",
    "title": "Kids Rolling Window 001",
    "roomId": "835e2452-cc7e-4b66-81d0-819905785a45",
    "mainTeacherId": "2d59b730-936a-4a75-8855-71eb3cd38428",
    "assistantTeacherId": null,
    "startDate": "2026-05-04",
    "endDate": null,
    "status": "Active",
    "capacity": 12,
    "weeklyScheduleSlots": [
      {
        "dayOfWeek": "Monday",
        "startTime": "18:00",
        "durationMinutes": 90
      },
      {
        "dayOfWeek": "Wednesday",
        "startTime": "18:00",
        "durationMinutes": 90
      }
    ],
    "description": "Class rolling window, sessions maintained 8 weeks ahead.",
    "name": "Kids Rolling Window 001",
    "scheduleText": "Monday 18:00 (90m), Wednesday 18:00 (90m)"
  }
}
```

## 5. API generate sessions sau khi tao class

### POST `/api/sessions/generate-from-pattern`

Muc dich: sinh sessions tu `weeklyScheduleSlots` cua class.

Roles:

- `Admin`
- `ManagementStaff`

Request:

```json
{
  "classId": "384d9d4f-612a-49ea-92de-2fa7ea2f450c",
  "onlyFutureSessions": true
}
```

Field notes:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `classId` | `guid` | Yes | Class vua tao |
| `onlyFutureSessions` | `bool` | No | Default `true`; chi sinh session tu hien tai tro di |

Rolling window behavior:

- Neu `onlyFutureSessions = true` va hom nay sau `startDate`, BE bat dau generate tu hom nay.
- Neu `onlyFutureSessions = false`, BE generate tu `class.startDate`.
- `generationEnd = min(endDate, generationStart + 8 tuan - 1 ngay)`.
- Voi rolling class `endDate = null`, `generationEnd = generationStart + 55 ngay`.

Success response `200 OK`:

```json
{
  "isSuccess": true,
  "data": {
    "createdSessionsCount": 16
  }
}
```

`createdSessionsCount = 0` la hop le neu:

- Session trong window da ton tai.
- Lich hoc khong co occurrence trong window.
- Tat ca occurrence bi skip vi qua khu khi `onlyFutureSessions = true`.

## 6. Quartz maintain rolling window

Job: `MaintainRollingSessionWindowJob`.

Job se lay cac class co status:

- `Planned`
- `Recruiting`
- `Active`

Sau do goi `GenerateSessionsFromPatternAsync(class, onlyFutureSessions: true)`.

Cron:

- Neu config `Quartz:Schedules:MaintainRollingSessionWindowJob` co gia tri thi dung config do.
- Neu khong co config, fallback trong code la `0 0 18 * * ?` (18:00 moi ngay).

FE khong can goi job nay. FE chi can goi generate ngay sau khi tao class neu muon nguoi dung thay session lap tuc.

## 7. Error response thuong gap

### Validation format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "NotEmptyValidator",
      "description": "Class code is required"
    }
  ]
}
```

### Create class errors

| HTTP | Code/title | Message BE | FE message goi y |
| --- | --- | --- | --- |
| `400` | `Validation.General` | `End date is required when weekly schedule is provided` | BE chua ho tro rolling payload `endDate = null`; can cap nhat validator |
| `400` | `SchedulePattern.Empty` | `Schedule pattern cannot be empty` | Vui long chon lich hoc trong tuan |
| `400` | `SchedulePattern.InvalidDayOfWeek` | Invalid day | Ngay hoc khong hop le |
| `400` | `SchedulePattern.InvalidStartTime` | Invalid start time | Gio bat dau khong hop le |
| `400` | `SchedulePattern.InvalidDuration` | Invalid duration | Thoi luong buoi hoc phai lon hon 0 |
| `400` | `SchedulePattern.DuplicateSlot` | Duplicate slot | Lich hoc bi trung ngay/gio |
| `400` | `Class.TeacherAndAssistantMustDiffer` | `Main teacher and assistant teacher must be different users` | Giao vien chinh va tro giang phai khac nhau |
| `404` | `Class.BranchNotFound` | `Branch not found or inactive` | Chi nhanh khong ton tai hoac ngung hoat dong |
| `404` | `Class.ProgramNotFound` | `Program not found, deleted, or inactive` | Chuong trinh khong ton tai hoac ngung hoat dong |
| `404` | `Class.RoomNotFound` | `Room not found or inactive` | Phong hoc khong ton tai hoac ngung hoat dong |
| `404` | `Class.MainTeacherNotFound` | `Main teacher not found or is not a teacher` | Giao vien chinh khong hop le |
| `404` | `Class.AssistantTeacherNotFound` | `Assistant teacher not found or is not a teacher` | Tro giang khong hop le |
| `409` | `Class.CodeExists` | `Class code already exists` | Ma lop da ton tai |
| `409` | `Class.RoomBranchMismatch` | `Room must belong to the same branch as the class` | Phong hoc khong thuoc chi nhanh da chon |
| `409` | `Class.MainTeacherBranchMismatch` | `Main teacher must belong to the same branch as the class` | Giao vien chinh khong thuoc chi nhanh da chon |
| `409` | `Class.AssistantTeacherBranchMismatch` | `Assistant teacher must belong to the same branch as the class` | Tro giang khong thuoc chi nhanh da chon |
| `409` | `Class.RoomConflict` | Room booked by another class | Phong hoc da co lich trung |
| `409` | `Class.TeacherConflict` | Teacher assigned to another class | Giao vien da co lich trung |
| `409` | `Class.AssistantConflict` | Assistant assigned to another class | Tro giang da co lich trung |

### Generate session errors

| HTTP | Code/title | FE message goi y |
| --- | --- | --- |
| `404` | `Class.NotFound` | Khong tim thay lop |
| `400` | `Session.InvalidClassStatus` | Chi co the sinh session cho lop Planned, Recruiting hoac Active |
| `400` | `Session.MissingSchedulePattern` | Lop chua co lich hoc tuan |
| `400` | `Session.InvalidBranch` | Chi nhanh cua lop khong hop le |
| `400` | `Session.InvalidRoom` | Phong hoc cua lop khong hop le |
| `400` | `Session.InvalidTeacher` | Giao vien chinh khong hop le |
| `400` | `Session.InvalidAssistant` | Tro giang khong hop le |
| `400` | `Session.SaveFailed` | Khong luu duoc sessions |
| `409` | `Class.RoomConflict` | Phong hoc da co lich trung trong window |
| `409` | `Class.TeacherConflict` | Giao vien da co lich trung trong window |
| `409` | `Class.AssistantConflict` | Tro giang da co lich trung trong window |

## 8. FE implementation goi y

Payload builder:

```ts
type ScheduleSlot = {
  dayOfWeek: string;
  startTime: string;
  durationMinutes: number;
};

function buildRollingClassPayload(input: {
  branchId: string;
  programId: string;
  code: string;
  title: string;
  roomId?: string | null;
  mainTeacherId?: string | null;
  assistantTeacherId?: string | null;
  startDate: string;
  capacity: number;
  weeklyScheduleSlots: ScheduleSlot[];
  description?: string | null;
}) {
  return {
    ...input,
    name: input.title,
    endDate: null
  };
}
```

Create + generate:

```ts
async function createRollingWindowClass(payload: unknown) {
  const createRes = await fetch("/api/classes", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify(payload)
  });

  const createBody = await createRes.json();
  if (!createRes.ok) {
    throw new Error(createBody.detail ?? createBody.title ?? "Create class failed");
  }

  const classId = createBody.data.id;

  const generateRes = await fetch("/api/sessions/generate-from-pattern", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify({
      classId,
      onlyFutureSessions: true
    })
  });

  const generateBody = await generateRes.json();
  if (!generateRes.ok) {
    throw new Error(generateBody.detail ?? generateBody.title ?? "Generate sessions failed");
  }

  return {
    class: createBody.data,
    createdSessionsCount: generateBody.data.createdSessionsCount
  };
}
```

UI copy goi y:

- Mode fixed date: FE bat buoc nhap `endDate`.
- Mode rolling window: FE an/disable `endDate`, gui `endDate: null`.
- Sau khi tao thanh cong: hien `Da tao lop va sinh {createdSessionsCount} buoi trong 8 tuan dau.`
- Neu generate tra `createdSessionsCount = 0`: hien `Lop da co du lich trong rolling window hien tai hoac khong co buoi moi can sinh.`
