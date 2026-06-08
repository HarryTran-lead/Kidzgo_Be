# Doc full ve cac API yeu cau cua FE

Tai lieu nay mo ta thay doi backend ngay `2026-06-09` lien quan den:

- tao `enrollment` moi nhung dung chung pool ticket cua `registration` da ton tai
- khong grant them ticket moi
- chi cho tao neu registration con ticket compatible voi class
- ap dung cho class co `slot type` compatible, khong gioi han chi rieng `Remedial`

Tai lieu nay tap trung vao:

- role va pham vi du lieu
- API contract FE can dung
- status definition
- permission matrix
- validation rule
- cac truong hop tra loi

## 1. Tong quan thay doi

### 1.1 Muc tieu nghiep vu

FE can ho tro case:

- hoc sinh da co `registration` va da duoc cap ticket
- hoc sinh hoc them 1 class khac
- class moi khong can tao goi hoc moi
- he thong dung ticket con lai cua `registration` hien co
- chi cho phep neu class moi compatible voi ticket type cua registration do

### 1.2 Thay doi code chinh

API `POST /api/enrollments` da duoc mo rong:

- them field `registrationId` trong request body
- `registrationId` la nullable
- neu `registrationId != null`:
  - enrollment moi se duoc gan voi registration do
  - he thong check ticket compatible + con ticket truoc khi tao enrollment
  - he thong khong grant them ticket moi
- neu `registrationId == null`:
  - giu nguyen flow create enrollment cu

### 1.3 Pham vi ap dung

Flow moi nay ap dung cho:

- `Remedial`
- `Review`
- class khac neu `slot type` compatible

Luu y quan trong:

- compatibility di theo `slot type`
- ticket consumption khi diem danh di theo `participation type`
- session chi consume ticket khi `participationType = Main`
- makeup participant di theo flow `MakeupCredit / MakeupAllocation` thi khong consume ticket tu registration

## 2. Moi role duoc xem du lieu gi

### 2.1 Enrollment APIs

| Role | Xem list enrollment | Xem detail enrollment | Tao enrollment |
|---|---|---|---|
| `Admin` | Co | Co | Co |
| `ManagementStaff` | Co | Co | Co |
| `Teacher` | Khong nen dung FE flow nay | Khong nen dung FE flow nay | Khong |
| `Parent` | Khong | Khong | Khong |
| `Student` | Khong | Khong | Khong |

Ghi chu:

- `POST /api/enrollments` duoc protect boi role `Admin,ManagementStaff`.
- `GET /api/enrollments` va `GET /api/enrollments/{id}` hien tai khong co explicit role attribute tai action level.
- Tuy nhien FE nen chi mo cho staff noi bo, vi student/parent da co API rieng de xem class va timetable.

### 2.2 Ticket helper APIs

| Role | Xem balance | Xem compatible ticket |
|---|---|---|
| `Admin` | Co | Co |
| `ManagementStaff` | Co | Co |
| `Teacher` | Co | Co |
| `Parent` | Co, chi student linked voi parent | Co, chi student linked voi parent |
| `Student` | Khong |

## 3. Pham vi du lieu

| API nhom | Pham vi du lieu |
|---|---|
| `POST /api/enrollments` | `all` trong pham vi staff noi bo |
| `GET /api/enrollments` | nen xem la `all` cho staff noi bo |
| `GET /api/enrollments/{id}` | nen xem la `all` cho staff noi bo |
| `GET /api/students/{studentProfileId}/tickets/balance` | `all` voi `Admin/ManagementStaff/Teacher`, `own-linked-child` voi `Parent` |
| `GET /api/students/{studentProfileId}/tickets/compatible` | `all` voi `Admin/ManagementStaff/Teacher`, `own-linked-child` voi `Parent` |

## 4. Cac hanh dong duoc phep

| Role | View enrollment | Create enrollment | View ticket balance | View ticket compatibility |
|---|---|---|---|---|
| `Admin` | Co | Co | Co | Co |
| `ManagementStaff` | Co | Co | Co | Co |
| `Teacher` | Han che, chi ticket helper | Khong | Co | Co |
| `Parent` | Khong | Khong | Co cho linked child | Co cho linked child |
| `Student` | Khong | Khong | Khong | Khong |

