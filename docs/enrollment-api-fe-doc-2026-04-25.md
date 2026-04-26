# Tai Lieu API FE - Enrollment - 2026-04-25

Tai lieu nay tong hop cac API trong `EnrollmentController.cs` de FE theo doi luong enrollment hien tai.

Pham vi tai lieu:

- Tao enrollment
- Backfill student session assignments
- Xem danh sach / chi tiet enrollment
- Cap nhat enrollment
- Pause / drop / reactivate enrollment
- Assign tuition plan
- Xem lich su enrollment cua hoc sinh
- Add enrollment schedule segment cho supplementary enrollment

## Tong quan role va pham vi du lieu

Tat ca API trong controller khong dong nhat ve authorize.

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Tat ca enrollment | `all` | `view`, `create`, `edit`, `pause`, `drop`, `reactivate`, `assign_tuition_plan`, `backfill`, `manage_schedule_segment` |
| ManagementStaff | Tat ca enrollment | `all` | `view`, `create`, `edit`, `pause`, `drop`, `reactivate`, `assign_tuition_plan`, `backfill`, `manage_schedule_segment` |
| Teacher | Hien tai khong duoc authorize o cac API mutation; 2 API GET list/detail dang mo o level controller | `all` neu endpoint khong co auth | `view` tren 2 API GET neu deployment khong co fallback auth global |
| Parent | Tuong tu Teacher, chi co the xem 2 API GET dang mo o level controller | `all` neu endpoint khong co auth | `view` tren 2 API GET dang mo |
| Student | Tuong tu Teacher, chi co the xem 2 API GET dang mo o level controller | `all` neu endpoint khong co auth | `view` tren 2 API GET dang mo |
| Anonymous | Tuong tu Teacher, chi co the xem 2 API GET dang mo o level controller | `all` neu endpoint khong co auth | `view` tren 2 API GET dang mo |

Ghi chu quan trong:

- `GET /api/enrollments`
- `GET /api/enrollments/{id}`

trong code hien tai dang **comment out `[Authorize]`**. Nghia la o cap controller, 2 API nay dang mo. Neu he thong co fallback auth global thi runtime co the chat hon, nhung theo code controller hien tai thi 2 endpoint nay chua chot role.

## Dinh dang response chung

