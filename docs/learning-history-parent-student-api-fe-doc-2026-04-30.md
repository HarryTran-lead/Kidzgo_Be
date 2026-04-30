# Tai Lieu API FE - Learning History Parent/Student - 2026-04-30

## Vi sao can API nay

API `learning history` duoc them vao de giai quyet bai toan lich su hoc tap bi phan manh.

Ly do nghiep vu:

- Sau khi hoc het buoi, registration co the chuyen sang `Completed`.
- Neu hoc vien hoc tiep theo flow hien tai, he thong thuong tao registration moi thay vi reopen registration cu.
- Khi do lich su hoc tap cua hoc vien bi tach ra thanh nhieu manh:
  - registration cu
  - registration moi
  - enrollment cu/moi
  - attendance
  - timetable
  - missions
- FE can mot API tong hop de hien thi "hanh trinh hoc tap" lien mach cho Parent va Student ma khong phai tu ghep du lieu tu nhieu endpoint.

Muc tieu cua API:

- Gom lich su registration.
- Gom lich su enrollment.
- Gom timeline session/attendance/makeup.
- Gom mission history.
- Tra them summary de FE dung nhanh cho dashboard/history screen.

## Pham vi tai lieu

Tai lieu nay gom 2 API:

- `GET /api/parent/learning-history`
- `GET /api/students/learning-history`

Ca 2 API cung dung chung `GetLearningHistoryQuery` va chung response `GetLearningHistoryResponse`.

## Role va pham vi du lieu

Tat ca API deu co `[Authorize]`.

| Role | Endpoint | Du lieu duoc xem | Pham vi du lieu | Hanh dong |
| --- | --- | --- | --- | --- |
| Parent | `/api/parent/learning-history` | Learning history cua hoc sinh lien ket voi parent | `own-linked-students` | `view` |
| Student | `/api/students/learning-history` | Learning history cua chinh hoc sinh dang dang nhap | `own` | `view` |
| Admin | Khong duoc expose qua 2 controller nay | `none` | `none` | `none` |
| ManagementStaff | Khong duoc expose qua 2 controller nay | `none` | `none` | `none` |
| Teacher | Khong duoc expose qua 2 controller nay | `none` | `none` | `none` |
| Anonymous | Khong duoc truy cap | `none` | `none` | `none` |

Ghi chu access:

- Parent co the gui `studentProfileId` de chon 1 hoc sinh cu the.
- Neu Parent khong gui `studentProfileId`, backend se uu tien:
  - `userContext.StudentId` neu token da chon hoc sinh.
  - neu khong co, lay hoc sinh lien ket dau tien cua parent.
- Parent chi xem duoc hoc sinh co `ParentStudentLink`.
- Student khong co param `studentProfileId`; neu co co tinh chen id khac o application layer thi se bi tra `Profile.StudentNotFound`.

## Dinh dang response chung

Success:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Profile.StudentNotLinkedToParent",
  "status": 404,
  "detail": "Student not linked to this parent"
}
```

Validation error neu co:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": []
}
```

## Danh sach API

### 1. GET `/api/parent/learning-history`

Dung de lay learning history tong hop cho 1 hoc sinh thuoc parent hien tai.

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | `null` | Hoc sinh can xem history. Neu bo trong, backend tu chon theo rule access. |
| `from` | `DateTime?` | No | `null` | Moc bat dau loc session va mission. |
| `to` | `DateTime?` | No | `null` | Moc ket thuc loc session va mission, backend mo rong den cuoi ngay VN. |
| `sessionPageNumber` | `int` | No | `1` | Trang cho `sessions`. |
| `sessionPageSize` | `int` | No | `20` | So item/trang cho `sessions`. |
| `missionPageNumber` | `int` | No | `1` | Trang cho `missions`. |
| `missionPageSize` | `int` | No | `20` | So item/trang cho `missions`. |

### 2. GET `/api/students/learning-history`