## 5. Danh sach API

## 5.1 POST `/api/enrollments`

### Muc dich

Tao mot enrollment moi cho hoc sinh vao class.

Co 2 mode:

- `regular enrollment`: khong truyen `registrationId`
- `shared-registration-ticket enrollment`: co truyen `registrationId` de dung chung ticket cua registration do

### Permission

- `Admin`
- `ManagementStaff`

### Request body

| Field | Type | Required | Mo ta |
|---|---|---|---|
| `classId` | `guid` | Yes | Class can tao enrollment |
| `studentProfileId` | `guid` | Yes | Hoc sinh duoc enroll |
| `enrollDate` | `date` | Yes | Ngay bat dau hoc cua enrollment |
| `registrationId` | `guid?` | No | Registration nguon ticket. Neu co, he thong dung ticket cua registration nay |
| `tuitionPlanId` | `guid?` | No | Plan cua enrollment. Neu co `registrationId` thi field nay nen bo trong hoac phai bang `registration.TuitionPlanId` |
| `track` | `string?` | No | `primary` hoac `secondary`. Mac dinh neu rong la `primary` |
| `weeklyPattern` | `WeeklyPatternEntry[]?` | No | Chon subset lich hoc cua class |
| `allowCrossBranchEnrollment` | `bool` | Yes | Cho phep hoc khac chi nhanh hay khong |

### `WeeklyPatternEntry`

| Field | Type | Required | Mo ta |
|---|---|---|---|
| `dayOfWeeks` | `string[]` | Yes | Vi du `["Monday","Wednesday"]` |
| `startTime` | `string` | Yes | Format `HH:mm` |
| `durationMinutes` | `int` | Yes | So phut cua slot |

### Vi du request dung chung registration

```json
{
  "classId": "REVIEW_OR_REMEDIAL_CLASS_ID",
  "studentProfileId": "STUDENT_ID",
  "enrollDate": "2026-06-09",
  "registrationId": "REGISTRATION_ID",
  "track": "primary",
  "weeklyPattern": [],
  "allowCrossBranchEnrollment": false
}
```

### Logic backend

Neu `registrationId` duoc truyen:

1. Tim registration.
2. Registration phai thuoc dung `studentProfileId`.
3. Registration khong duoc `Cancelled` hoac `Completed`.
4. Neu `tuitionPlanId` duoc gui thi phai bang `registration.TuitionPlanId`.
5. He thong check con it nhat 1 ticket compatible voi `class.SlotTypeId`.
6. Neu pass, tao enrollment moi va gan `enrollment.RegistrationId = registrationId`.

Neu `registrationId` khong duoc truyen:

- giu nguyen flow cu
- khong check shared registration ticket

### Response success

HTTP `201 Created`

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "CLS-001",
    "classTitle": "Review Class A",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "registrationId": "guid|null",
    "enrollDate": "2026-06-09",
    "status": "Active",
    "tuitionPlanId": "guid|null",
    "tuitionPlanName": "Plan Name|null",
    "studentHomeBranchId": "guid|null",
    "studentActiveBranchId": "guid|null",
    "isCrossBranchEnrollment": false
  }
}
```

### Response error

Backend loi dung `ProblemDetails`.

#### Validation / NotFound / Conflict format

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Enrollment.SomeErrorCode",
  "status": 400,
  "detail": "Mo ta loi",
  "errors": [
    {
      "code": "Enrollment.SomeErrorCode",
      "description": "Mo ta loi"
    }
  ]
}
```

### Cac ma loi quan trong