Success tu `MatchOk()` / `MatchCreated()`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error tu domain result:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Enrollment.ClassFull",
  "status": 409,
  "detail": "Class has reached its capacity"
}
```

Validation pipeline co the tra them `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "WeeklyPattern.InvalidDuration",
      "description": "weeklyPattern durationMinutes must be greater than 0."
    }
  ]
}
```

## Kieu du lieu chung

### `EnrollmentStatus`

| Status | Y nghia |
| --- | --- |
| `Active` | Enrollment dang co hieu luc |
| `Paused` | Enrollment tam dung |
| `Dropped` | Enrollment da dung hieu luc / da drop |

### `weeklyPattern`

`weeklyPattern` la subset lich hoc cua class ma hoc vien se tham gia.

Vi du:

```json
[
  {
    "dayOfWeeks": ["MO", "WE"],
    "startTime": "18:00",
    "durationMinutes": 90
  }
]
```

Rule:

- `dayOfWeeks`: `MO`, `TU`, `WE`, `TH`, `FR`, `SA`, `SU`
- `startTime`: `HH:mm`
- `durationMinutes` > 0
- phai la subset cua lich class
- neu bo qua `weeklyPattern` thi hoc vien hoc toan bo lich class

## Danh sach API

### 1. POST `/api/enrollments`

Dung de tao enrollment moi cho hoc sinh vao class.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "classId": "guid",
  "studentProfileId": "guid",
  "enrollDate": "2026-04-25",
  "tuitionPlanId": "guid",
  "track": "primary",
  "weeklyPattern": [
    {
      "dayOfWeeks": ["MO"],
      "startTime": "18:00",
      "durationMinutes": 90
    }
  ]
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `classId` | `Guid` | Yes | Lop can ghi danh |
| `studentProfileId` | `Guid` | Yes | Profile hoc sinh |
| `enrollDate` | `DateOnly` | Yes | Ngay co hieu luc enrollment |
| `tuitionPlanId` | `Guid?` | No | Goi hoc gan cho enrollment |
| `track` | `string?` | No | `primary` / `secondary`; neu null backend normalize theo helper |
| `weeklyPattern` | `array<WeeklyPatternEntry>?` | No | Subset lich hoc cua class |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | Validation pipeline | Thieu `classId`, `studentProfileId`, `enrollDate` |
| 404 | `Enrollment.ClassNotFound` | Lop khong ton tai |
| 409 | `Enrollment.ClassNotAvailable` | Lop khong o `Active` hoac `Planned` |
| 404 | `Enrollment.StudentNotFound` | Student profile khong ton tai / khong active / khong phai student |
| 409 | `Enrollment.AlreadyEnrolled` | Hoc sinh da co enrollment `Active` hoac `Paused` trong lop nay |
| 409 | `Enrollment.ClassFull` | Lop da full |
| 404 | `Enrollment.TuitionPlanNotFound` | Tuition plan khong ton tai |
| 409 | `Enrollment.TuitionPlanNotAvailable` | Tuition plan inactive/deleted |
| 409 | `Enrollment.TuitionPlanBranchMismatch` | Tuition plan khac branch cua class |
| 409 | `Enrollment.TuitionPlanProgramMismatch` | Tuition plan khac program cua class |
| 400/409 | `Enrollment.SessionSelectionPatternInvalid` / `Empty` / `Mismatch` | `weeklyPattern` khong hop le / khong phai subset cua lich class |
| 409 | `Enrollment.StudentScheduleConflict` | Trung lich voi buoi/lop khac |

Ghi chu:

- Neu class la supplementary program, backend se tao `ClassEnrollmentScheduleSegment` dau tien tu `EnrollDate`.
- Sau khi tao enrollment, backend goi sync de tao `StudentSessionAssignment`.

### 2. POST `/api/enrollments/backfill-session-assignments`

Dung de quet lai enrollments va tao/reactivate/cancel `StudentSessionAssignment` theo session hien co.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "enrollmentId": "guid",
  "classId": "guid",
  "studentProfileId": "guid",
  "batchSize": 100
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `enrollmentId` | `Guid?` | No | Loc theo enrollment cu the |
| `classId` | `Guid?` | No | Loc theo class |
| `studentProfileId` | `Guid?` | No | Loc theo student |
| `batchSize` | `int?` | No | So enrollment xu ly moi batch; default 100, max 500 |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "matchedEnrollments": 5,
    "processedEnrollments": 5,
    "affectedClasses": 2,
    "batchSize": 100,
    "createdAssignments": 20,
    "reactivatedAssignments": 3,
    "cancelledAssignments": 1
  }
}
```

Response loi:

- 401 Unauthorized
- 403 Forbidden

Ghi chu:

- API nay chi xu ly enrollment co status `Active` hoac `Paused`.
- Neu khong match enrollment nao, response success van tra ve so dem = 0.

### 3. POST `/api/enrollments/{id}/schedule-segments`

Dung de them enrollment schedule segment theo ngay hieu luc cho supplementary enrollment.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "weeklyPattern": [
    {
      "dayOfWeeks": ["SA"],
      "startTime": "17:00",
      "durationMinutes": 60
    }
  ],
  "clearWeeklyPattern": false
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `effectiveFrom` | `DateOnly` | Yes | Ngay bat dau segment |
| `effectiveTo` | `DateOnly?` | No | Ngay ket thuc segment |
| `weeklyPattern` | `array<WeeklyPatternEntry>?` | No | Lich hoc moi cua hoc vien trong segment |
| `clearWeeklyPattern` | `bool` | No | `true` = bo pattern rieng, quay ve hoc full lich class |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "enrollmentId": "guid",
    "classId": "guid",
    "programId": "guid",
    "effectiveFrom": "2026-06-01",
    "effectiveTo": null,
    "weeklyPattern": [
      {
        "dayOfWeeks": ["SA"],
        "startTime": "17:00",
        "durationMinutes": 60
      }
    ],
    "activeWeeklyPattern": [
      {
        "dayOfWeeks": ["SA"],
        "startTime": "17:00",
        "durationMinutes": 60
      }
    ]
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |
| 400 | `Enrollment.SupplementaryProgramRequired` | Chi supplementary enrollment moi duoc them segment |
| 409 | `Enrollment.AlreadyDropped` | Enrollment da dropped |
| 400 | `Enrollment.ScheduleSegmentInvalidEffectiveDate` | Effective range khong hop le |
| 409 | `Enrollment.ScheduleSegmentAlreadyExists` | Da co segment cung `effectiveFrom` |
| 409 | `Enrollment.FutureScheduleSegmentExists` | Da co future segment muon hon |
| 400/409 | `Enrollment.SessionSelectionPatternInvalid` / `Empty` / `Mismatch` | `weeklyPattern` khong hop le |
| 409 | `Enrollment.StudentScheduleConflict` | Trung lich tu ngay segment co hieu luc |