Dung de lay learning history tong hop cho hoc sinh dang dang nhap.

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `from` | `DateTime?` | No | `null` | Moc bat dau loc session va mission. |
| `to` | `DateTime?` | No | `null` | Moc ket thuc loc session va mission, backend mo rong den cuoi ngay VN. |
| `sessionPageNumber` | `int` | No | `1` | Trang cho `sessions`. |
| `sessionPageSize` | `int` | No | `20` | So item/trang cho `sessions`. |
| `missionPageNumber` | `int` | No | `1` | Trang cho `missions`. |
| `missionPageSize` | `int` | No | `20` | So item/trang cho `missions`. |

## Response success

Ca 2 API deu tra cung mot response shape: `GetLearningHistoryResponse`.

### Root object

| Field | Type | Mo ta |
| --- | --- | --- |
| `studentProfileId` | `Guid` | Hoc sinh duoc xem lich su. |
| `studentName` | `string` | Ten hien thi hoc sinh. |
| `summary` | `LearningHistorySummaryDto` | So lieu tong hop. |
| `registrations` | `LearningHistoryRegistrationDto[]` | Toan bo lich su registration cua hoc sinh. |
| `enrollments` | `LearningHistoryEnrollmentDto[]` | Toan bo lich su enrollment cua hoc sinh. |
| `sessions` | `Page<LearningHistorySessionDto>` | Timeline session co paging. |
| `missions` | `Page<LearningHistoryMissionDto>` | Mission history co paging. |

### `summary`

| Field | Type | Mo ta |
| --- | --- | --- |
| `totalRegistrations` | `int` | Tong so registration cua hoc sinh. |
| `completedRegistrations` | `int` | So registration dang `Completed`. |
| `totalPurchasedSessions` | `int` | Tong `TotalSessions` cong don tat ca registration. |
| `totalUsedSessions` | `int` | Tong `UsedSessions` cong don tat ca registration. |
| `totalRemainingSessions` | `int` | Tong `RemainingSessions` cong don tat ca registration. |
| `totalEnrollments` | `int` | Tong enrollment lich su. |
| `completedEnrollments` | `int` | So enrollment co status `Completed`. |
| `totalSessionRecords` | `int` | Tong so session record trong timeline da loc theo `from/to` neu co. |
| `presentSessions` | `int` | So session co attendance `Present`. |
| `absentSessions` | `int` | So session co attendance `Absent`. |
| `makeupSessions` | `int` | So session makeup hoac attendance `Makeup`. |
| `totalMissions` | `int` | Tong so mission progress cua hoc sinh. |
| `completedMissions` | `int` | So mission progress da `Completed`. |

### `registrations[]`

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `registrationDate` | `DateTime` |
| `status` | `string` |
| `operationType` | `string?` |
| `branchId` | `Guid` |
| `branchName` | `string` |
| `programId` | `Guid` |
| `programName` | `string` |
| `secondaryProgramId` | `Guid?` |
| `secondaryProgramName` | `string?` |
| `tuitionPlanId` | `Guid` |
| `tuitionPlanName` | `string` |
| `classId` | `Guid?` |
| `className` | `string?` |
| `secondaryClassId` | `Guid?` |
| `secondaryClassName` | `string?` |
| `expectedStartDate` | `DateTime?` |
| `actualStartDate` | `DateTime?` |
| `totalSessions` | `int` |
| `usedSessions` | `int` |
| `remainingSessions` | `int` |
| `originalRegistrationId` | `Guid?` |
| `updatedAt` | `DateTime` |

### `enrollments[]`

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `registrationId` | `Guid?` |
| `classId` | `Guid` |
| `classCode` | `string` |
| `classTitle` | `string` |
| `programId` | `Guid` |
| `programName` | `string` |
| `branchId` | `Guid` |
| `branchName` | `string` |
| `track` | `string` |
| `enrollDate` | `DateOnly` |
| `status` | `EnrollmentStatus` |
| `createdAt` | `DateTime` |
| `updatedAt` | `DateTime` |