| HTTP | Error code | Khi nao xay ra |
|---|---|---|
| `400` | `Enrollment.RegistrationStudentMismatch` | `registrationId` khong thuoc student duoc gui |
| `400` | `Enrollment.TuitionPlanRegistrationMismatch` | Co `registrationId` nhung `tuitionPlanId` khac plan cua registration |
| `400` | `Enrollment.SessionSelectionPatternInvalid` | `weeklyPattern` sai format |
| `400` | `Enrollment.SessionSelectionPatternEmpty` | pattern khong map duoc slot nao |
| `400` | `Enrollment.SessionSelectionPatternMismatch` | pattern khong phai subset cua lich class |
| `404` | `Enrollment.ClassNotFound` | Khong tim thay class |
| `404` | `Enrollment.StudentNotFound` | Khong tim thay hoc sinh |
| `404` | `Enrollment.RegistrationNotFound` | `registrationId` khong ton tai |
| `404` | `Enrollment.TuitionPlanNotFound` | `tuitionPlanId` khong ton tai |
| `409` | `Enrollment.ClassNotAvailable` | Class khong o trang thai duoc enroll |
| `409` | `Enrollment.AlreadyEnrolled` | Hoc sinh da co enrollment active/paused trong class do |
| `409` | `Enrollment.ClassFull` | Class da het cho |
| `409` | `Enrollment.StudentScheduleConflict` | Trung lich voi class/session khac |
| `409` | `Enrollment.RegistrationNotActive` | Registration o trang thai `Cancelled` hoac `Completed` |
| `409` | `Enrollment.RegistrationTicketNotAvailable` | Registration khong con ticket compatible de dung |
| `409` | `Enrollment.TuitionPlanNotAvailable` | Tuition plan bi inactive/deleted |
| `409` | `Enrollment.TuitionPlanProgramMismatch` | Tuition plan khac program voi class |
| `409` | `Enrollment.TuitionPlanLevelMismatch` | Tuition plan khac level voi class |
| `409` | `Enrollment.TuitionPlanModuleMismatch` | Tuition plan theo module khong khop class |
| `409` | `Enrollment.ModuleBasedTuitionPlanRequiresUpcomingClass` | Plan theo module chi dung voi class `Planned/Recruiting` |
| `409` | `Enrollment.TuitionPlanIncompatibleWithClassSlotType` | Ticket type cua plan khong compatible voi slot type class |

### Ghi chu FE

- `registrationId` la `nullable`, nhung neu FE muon dung chung goi da mua thi nen truyen field nay.
- Neu khong truyen `registrationId`, enrollment se khong consume tu registration pool da ton tai theo flow moi.
- Check create-time chi xac nhan "hien tai con ticket compatible", khong reserve ticket ngay luc tao enrollment.

## 5.2 GET `/api/enrollments`

### Muc dich

Lay danh sach enrollment de staff quan ly.

### Permission

- Hien tai action khong co explicit role attribute.
- FE nen chi expose cho `Admin` va `ManagementStaff`.

### Query params

| Field | Type | Required | Mo ta |
|---|---|---|---|
| `classId` | `guid?` | No | Loc theo class |
| `studentProfileId` | `guid?` | No | Loc theo hoc sinh |
| `status` | `string?` | No | `Active`, `Paused`, `Completed`, `Dropped` |
| `pageNumber` | `int` | No | Mac dinh `1` |
| `pageSize` | `int` | No | Mac dinh `10` |