### 4. GET `/api/enrollments`

Dung de lay danh sach enrollment.

Roles theo controller hien tai:

- `[Authorize]` dang comment out
- ve mat code controller, endpoint dang mo

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `classId` | `Guid?` | No | null | Loc theo lop |
| `studentProfileId` | `Guid?` | No | null | Loc theo hoc sinh |
| `status` | `string?` | No | null | `Active`, `Paused`, `Dropped` |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "enrollments": {
      "items": [
        {
          "id": "guid",
          "classId": "guid",
          "classCode": "APPLE-A2",
          "classTitle": "Apple A2",
          "studentProfileId": "guid",
          "studentName": "Nguyen Van A",
          "enrollDate": "2026-04-25",
          "status": "Active",
          "tuitionPlanId": "guid",
          "tuitionPlanName": "Goi 48 buoi"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

Response loi:

- Khong co validation error dac thu; `status` query parse khong duoc thi bo qua filter
- Neu deployment khong co fallback auth global, endpoint hien tai co the truy cap duoc boi anonymous

### 5. GET `/api/enrollments/{id}`

Dung de lay chi tiet enrollment.

Roles theo controller hien tai:

- `[Authorize]` dang comment out
- ve mat code controller, endpoint dang mo

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Enrollment id |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "programId": "guid",
    "programName": "Apple",
    "branchId": "guid",
    "branchName": "HCM",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "weeklyPattern": [
      {
        "dayOfWeeks": ["MO"],
        "startTime": "18:00",
        "durationMinutes": 90
      }
    ],
    "scheduleSegments": [],
    "createdAt": "2026-04-25T09:00:00Z",
    "updatedAt": "2026-04-25T09:00:00Z"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |

### 6. PUT `/api/enrollments/{id}`

Dung de cap nhat thong tin enrollment.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "enrollDate": "2026-04-25",
  "tuitionPlanId": "guid",
  "track": "primary",
  "weeklyPattern": [
    {
      "dayOfWeeks": ["MO"],
      "startTime": "18:00",
      "durationMinutes": 90
    }
  ],
  "clearWeeklyPattern": false
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `enrollDate` | `DateOnly?` | No | Doi ngay co hieu luc |
| `tuitionPlanId` | `Guid?` | No | Doi tuition plan |
| `track` | `string?` | No | `primary` / `secondary` |
| `weeklyPattern` | `array<WeeklyPatternEntry>?` | No | Lich hoc rieng cua hoc vien |
| `clearWeeklyPattern` | `bool` | No | `true` = bo lich hoc rieng |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | Validation pipeline | Thieu `id` trong command |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |
| 404 | `Enrollment.TuitionPlanNotFound` | Tuition plan khong ton tai |
| 409 | `Enrollment.TuitionPlanNotAvailable` | Tuition plan inactive/deleted |
| 409 | `Enrollment.TuitionPlanProgramMismatch` | Khac program |
| 400/409 | `Enrollment.SessionSelectionPatternInvalid` / `Empty` / `Mismatch` | `weeklyPattern` khong hop le |
| 409 | `Enrollment.StudentScheduleConflict` | Trung lich sau update |

Ghi chu:

- Neu la supplementary enrollment, update `weeklyPattern` co the tu dong cat segment dang active va tao segment moi tu ngay hieu luc.
- Sau update, backend sync lai `StudentSessionAssignment`.

### 7. PATCH `/api/enrollments/{id}/pause`

Dung de chuyen enrollment sang `Paused`.

Roles: `Admin`, `ManagementStaff`

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Paused",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |
| 409 | `Enrollment.InvalidStatus` | Enrollment khong o `Active` |

Ghi chu:

- Backend cancel future assignments cua enrollment tu thoi diem hien tai tro di.

### 8. PATCH `/api/enrollments/{id}/drop`

Dung de drop enrollment.

Roles: `Admin`, `ManagementStaff`

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Dropped",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |
| 409 | `Enrollment.AlreadyDropped` | Enrollment da `Dropped` |

Ghi chu:

- Backend cancel future assignments cua enrollment.
- Backend sync lai availability status cua class.

### 9. PATCH `/api/enrollments/{id}/reactivate`

Dung de kich hoat lai enrollment tu `Paused` ve `Active`.

Roles: `Admin`, `ManagementStaff`

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |
| 409 | `Enrollment.AlreadyActive` | Enrollment da active |
| 409 | `Enrollment.CannotReactivateDropped` | Enrollment da dropped |
| 409 | `Enrollment.ClassNotAvailable` | Class khong o `Active`, `Planned`, `Recruiting` |
| 409 | `Enrollment.ClassFull` | Lop da full |
| 409 | `Enrollment.StudentScheduleConflict` | Trung lich khi reactivate |

Ghi chu:

- Reactivate se sync lai `StudentSessionAssignment`.

### 10. PATCH `/api/enrollments/{id}/assign-tuition-plan`

Dung de gan / doi tuition plan cho enrollment.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "tuitionPlanId": "guid"
}
```

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollDate": "2026-04-25",
    "status": "Active",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi moi"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | Validation pipeline | Thieu `id` hoac `tuitionPlanId` |
| 404 | `Enrollment.NotFound` | Enrollment khong ton tai |
| 404 | `Enrollment.TuitionPlanNotFound` | Tuition plan khong ton tai |
| 409 | `Enrollment.TuitionPlanNotAvailable` | Tuition plan inactive/deleted |
| 409 | `Enrollment.TuitionPlanBranchMismatch` | Khac branch cua class |
| 409 | `Enrollment.TuitionPlanProgramMismatch` | Khac program cua class |

### 11. GET `/api/enrollments/student/{studentProfileId}/history`

Dung de lay lich su enrollment cua mot hoc sinh.

Roles: `Admin`, `ManagementStaff`

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Hoc sinh can xem lich su |

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "enrollments": {
      "items": [
        {
          "id": "guid",
          "classId": "guid",
          "classCode": "APPLE-A2",
          "classTitle": "Apple A2",
          "programId": "guid",
          "programName": "Apple",
          "branchId": "guid",
          "branchName": "HCM",
          "enrollDate": "2026-04-25",
          "status": "Active",
          "tuitionPlanId": "guid",
          "tuitionPlanName": "Goi 48 buoi",
          "createdAt": "2026-04-25T09:00:00Z",
          "updatedAt": "2026-04-25T10:00:00Z"
        }
      ],
      "pageNumber": 1,
      "totalPages": 1,
      "totalCount": 1
    }
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Enrollment.StudentNotFound` | Student profile khong ton tai / khong phai student |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

## Status definition

### EnrollmentStatus

| Status | Y nghia |
| --- | --- |
| `Active` | Hoc sinh dang duoc ghi danh va co the duoc xep assignment cho cac session hop le |
| `Paused` | Enrollment tam dung, future assignments bi cancel |
| `Dropped` | Enrollment da ket thuc, khong duoc reactivate bang flow thuong |

## Luong chuyen trang thai

Luong chinh:

1. Tao enrollment moi -> `Active`
2. `PATCH /pause`:
   - `Active -> Paused`
3. `PATCH /reactivate`:
   - `Paused -> Active`
4. `PATCH /drop`:
   - `Active -> Dropped`
   - `Paused -> Dropped`
5. `Dropped` la terminal trong controller hien tai:
   - khong duoc `reactivate`

Khong doi status:

- `PUT /api/enrollments/{id}`
- `PATCH /assign-tuition-plan`
- `POST /schedule-segments`
- `POST /backfill-session-assignments`

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/enrollments` | Yes | Yes | No | No | No | No |
| `POST /api/enrollments/backfill-session-assignments` | Yes | Yes | No | No | No | No |
| `POST /api/enrollments/{id}/schedule-segments` | Yes | Yes | No | No | No | No |
| `GET /api/enrollments` | Yes | Yes | Open by current controller code | Open by current controller code | Open by current controller code | Open by current controller code |
| `GET /api/enrollments/{id}` | Yes | Yes | Open by current controller code | Open by current controller code | Open by current controller code | Open by current controller code |
| `PUT /api/enrollments/{id}` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/pause` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/drop` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/reactivate` | Yes | Yes | No | No | No | No |
| `PATCH /api/enrollments/{id}/assign-tuition-plan` | Yes | Yes | No | No | No | No |
| `GET /api/enrollments/student/{studentProfileId}/history` | Yes | Yes | No | No | No | No |

## Validation rule tong hop

| Rule | API ap dung | Ket qua khi sai |
| --- | --- | --- |
| User phai dang nhap | Tat ca API co `[Authorize]` | 401 |
| Role phai la `Admin`, `ManagementStaff` | Tat ca API mutation va history | 403 |
| `GET /api/enrollments`, `GET /api/enrollments/{id}` hien tai chua gắn `[Authorize]` | 2 API GET | Theo controller code hien tai dang mo |
| `classId`, `studentProfileId`, `enrollDate` bat buoc | Create enrollment | 400 validation pipeline |
| Class phai ton tai | Create | 404 `Enrollment.ClassNotFound` |
| Class phai o `Active` hoac `Planned` | Create | 409 `Enrollment.ClassNotAvailable` |
| Student profile phai la student active | Create, history | 404 `Enrollment.StudentNotFound` |
| Khong duoc tao enrollment trung class cho hoc sinh dang `Active`/`Paused` | Create | 409 `Enrollment.AlreadyEnrolled` |
| Lop khong duoc full | Create, reactivate | 409 `Enrollment.ClassFull` |
| Tuition plan phai ton tai, active, dung program/branch | Create, update, assign-tuition-plan | 404/409 |
| `weeklyPattern` phai hop le va la subset cua lich class | Create, update, add schedule segment | 400/409 `Enrollment.SessionSelectionPattern...` |
| Hoc sinh khong duoc trung lich | Create, update, add schedule segment, reactivate | 409 `Enrollment.StudentScheduleConflict` |
| Chi supplementary enrollment moi co schedule segment | Add schedule segment | 400 `Enrollment.SupplementaryProgramRequired` |
| `effectiveFrom/effectiveTo` phai hop le trong khoang enrollment/class | Add schedule segment | 400 `Enrollment.ScheduleSegmentInvalidEffectiveDate` |
| Khong duoc add segment trung ngay bat dau hoac chen vao sau future segment da co | Add schedule segment | 409 `Enrollment.ScheduleSegmentAlreadyExists` / `FutureScheduleSegmentExists` |
| Chi enrollment `Active` moi duoc pause | Pause | 409 `Enrollment.InvalidStatus` |
| Enrollment `Dropped` khong duoc reactivate | Reactivate | 409 `Enrollment.CannotReactivateDropped` |
| Enrollment `Active` khong duoc reactivate tiep | Reactivate | 409 `Enrollment.AlreadyActive` |
| `batchSize` backfill neu null/<=0 thi mac dinh 100; >500 thi bi cat ve 500 | Backfill | Khong tra loi, backend normalize |

## Luu y FE quan trong

- `weeklyPattern` la API public; backend van luu noi bo duoi ten `SessionSelectionPattern`.
- `GET /api/enrollments/{id}` tra them `weeklyPattern` va `scheduleSegments`, phu hop de FE render lich hoc thuc te cua hoc vien.
- `POST /api/enrollments/{id}/schedule-segments` chi dung cho supplementary enrollment.
- `pause`, `drop`, `reactivate` co anh huong truc tiep toi `StudentSessionAssignment`.
- `backfill-session-assignments` la API ky thuat/de-dup/sync lai assignment, khong doi status enrollment.
- 2 API GET list/detail hien tai chua gắn `Authorize`; neu muon khoa lai theo nghiep vu, can bat auth o controller hoac fallback policy.