### `sessions`

`sessions` la `Page<LearningHistorySessionDto>`:

| Field | Type | Mo ta |
| --- | --- | --- |
| `items` | `LearningHistorySessionDto[]` | Du lieu trang hien tai |
| `pageNumber` | `int?` | Trang hien tai |
| `totalPages` | `int?` | Tong so trang |
| `totalCount` | `int` | Tong so session record |
| `hasPreviousPage` | `bool` | Co trang truoc hay khong |
| `hasNextPage` | `bool` | Co trang sau hay khong |

`LearningHistorySessionDto`:

| Field | Type |
| --- | --- |
| `sessionId` | `Guid` |
| `classId` | `Guid` |
| `classCode` | `string` |
| `classTitle` | `string` |
| `plannedDatetime` | `DateTime` |
| `actualDatetime` | `DateTime?` |
| `durationMinutes` | `int` |
| `sessionStatus` | `string` |
| `registrationId` | `Guid?` |
| `track` | `string?` |
| `isMakeup` | `bool` |
| `attendanceStatus` | `string?` |
| `absenceType` | `string?` |
| `attendanceMarkedAt` | `DateTime?` |
| `attendanceNote` | `string?` |
| `teacherId` | `Guid?` |
| `teacherName` | `string?` |
| `roomId` | `Guid?` |
| `roomName` | `string?` |

### `missions`

`missions` la `Page<LearningHistoryMissionDto>`:

| Field | Type | Mo ta |
| --- | --- | --- |
| `items` | `LearningHistoryMissionDto[]` | Du lieu trang hien tai |
| `pageNumber` | `int?` | Trang hien tai |
| `totalPages` | `int?` | Tong so trang |
| `totalCount` | `int` | Tong so mission record sau khi query |
| `hasPreviousPage` | `bool` | Co trang truoc hay khong |
| `hasNextPage` | `bool` | Co trang sau hay khong |

`LearningHistoryMissionDto`:

| Field | Type |
| --- | --- |
| `id` | `Guid` |
| `missionId` | `Guid` |
| `title` | `string` |
| `description` | `string?` |
| `missionType` | `string` |
| `progressMode` | `string` |
| `status` | `string` |
| `progressValue` | `decimal?` |
| `totalRequired` | `int?` |
| `progressPercentage` | `decimal` |
| `rewardStars` | `int?` |
| `rewardExp` | `int?` |
| `startAt` | `DateTime?` |
| `endAt` | `DateTime?` |
| `createdAt` | `DateTime` |
| `completedAt` | `DateTime?` |

## Response error

| HTTP | Code | Khi nao xay ra |
| --- | --- | --- |
| `401` | Unauthorized | Chua dang nhap / token khong hop le |
| `404` | `Profile.ParentNotFound` | Token parent khong map duoc parent profile active |
| `404` | `Profile.StudentNotLinkedToParent` | Parent truy cap hoc sinh khong lien ket |
| `404` | `Profile.StudentNotFound` | Student profile khong ton tai, khong active, bi delete, hoac student truy cap sai student |

## Validation rule

- `studentProfileId` chi dung cho endpoint Parent.
- `from` va `to` la optional.
- Backend hien tai khong co validator rieng cho:
  - `from > to`
  - `pageNumber <= 0`
  - `pageSize <= 0`
- FE nen tu validate truoc de tranh request khong mong muon.

## Ghi chu nghiep vu quan trong

- `registrations` va `enrollments` hien tai tra toan bo lich su, khong bi cat theo `from/to`.
- `sessions` co ap dung filter `from/to` tren `Session.PlannedDatetime`.
- `missions` page co ap dung filter `from/to` tren `Mission.CreatedAt`.
- `summary.totalMissions` va `summary.completedMissions` hien tai la tong toan cuc cua hoc sinh, khong bi cat theo `from/to`.
- Datetime trong response da duoc convert ve gio Viet Nam.