### Response success

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "enrollments": {
      "items": [
        {
          "id": "guid",
          "classId": "guid",
          "classCode": "CLS-001",
          "classTitle": "Review Class A",
          "studentProfileId": "guid",
          "studentName": "Nguyen Van A",
          "registrationId": "guid|null",
          "enrollDate": "2026-06-09",
          "status": "Active",
          "tuitionPlanId": "guid|null",
          "tuitionPlanName": "Plan Name|null"
        }
      ],
      "pageNumber": 1,
      "totalPages": 3,
      "totalCount": 25
    }
  }
}
```

### Response error

- Thuong la `200 OK`
- Neu co filter sai enum `status`, backend hien tai se parse fail va bo qua filter, khong tra loi `400`

## 5.3 GET `/api/enrollments/{id}`

### Muc dich

Lay chi tiet 1 enrollment.

### Permission

- Hien tai action khong co explicit role attribute.
- FE nen chi expose cho `Admin` va `ManagementStaff`.

### Path params

| Field | Type | Required | Mo ta |
|---|---|---|---|
| `id` | `guid` | Yes | Enrollment id |

### Response success

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "classId": "guid",
    "classCode": "CLS-001",
    "classTitle": "Review Class A",
    "programId": "guid",
    "programName": "Program Name",
    "branchId": "guid",
    "branchName": "Branch Name",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "registrationId": "guid|null",
    "studentHomeBranchId": "guid|null",
    "studentActiveBranchId": "guid|null",
    "isCrossBranchEnrollment": false,
    "enrollDate": "2026-06-09",
    "status": "Active",
    "tuitionPlanId": "guid|null",
    "tuitionPlanName": "Plan Name|null",
    "weeklyPattern": [
      {
        "dayOfWeeks": ["Monday"],
        "startTime": "18:00",
        "durationMinutes": 90
      }
    ],
    "scheduleSegments": [
      {
        "id": "guid",
        "effectiveFrom": "2026-06-09",
        "effectiveTo": null,
        "weeklyPattern": []
      }
    ],
    "createdAt": "2026-06-09T10:00:00Z",
    "updatedAt": "2026-06-09T10:00:00Z"
  }
}
```

### Response error

| HTTP | Error code | Khi nao xay ra |
|---|---|---|
| `404` | `Enrollment.NotFound` | Enrollment id khong ton tai |

## 5.4 GET `/api/students/{studentProfileId}/tickets/balance`

### Muc dich

Lay tong ticket balance cua 1 hoc sinh tren cac registration con hieu luc.

API nay huu ich cho FE khi can:

- show so ticket con lai truoc khi tao enrollment dung chung registration
- hien thong tin tong quan cho staff / teacher / parent

### Permission

- `Admin`
- `ManagementStaff`
- `Teacher`
- `Parent`

### Data scope

- `Admin/ManagementStaff/Teacher`: `all`
- `Parent`: chi student linked voi parent hien tai

### Path params

| Field | Type | Required | Mo ta |
|---|---|---|---|
| `studentProfileId` | `guid` | Yes | Hoc sinh can xem balance |

### Response success

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "studentProfileId": "guid",
    "available": 8,
    "consumed": 12,
    "totalGranted": 20
  }
}
```

### Response error

| HTTP | Error code | Khi nao xay ra |
|---|---|---|
| `404` | `LearningTicket.UserNotFound` | Current user khong ton tai |
| `404` | `ParentProfile` | Parent profile khong tim thay |
| `404` | `StudentProfile` | Student khong linked voi parent hien tai |

## 5.5 GET `/api/students/{studentProfileId}/tickets/compatible?sessionId=...`

Alias:

- `GET /api/students/{studentProfileId}/compatible-tickets?sessionId=...`

### Muc dich

Check xem hoc sinh hien tai co ticket compatible cho 1 session cu the hay khong.

API nay la helper API cho FE.

### Permission

- `Admin`
- `ManagementStaff`
- `Teacher`
- `Parent`

### Data scope

- `Admin/ManagementStaff/Teacher`: `all`
- `Parent`: chi student linked voi parent hien tai

### Params

| Field | In | Type | Required | Mo ta |
|---|---|---|---|---|
| `studentProfileId` | path | `guid` | Yes | Hoc sinh can check |
| `sessionId` | query | `guid` | Yes | Session can check compatibility |

### Response success

HTTP `200 OK`

```json
{
  "isSuccess": true,
  "data": {
    "compatible": true,
    "ticketItemId": "guid|null",
    "ticketTypeId": "guid|null",
    "ticketTypeCode": "STANDARD|null",
    "reason": "Compatible by default rule."
  }
}
```

### Y nghia field

| Field | Mo ta |
|---|---|
| `compatible = true` | Co ticket compatible va tim thay duoc it nhat 1 ticket item available |
| `compatible = false` | Khong co ticket compatible hoac het ticket |
| `reason` | Backend reason de FE show tooltip / warning |

### Response error

| HTTP | Error code | Khi nao xay ra |
|---|---|---|
| `404` | `LearningTicket.UserNotFound` | Current user khong ton tai |
| `404` | `ParentProfile` | Parent profile khong tim thay |
| `404` | `StudentProfile` | Student khong linked voi parent hien tai |

Ghi chu:

- Neu `sessionId` khong ton tai, backend hien tai tra `200` voi `compatible = false` va `reason = "Session not found"`.

## 5.6 POST `/api/registrations/{id}/assign-class`

### Trang thai API

- Khong thay doi contract trong dot cap nhat nay.
- Logic cu van giu nguyen.

### Y nghia trong tai lieu nay

API nay duoc liet ke de FE biet:

- shared-registration-ticket flow moi khong lam vo assign-class cu
- assign-class van la flow chinh de xep lop cho registration
- enrollment shared-ticket la flow bo sung de them class hoc them

## 6. Status definition

## 6.1 Enrollment status

| Status | Y nghia |
|---|---|
| `Active` | Enrollment dang hoc va session assignment con hieu luc |
| `Paused` | Enrollment tam dung |
| `Completed` | Enrollment da ket thuc, thuong do registration het ticket hoac hoc xong |
| `Dropped` | Enrollment bi huy / nghi hoc |

### Luong chuyen trang thai

| From | To | Cach chuyen |
|---|---|---|
| `Active` | `Paused` | `PATCH /api/enrollments/{id}/pause` |
| `Paused` | `Active` | `PATCH /api/enrollments/{id}/reactivate` |
| `Active` | `Dropped` | `PATCH /api/enrollments/{id}/drop` |
| `Paused` | `Dropped` | `PATCH /api/enrollments/{id}/drop` |
| `Active` | `Completed` | He thong co the auto dat khi registration het ticket |
| `Completed` | `Active` | Co the reopen trong 1 so flow rollback consume ticket |

## 6.2 Registration status lien quan

| Status | Y nghia voi flow moi |
|---|---|
| `New` | Van co the duoc dung lam registration nguon ticket neu con ticket |
| `WaitingForClass` | Van co the duoc dung neu con ticket |
| `ClassAssigned` | Van co the duoc dung neu con ticket |
| `Studying` | Van co the duoc dung neu con ticket |
| `Paused` | Hien tai khong bi chan trong create shared enrollment |
| `Completed` | Bi chan, khong duoc lam registration nguon ticket |
| `Cancelled` | Bi chan, khong duoc lam registration nguon ticket |

Ghi chu:

- Trong code hien tai, create shared enrollment chi block `Cancelled` va `Completed`.
- `Paused` khong bi block boi check moi nay.

## 6.3 Section type / Usage type definition

Mapping chinh:

| Slot usage | Session section type |
|---|---|
| `Standard` | `Normal` |
| `Review` | `Review` |
| `Remedial` | `Remedial` |
| `Makeup` | `Makeup` |

Ghi chu:

- Flow shared ticket khong gioi han chi 1 loai usage.
- Mien la ticket compatible voi `slot type`, enrollment co the tao.

## 6.4 Ticket consumption behavior

| Dieu kien | Co consume ticket khong |
|---|---|
| `participationType = Main` va `Attendance = Present` | Co |
| `participationType = Main` va `Attendance = Absent` + `NoNotice` | Co |
| `participationType = Main` va `Attendance = Absent` + co notice | Khong |
| `participationType != Main` | Khong |
| participant di theo makeup-credit flow | Khong consume tu registration ticket |

Dieu nay co nghia:

- `Review` session van co the consume ticket neu session do la `Main`
- `Remedial` session van co the consume ticket neu session do la `Main`
- `Makeup` session neu la participant makeup-credit thi di flow rieng, khong consume ticket tu registration

## 7. Permission matrix theo role

| API / Hanh dong | Admin | ManagementStaff | Teacher | Parent | Student |
|---|---|---|---|---|---|
| Create enrollment | Co | Co | Khong | Khong | Khong |
| View enrollment list | Co | Co | Khong nen expose | Khong | Khong |
| View enrollment detail | Co | Co | Khong nen expose | Khong | Khong |
| View ticket balance | Co | Co | Co | Chi linked child | Khong |
| View ticket compatibility | Co | Co | Co | Chi linked child | Khong |
| Assign class cho registration | Co | Co | Khong | Khong | Khong |

## 8. Validation rule

## 8.1 Rule kiem tra du lieu khi tao enrollment

1. `classId` bat buoc.
2. `studentProfileId` bat buoc.
3. `enrollDate` bat buoc.
4. `registrationId` la optional.
5. Neu co `registrationId`:
   - registration phai ton tai
   - registration phai thuoc dung student
   - registration khong duoc `Cancelled` hoac `Completed`
   - neu co `tuitionPlanId` thi phai bang `registration.TuitionPlanId`
   - registration phai con it nhat 1 ticket compatible voi `class.SlotTypeId`
6. Hoc sinh khong duoc da co enrollment `Active` hoac `Paused` trong cung class.
7. Class phai o trang thai co the nhan hoc vien:
   - `Active`
   - `Planned`
   - `Recruiting`
8. Class phai con suc chua.
9. `weeklyPattern` neu gui len phai la subset hop le cua lich class.
10. Hoc sinh khong duoc trung lich voi cac enrollment khac.
11. Neu khong co `registrationId` va co `tuitionPlanId` thi ap dung toan bo rule tuition plan cu:
   - plan ton tai
   - plan active
   - cung program
   - cung level
   - neu la module-based thi phai khop module va class phai la upcoming

## 8.2 Rule FE nen ap dung

1. Neu business muon dung chung ticket da mua, FE nen bat buoc staff chon `registrationId`.
2. Neu da chon `registrationId`, FE nen an hoac read-only `tuitionPlanId`.
3. FE nen show `ticket balance` truoc khi tao.
4. FE nen show warning neu class la `Makeup` de tranh nham voi flow `MakeupCredit`.
5. FE nen hieu rang check create-time khong reserve ticket.

## 9. Cac truong hop tra loi

## 9.1 Truong hop tao enrollment that bai

| Nhom loi | Tinh huong |
|---|---|
| Sai registration | registration khong ton tai, khong thuoc student, da cancelled/completed |
| Het ticket | registration khong con ticket available hoac khong co ticket compatible |
| Sai tuition plan | tuitionPlanId khac registration plan, plan khong active, khong compatible voi class |
| Sai lich | weeklyPattern sai, khong subset lich class, trung lich voi class khac |
| Sai class | class khong ton tai, khong available, da full |
| Sai hoc sinh | student khong ton tai hoac khong active |

## 9.2 Truong hop FE de y de tranh bug

1. Tao enrollment pass nhung den luc diem danh moi het ticket:
   - co the xay ra
   - vi backend chua reserve ticket tai create-time
2. `Makeup` co 2 nghia:
   - makeup class theo slot usage / section type
   - makeup participant theo `MakeupCredit`
   - 2 flow nay khong giong nhau
3. `GET /api/enrollments` va `GET /api/enrollments/{id}` hien tai chua co role guard explicit:
   - FE nen tu han che hien thi

## 10. Khuyen nghi FE implementation

1. O man hinh add class hoc them, cho staff chon:
   - hoc sinh
   - registration nguon ticket
   - class muc tieu
2. Sau khi staff chon class, FE nen:
   - show `ticket balance`
   - show thong tin slot type / usage type cua class
   - neu can, goi helper API compatibility theo session mau hoac doc matrix compatibility
3. Khi goi `POST /api/enrollments`, neu business muon dung chung ticket cu thi luon truyen `registrationId`.
4. Khong coi `AllowAll` la dieu kien duy nhat:
   - co the `RuleBased`
   - co the `manual override allow`
5. Neu class mang nghia makeup-credit thuan nghiep vu, uu tien dung flow makeup rieng thay vi enrollment shared ticket.
