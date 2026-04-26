# Tai Lieu API FE - Registration - 2026-04-25

Tai lieu nay tong hop cac API trong `RegistrationController.cs` de FE theo doi luong registration hien tai.

Pham vi tai lieu:

- Tao registration moi
- Import registration dang hoc giua chung
- Xem danh sach / chi tiet registration
- Cap nhat / huy registration
- Goi y lop, xep lop, chuyen lop
- Waiting list
- Nang goi hoc
- Preview / history / generate enrollment confirmation PDF
- Payment setting dung cho enrollment confirmation PDF

## Tong quan role va pham vi du lieu

Tat ca API trong controller deu co `[Authorize]`.

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Toan bo registration, waiting list, PDF preview/history, payment setting | `all` | `view`, `create`, `edit`, `cancel`, `assign_class`, `transfer_class`, `upgrade`, `generate_pdf`, `manage_payment_setting` |
| ManagementStaff | Toan bo registration, waiting list, PDF preview/history, payment setting read-only | `all` | `view`, `create`, `edit`, `cancel`, `assign_class`, `transfer_class`, `upgrade`, `generate_pdf` |
| Teacher | Khong duoc truy cap cac API trong controller nay | `none` | `none` |
| Parent | Khong duoc truy cap | `none` | `none` |
| Student | Khong duoc truy cap | `none` | `none` |
| Anonymous | Khong duoc truy cap | `none` | `none` |

Ghi chu:

- Hien tai handler khong co filter `own` hay `department`; Admin va ManagementStaff dang xem du lieu theo scope `all`.
- `PUT /api/registrations/enrollment-confirmation-payment-setting` chi cho `Admin`.

## Dinh dang response chung

Success tu `MatchOk()` / `MatchCreated()`:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error tu domain/validation:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Registration.ClassFull",
  "status": 400,
  "detail": "Class with ID ... is already full"
}
```

Mot so validation pipeline co the tra them danh sach `errors`:

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

## Danh sach API

### 1. POST `/api/registrations`

Dung de tao registration moi cho hoc vien.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "studentProfileId": "guid",
  "branchId": "guid",
  "programId": "guid",
  "tuitionPlanId": "guid",
  "secondaryProgramId": "guid",
  "secondaryProgramSkillFocus": "Speaking",
  "expectedStartDate": "2026-05-01T00:00:00Z",
  "preferredSchedule": "Thu 3, Thu 5 buoi toi",
  "note": "Hoc vien uu tien hoc toi"
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Phai la profile hoc sinh |
| `branchId` | `Guid` | Yes | Chi nhanh dang ky |
| `programId` | `Guid` | Yes | Chuong trinh chinh |
| `tuitionPlanId` | `Guid` | Yes | Goi hoc cua chuong trinh chinh |
| `secondaryProgramId` | `Guid?` | No | Chuong trinh phu cung registration, chi ap dung cho non-supplementary |
| `secondaryProgramSkillFocus` | `string?` | No | Skill focus cho chuong trinh phu |
| `expectedStartDate` | `DateTime?` | No | Ngay du kien bat dau |
| `preferredSchedule` | `string?` | No | Nhu cau lich hoc mong muon |
| `note` | `string?` | No | Ghi chu noi bo |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "studentProfileId": "guid",
    "branchId": "guid",
    "programId": "guid",
    "programName": "Apple",
    "secondaryProgramId": null,
    "secondaryProgramName": null,
    "secondaryProgramSkillFocus": null,
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "registrationDate": "2026-04-25T09:00:00Z",
    "expectedStartDate": "2026-05-01T00:00:00Z",
    "preferredSchedule": "Thu 3, Thu 5 buoi toi",
    "note": "Hoc vien uu tien hoc toi",
    "status": "New",
    "classId": null,
    "className": null,
    "secondaryClassId": null,
    "secondaryClassName": null,
    "createdAt": "2026-04-25T09:00:00Z"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.StudentNotFound` | `studentProfileId` khong ton tai hoac khong phai student |
| 404 | `Registration.BranchNotFound` | `branchId` khong ton tai hoac branch inactive |
| 404 | `Registration.ProgramNotFound` | `programId` hoac `secondaryProgramId` khong ton tai/inactive |
| 400 | `Registration.ProgramNotAvailableInBranch` | Program khong duoc gan vao branch |
| 404 | `Registration.TuitionPlanNotFound` | Goi hoc khong hop le cho branch/program |
| 400 | `Registration.SecondaryProgramDuplicated` | Secondary program trung voi primary |
| 400 | `Registration.SecondarySupplementaryRequiresSeparateRegistration` | Secondary program la supplementary, phai tao registration rieng |
| 409 | `Registration.AlreadyExists` | Hoc sinh da co registration active voi program nay |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

### 2. POST `/api/registrations/import-active`

Dung de import hoc sinh dang hoc giua chung tu he thong cu, co san so buoi da hoc va so buoi con lai.

Roles: `Admin`, `ManagementStaff`

Luu y nghiep vu:

- API nay chi tao registration du lieu thuc te
- Sau khi import, FE van phai goi `POST /api/registrations/{id}/assign-class` de bat dau theo doi tu buoi sap toi

Body JSON:

```json
{
  "studentProfileId": "guid",
  "branchId": "guid",
  "programId": "guid",
  "tuitionPlanId": "guid",
  "expectedStartDate": "2026-02-01T00:00:00Z",
  "actualStartDate": "2026-02-10T18:00:00Z",
  "preferredSchedule": "Thu 2, Thu 4",
  "note": "Import tu he thong cu",
  "usedSessions": 12,
  "remainingSessions": 24
}
```

Fields:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `studentProfileId` | `Guid` | Yes | Hoc sinh dang hoc giua chung |
| `branchId` | `Guid` | Yes | Chi nhanh |
| `programId` | `Guid` | Yes | Chuong trinh dang hoc |
| `tuitionPlanId` | `Guid` | Yes | Goi hoc goc |
| `expectedStartDate` | `DateTime?` | No | Ngay du kien bat dau neu muon luu |
| `actualStartDate` | `DateTime` | Yes | Ngay thuc te bat dau hoc |
| `preferredSchedule` | `string?` | No | Nhu cau lich hoc |
| `note` | `string?` | No | Ghi chu |
| `usedSessions` | `int` | Yes | So buoi da hoc, `>= 0` |
| `remainingSessions` | `int` | Yes | So buoi con lai, `> 0` |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "studentProfileId": "guid",
    "branchId": "guid",
    "programId": "guid",
    "programName": "Apple",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 36 buoi",
    "registrationDate": "2026-04-25T09:00:00Z",
    "expectedStartDate": "2026-02-01T00:00:00Z",
    "actualStartDate": "2026-02-10T18:00:00Z",
    "preferredSchedule": "Thu 2, Thu 4",
    "note": "Import tu he thong cu",
    "status": "New",
    "totalSessions": 36,
    "usedSessions": 12,
    "remainingSessions": 24,
    "createdAt": "2026-04-25T09:00:00Z"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 400 | Validation pipeline | Thieu field bat buoc, `remainingSessions <= 0`, `usedSessions < 0` |
| 400 | `Registration.ActualStartDateInFuture` | `actualStartDate` lon hon hien tai |
| 400 | `Registration.ImportSessionCountMismatch` | `usedSessions + remainingSessions` khong bang tong so buoi cua tuition plan |
| 404/409 | Giong API create | Student/branch/program/tuition plan invalid hoac da co registration active |

### 3. GET `/api/registrations`

Dung de lay danh sach registration.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `studentProfileId` | `Guid?` | No | null | Loc theo hoc sinh |
| `branchId` | `Guid?` | No | null | Loc theo branch |
| `programId` | `Guid?` | No | null | Loc theo primary hoac secondary program |
| `status` | `string?` | No | null | Loc theo `RegistrationStatus` |
| `classId` | `Guid?` | No | null | Loc theo primary hoac secondary class |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "page": {
      "items": [
        {
          "id": "guid",
          "studentProfileId": "guid",
          "studentName": "Nguyen Van A",
          "branchId": "guid",
          "branchName": "HCM",
          "programId": "guid",
          "programName": "Apple",
          "secondaryProgramId": null,
          "secondaryProgramName": null,
          "secondaryProgramSkillFocus": null,
          "tuitionPlanId": "guid",
          "tuitionPlanName": "Goi 48 buoi",
          "registrationDate": "2026-04-25T09:00:00Z",
          "expectedStartDate": "2026-05-01T00:00:00Z",
          "actualStartDate": null,
          "preferredSchedule": "Thu 2, Thu 4",
          "note": null,
          "status": "New",
          "classId": null,
          "className": null,
          "secondaryClassId": null,
          "secondaryClassName": null,
          "totalSessions": 48,
          "usedSessions": 0,
          "remainingSessions": 48,
          "createdAt": "2026-04-25T09:00:00Z"
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

- 401 Unauthorized
- 403 Forbidden

Ghi chu:

- Neu `status` query khong parse duoc enum, handler hien tai bo qua filter thay vi tra loi.

### 4. GET `/api/registrations/{id}`

Dung de lay chi tiet registration, bao gom first study session va actual study schedules.

Roles: `Admin`, `ManagementStaff`

Path params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `id` | `Guid` | Yes | Registration id |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "studentProfileId": "guid",
    "studentName": "Nguyen Van A",
    "branchId": "guid",
    "branchName": "HCM",
    "programId": "guid",
    "programName": "Apple",
    "secondaryProgramId": null,
    "secondaryProgramName": null,
    "secondaryProgramSkillFocus": null,
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "registrationDate": "2026-04-25T09:00:00Z",
    "expectedStartDate": "2026-05-01T00:00:00Z",
    "actualStartDate": null,
    "preferredSchedule": "Thu 2, Thu 4",
    "note": null,
    "status": "ClassAssigned",
    "classId": "guid",
    "className": "Apple A2",
    "entryType": "immediate",
    "secondaryClassId": null,
    "secondaryClassName": null,
    "secondaryEntryType": null,
    "totalSessions": 48,
    "usedSessions": 0,
    "remainingSessions": 48,
    "originalRegistrationId": null,
    "operationType": null,
    "firstStudySession": {
      "sessionId": "guid",
      "classEnrollmentId": "guid",
      "track": "primary",
      "classId": "guid",
      "className": "Apple A2",
      "plannedDatetime": "2026-05-03T11:00:00Z",
      "studyDate": "2026-05-03"
    },
    "actualStudySchedules": [],
    "createdAt": "2026-04-25T09:00:00Z",
    "updatedAt": "2026-04-25T10:00:00Z"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Khong tim thay registration |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

### 5. PUT `/api/registrations/{id}`

Dung de cap nhat registration khi chua complete/cancel.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "expectedStartDate": "2026-05-05T00:00:00Z",
  "preferredSchedule": "Thu 3, Thu 5",
  "note": "Cap nhat lich mong muon",
  "tuitionPlanId": "guid",
  "secondaryProgramId": "guid",
  "secondaryProgramSkillFocus": "Speaking",
  "removeSecondaryProgram": false
}
```

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "expectedStartDate": "2026-05-05T00:00:00Z",
    "preferredSchedule": "Thu 3, Thu 5",
    "note": "Cap nhat lich mong muon",
    "tuitionPlanId": "guid",
    "tuitionPlanName": "Goi 48 buoi",
    "secondaryProgramId": "guid",
    "secondaryProgramName": "Grammar Plus",
    "secondaryProgramSkillFocus": "Speaking",
    "updatedAt": "2026-04-25T10:30:00Z"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Registration khong ton tai |
| 400 | `Registration.InvalidStatus` | Registration da `Completed` hoac `Cancelled` |
| 400 | `Registration.SecondaryProgramDuplicated` | Secondary program trung primary |
| 400 | `Registration.SecondarySupplementaryRequiresSeparateRegistration` | Secondary supplementary khong duoc gan chung |
| 400 | `Registration.SecondaryClassAssigned` | Dang co secondary class nen khong duoc doi/remove secondary program |
| 409 | `Registration.AlreadyExists` | Trung secondary program voi registration active khac |
| 400 | `Registration.SecondaryProgramMissing` | Gui skill focus khi khong co secondary program |
| 400 | `Registration.ClassAlreadyAssigned` | Da xep class roi thi khong doi tuition plan bang API nay duoc |
| 404 | `Registration.TuitionPlanNotFound` | Tuition plan khong ton tai |
| 400 | `DifferentProgram` | Tuition plan khac program |

### 6. PATCH `/api/registrations/{id}/cancel`

Dung de huy registration va drop cac enrollment active lien quan.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `reason` | `string?` | No | null | Ly do huy |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "status": "Cancelled",
    "reason": "Phu huynh khong tiep tuc",
    "cancelledAt": "2026-04-25T11:00:00Z"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Khong tim thay registration |
| 400 | `Registration.InvalidStatus` | Registration da `Completed` hoac `Cancelled` |

### 7. GET `/api/registrations/{id}/suggest-classes`

Dung de goi y lop phu hop theo branch, program va preferred schedule.

Roles: `Admin`, `ManagementStaff`

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "programName": "Apple",
    "branchName": "HCM",
    "preferredSchedule": "Thu 2, Thu 4 buoi toi",
    "suggestedClasses": [
      {
        "id": "guid",
        "code": "APPLE-A2",
        "title": "Apple A2",
        "status": "Recruiting",
        "capacity": 12,
        "currentEnrollment": 5,
        "remainingSlots": 7,
        "startDate": "2026-05-01",
        "endDate": "2026-08-31",
        "weeklyScheduleSlots": [
          { "dayOfWeek": "MO", "startTime": "18:00", "durationMinutes": 90 }
        ],
        "mainTeacherName": "Teacher A",
        "classroomName": null,
        "isClassStarted": false
      }
    ],
    "alternativeClasses": [],
    "secondaryProgramId": null,
    "secondaryProgramName": null,
    "secondaryProgramSkillFocus": null,
    "secondarySuggestedClasses": [],
    "secondaryAlternativeClasses": []
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Registration khong ton tai |

Ghi chu:

- API nay da match theo `weeklyScheduleSlots` effective hien tai cua class.

### 8. POST `/api/registrations/{id}/assign-class`

Dung de xep lop cho registration.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "classId": "guid",
  "entryType": "immediate",
  "track": "primary",
  "firstStudyDate": "2026-05-03",
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
| `classId` | `Guid?` | Required voi `immediate`/`retake` | Lop can xep |
| `entryType` | `string` | Yes | `immediate`, `wait`, `retake` |
| `track` | `string` | No | `primary`, `secondary`; default `primary` |
| `firstStudyDate` | `DateOnly?` | No | Ngay hoc dau tien mong muon |
| `weeklyPattern` | `array<WeeklyPatternEntry>?` | No | Subset lich hoc cua be; bo qua nghia la hoc full lich class |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "registrationStatus": "ClassAssigned",
    "classId": "guid",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "track": "primary",
    "entryType": "immediate",
    "classAssignedDate": "2026-04-25T10:00:00Z",
    "firstStudyDate": "2026-05-03",
    "firstStudySessionAt": "2026-05-03T11:00:00Z",
    "warningMessage": "Class da bat dau. Hoc vien se tham gia giua chung."
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Registration khong ton tai |
| 400 | `Registration.InvalidStatus` | Registration da complete/cancel hoac transition entryType khong hop le |
| 400 | `Registration.InvalidEntryType` | EntryType khong nam trong `immediate`, `wait`, `retake` |
| 400 | `Registration.SecondaryProgramMissing` | Track secondary nhung khong co secondary program |
| 400 | `Registration.ClassAlreadyAssigned` | Track da co lop, phai dung transfer-class |
| 400 | `Registration.FirstStudyDateNotAllowed` | `entryType = wait` nhung gui `firstStudyDate` |
| 400 | `Registration.ClassIdRequired` | `entryType != wait` ma khong gui `classId` |
| 404 | `Registration.ClassNotFound` | Lop khong ton tai |
| 400 | `Registration.ClassNotMatchingBranch` | Lop khac branch |
| 400 | `Registration.ClassNotMatchingProgram` | Lop khac program cua track |
| 400 | `Registration.FirstStudyDateInPast` | `firstStudyDate` truoc hom nay |
| 400 | `Registration.FirstStudyDateBeforeClassStart` | `firstStudyDate` truoc `class.StartDate` |
| 400 | `Registration.FirstStudyDateAfterClassEnd` | `firstStudyDate` sau `class.EndDate` |
| 400 | `Registration.FirstStudyDateNoSession` | Ngay chon khong khop buoi hoc thuc te cua lop/pattern |
| 400 | `Registration.ClassFull` | Lop da full |
| 400 | `ClassNotAvailable` | Lop o `Completed`, `Cancelled`, `Suspended` |
| 409 | `AlreadyEnrolled` | Student da co enrollment trong lop nay |
| 400/409 | `Enrollment.SessionSelectionPatternInvalid` / `Empty` / `Mismatch` | `weeklyPattern` khong hop le hoac khong phai subset cua lich class |
| 409 | `Enrollment.StudentScheduleConflict` | Trung lich voi lop/buoi khac |

Ghi chu:

- Assign class se tao `ClassEnrollment`.
- Neu co session phu hop, backend se tinh `firstStudySessionAt`.
- Neu `entryType = immediate` va `ActualStartDate` cua registration dang null, backend se set `ActualStartDate`.
- Sau khi tao enrollment, service se sync `StudentSessionAssignment`.

### 9. GET `/api/registrations/waiting-list`

Dung de lay danh sach registration/track dang cho xep lop.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null | Loc branch |
| `programId` | `Guid?` | No | null | Loc program |
| `track` | `string?` | No | null | `primary` hoac `secondary` |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "studentName": "Nguyen Van A",
        "branchId": "guid",
        "branchName": "HCM",
        "track": "primary",
        "programId": "guid",
        "programName": "Apple",
        "isSupplementaryProgram": false,
        "programSkillFocus": null,
        "tuitionPlanId": "guid",
        "tuitionPlanName": "Goi 48 buoi",
        "registrationDate": "2026-04-25T09:00:00Z",
        "expectedStartDate": "2026-05-01T00:00:00Z",
        "preferredSchedule": "Thu 2, Thu 4",
        "registrationStatus": "New",
        "daysWaiting": 3
      }
    ],
    "totalCount": 1,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

Response loi:

- 401 Unauthorized
- 403 Forbidden

### 10. POST `/api/registrations/{id}/transfer-class`

Dung de chuyen lop cho registration da co lop.

Roles: `Admin`, `ManagementStaff`

Body JSON:

```json
{
  "newClassId": "guid",
  "track": "primary",
  "weeklyPattern": [
    {
      "dayOfWeeks": ["TH"],
      "startTime": "18:00",
      "durationMinutes": 90
    }
  ],
  "effectiveDate": "2026-05-01T00:00:00Z"
}
```

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "oldClassId": "guid",
    "oldClassName": "Apple A1",
    "newClassId": "guid",
    "newClassName": "Apple A2",
    "track": "primary",
    "effectiveDate": "2026-05-01T00:00:00Z",
    "status": "Studying"
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Khong tim thay registration |
| 400 | `Registration.InvalidStatus` | Registration complete/cancel |
| 400 | `Registration.SecondaryProgramMissing` | Track secondary nhung khong co secondary program |
| 400 | `NoClassAssigned` | Track hien tai chua co lop |
| 400 | `Registration.CannotTransferToSameClass` | Chuyen sang chinh lop hien tai |
| 404 | `Registration.ClassNotFound` | Lop moi khong ton tai |
| 400 | `Registration.ClassNotMatchingBranch` / `ClassNotMatchingProgram` | Lop moi khong hop le |
| 400 | `Registration.ClassFull` | Lop moi da full |
| 400 | `ClassNotAvailable` | Lop moi khong o `Active` hoac `Recruiting` |
| 400/409 | `Enrollment.SessionSelectionPatternInvalid` / `Empty` / `Mismatch` | `weeklyPattern` khong hop le |
| 409 | `Enrollment.StudentScheduleConflict` | Trung lich sau effective date |

### 11. POST `/api/registrations/{id}/upgrade`

Dung de nang goi hoc cho registration dang active.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Mo ta |
| --- | --- | --- | --- |
| `newTuitionPlanId` | `Guid` | Yes | Goi hoc moi |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "originalRegistrationId": "guid",
    "newRegistrationId": "guid",
    "oldTuitionPlanName": "Goi 24 buoi",
    "newTuitionPlanName": "Goi 48 buoi",
    "oldTotalSessions": 24,
    "newTotalSessions": 60,
    "addedSessions": 36,
    "status": "Studying"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Khong tim thay registration |
| 400 | `Registration.NoActiveRegistrationForUpgrade` | Registration khong o `Studying`, `ClassAssigned`, `WaitingForClass` |
| 404 | `Registration.TuitionPlanNotFound` | Goi moi khong ton tai/khong dung branch |
| 400 | `DifferentProgram` | Tuition plan moi khac program |

Ghi chu:

- Handler hien tai upgrade in-place tren chinh registration hien co.
- `RemainingSessions` moi = `remainingSessions cu + totalSessions cua goi moi`.

### 12. GET `/api/registrations/{id}/enrollment-confirmation-pdf`

Dung de preview du lieu PDF enrollment confirmation truoc khi generate file.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `track` | `string` | No | `primary` | `primary` hoac `secondary` |
| `formType` | `string` | No | `auto` | `auto`, `newStudent`, `continuingStudent` |

Response success:

- Tra thong tin context + activePdf + preview data
- `preview` gom student, branch, class, tuition, payment, QR, logo, policy lines, reconciliation neu la continuing form

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Registration khong ton tai |
| 400 | `Registration.InvalidEnrollmentConfirmationPdfFormType` | `formType` khong hop le |
| 400 | Validation/NotFound khac | Chua co enrollment active cho track, thieu payment setting, thieu du lieu bat buoc de build preview |

### 13. GET `/api/registrations/{id}/enrollment-confirmation-pdf/history`

Dung de lay lich su cac file PDF da generate.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `track` | `string?` | No | null | Loc theo `primary` / `secondary` |
| `formType` | `string?` | No | null | Loc theo form type |
| `pageNumber` | `int` | No | 1 | Trang |
| `pageSize` | `int` | No | 10 | So item/trang |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "pdfs": {
      "items": [
        {
          "pdfRecordId": "guid",
          "registrationId": "guid",
          "enrollmentId": "guid",
          "track": "primary",
          "formType": "newStudent",
          "pdfUrl": "https://...",
          "generatedAt": "2026-04-25T11:30:00Z",
          "generatedBy": "guid",
          "generatedByName": "Staff A",
          "isActive": true,
          "hasSnapshot": true,
          "studentName": "Nguyen Van A",
          "classCode": "APPLE-A2",
          "classTitle": "Apple A2",
          "programName": "Apple"
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
| 400 | `Registration.InvalidEnrollmentConfirmationPdfFormType` | `formType` filter khong hop le |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

### 14. POST `/api/registrations/{id}/enrollment-confirmation-pdf`

Dung de generate file PDF enrollment confirmation.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `track` | `string` | No | `primary` | Track can generate |
| `regenerate` | `bool` | No | `false` | Neu `false`, co the reuse file active hien tai |
| `formType` | `string` | No | `auto` | `auto`, `newStudent`, `continuingStudent` |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "enrollmentId": "guid",
    "pdfRecordId": "guid",
    "track": "primary",
    "formType": "newStudent",
    "pdfUrl": "https://...",
    "pdfGeneratedAt": "2026-04-25T11:30:00Z",
    "reusedExistingPdf": false,
    "enrollDate": "2026-05-03",
    "firstStudyDate": "2026-05-03",
    "studentName": "Nguyen Van A",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "programName": "Apple",
    "tuitionPlanName": "Goi 48 buoi",
    "tuitionAmount": 30000000,
    "currency": "VND"
  }
}
```

Response loi:

- Cung nhom loi voi preview API
- Co the fail neu luu file/storage/render PDF loi

### 15. GET `/api/registrations/enrollment-confirmation-payment-setting`

Dung de lay cau hinh thong tin thanh toan va logo cho enrollment confirmation PDF.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `branchId` | `Guid?` | No | null | Neu co thi uu tien lay setting theo branch; fallback global neu khong co |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "guid",
    "isFallbackToGlobal": false,
    "paymentMethod": "Chuyen khoan",
    "accountName": "TRINH DUC ANH",
    "accountNumber": "0123456789",
    "bankName": "MB Bank",
    "bankCode": "mbbank",
    "bankBin": "970422",
    "vietQrTemplate": "compact2",
    "logoUrl": "https://...",
    "newStudentPolicyLines": ["Khong ap dung hoan phi."],
    "reservationPolicyLines": ["Chinh sach bao luu toi da 01 lan."],
    "qrPreviewUrl": "https://...",
    "isActive": true,
    "createdAt": "2026-04-25T09:00:00Z",
    "updatedAt": "2026-04-25T10:00:00Z",
    "updatedBy": "guid"
  }
}
```

Response loi:

- 401 Unauthorized
- 403 Forbidden

### 16. PUT `/api/registrations/enrollment-confirmation-payment-setting`

Dung de cap nhat payment setting va logo cho PDF.

Roles: `Admin`

Body JSON:

```json
{
  "branchId": "guid",
  "paymentMethod": "Chuyen khoan",
  "accountName": "TRINH DUC ANH",
  "accountNumber": "0123456789",
  "bankName": "MB Bank",
  "bankCode": "mbbank",
  "bankBin": "970422",
  "vietQrTemplate": "compact2",
  "logoUrl": "https://...",
  "newStudentPolicyLines": ["Khong ap dung hoan phi."],
  "reservationPolicyLines": ["Chinh sach bao luu toi da 01 lan."],
  "isActive": true
}
```

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "id": "guid",
    "branchId": "guid",
    "paymentMethod": "Chuyen khoan",
    "accountName": "TRINH DUC ANH",
    "accountNumber": "0123456789",
    "bankName": "MB Bank",
    "bankCode": "mbbank",
    "bankBin": "970422",
    "vietQrTemplate": "compact2",
    "logoUrl": "https://...",
    "newStudentPolicyLines": ["Khong ap dung hoan phi."],
    "reservationPolicyLines": ["Chinh sach bao luu toi da 01 lan."],
    "qrPreviewUrl": "https://...",
    "isActive": true,
    "createdAt": "2026-04-25T09:00:00Z",
    "updatedAt": "2026-04-25T10:30:00Z",
    "updatedBy": "guid"
  }
}
```

Response loi:

- 400 Validation pipeline neu thieu `accountName`, `accountNumber`, `bankName`
- 401 Unauthorized
- 403 Forbidden

## Status definition

### RegistrationStatus

| Status | Y nghia |
| --- | --- |
| `New` | Moi tao registration, chua chot waiting/assign hoc thuc te |
| `WaitingForClass` | Dang cho xep lop |
| `ClassAssigned` | Da co lop va enrollment, chua den moc system coi la dang hoc |
| `Studying` | Dang hoc |
| `Paused` | Dang bao luu |
| `Completed` | Da hoan thanh |
| `Cancelled` | Da huy |

### EntryType

| Value | Y nghia |
| --- | --- |
| `immediate` | Vao hoc ngay / tham gia cac buoi con lai |
| `wait` | Cho lop moi |
| `retake` | Thi lai / cho xep lop sau placement retake |

### OperationType

| Value | Y nghia |
| --- | --- |
| `Initial` | Dang ky lan dau |
| `Upgrade` | Nang goi |
| `Renewal` | Gia han |
| `Transfer` | Chuyen lop |
| `TransferBranch` | Chuyen chi nhanh |
| `Retake` | Thi lai |

## Luong chuyen trang thai

Luong tong quat:

1. Staff tao registration -> `New`
2. Staff chon:
   - assign `entryType = wait` -> `WaitingForClass`
   - assign `entryType = immediate/retake` + co lop -> backend resolve status thanh `ClassAssigned` hoac `Studying` tuy ngữ cảnh
3. Trong qua trinh hoc:
   - pause enrollment co the lam registration thanh `Paused`
   - reassign/reactivate co the dua ve `ClassAssigned` hoac `Studying`
4. Cancel -> `Cancelled`
5. Ket thuc khoa hoc -> `Completed`

Ghi chu:

- Status cu the duoc resolve qua helper `RegistrationTrackHelper.ResolveStatus(registration)`.
- Mot registration co the co ca track `primary` va `secondary`; status tong la status gop.

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/registrations` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/import-active` | Yes | Yes | No | No | No | No |
| `GET /api/registrations` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}` | Yes | Yes | No | No | No | No |
| `PUT /api/registrations/{id}` | Yes | Yes | No | No | No | No |
| `PATCH /api/registrations/{id}/cancel` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}/suggest-classes` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/{id}/assign-class` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/waiting-list` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/{id}/transfer-class` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/{id}/upgrade` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf/history` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/enrollment-confirmation-payment-setting` | Yes | Yes | No | No | No | No |
| `PUT /api/registrations/enrollment-confirmation-payment-setting` | Yes | No | No | No | No | No |

## Validation rule tong hop

| Rule | API ap dung | Ket qua khi sai |
| --- | --- | --- |
| User phai dang nhap | Tat ca | 401 |
| Role phai dung theo endpoint | Tat ca | 403 |
| `studentProfileId` phai la student ton tai | Create, import-active | 404 `Registration.StudentNotFound` |
| `branchId` phai ton tai va active | Create, import-active | 404 `Registration.BranchNotFound` |
| Program phai ton tai, active, khong deleted | Create, import-active, update secondary | 404 `Registration.ProgramNotFound` |
| Program phai duoc assign vao branch | Create, import-active | 400 `Registration.ProgramNotAvailableInBranch` |
| Tuition plan phai hop le voi program/branch | Create, import-active, update, upgrade | 404 `Registration.TuitionPlanNotFound` hoac 400 `DifferentProgram` |
| Khong duoc tao trung registration active cung program | Create, import-active, update secondary | 409 `Registration.AlreadyExists` |
| Secondary program khong duoc trung primary | Create, update | 400 `Registration.SecondaryProgramDuplicated` |
| Secondary supplementary phai tao registration rieng | Create, update | 400 `Registration.SecondarySupplementaryRequiresSeparateRegistration` |
| Khong duoc update/cancel registration da complete/cancel | Update, cancel | 400 `Registration.InvalidStatus` |
| `actualStartDate` import khong duoc o tuong lai | Import-active | 400 `Registration.ActualStartDateInFuture` |
| `usedSessions + remainingSessions` phai bang tong buoi cua tuition plan | Import-active | 400 `Registration.ImportSessionCountMismatch` |
| `remainingSessions > 0` voi import-active | Import-active | 400 validation pipeline |
| `entryType` chi nhan `immediate`, `wait`, `retake` | Assign-class | 400 `Registration.InvalidEntryType` |
| `classId` bat buoc khi assign immediate/retake | Assign-class | 400 `Registration.ClassIdRequired` |
| `firstStudyDate` chi dung khi co class | Assign-class | 400 `Registration.FirstStudyDateNotAllowed` |
| `firstStudyDate` phai khop session thuc te cua lop | Assign-class | 400 `Registration.FirstStudyDateNoSession` |
| `weeklyPattern` phai la subset cua lich class | Assign, transfer, enrollment-related flows | 400/409 `Enrollment.SessionSelectionPattern...` |
| Student khong duoc trung lich hoc | Assign, transfer, enrollment-related flows | 409 `Enrollment.StudentScheduleConflict` |
| Upgrade chi ap dung cho registration active | Upgrade | 400 `Registration.NoActiveRegistrationForUpgrade` |
| `formType` PDF chi nhan `auto`, `newStudent`, `continuingStudent` | Preview/history/generate PDF | 400 `Registration.InvalidEnrollmentConfirmationPdfFormType` |

## Luu y FE quan trong

- `GET /api/registrations/{id}` da tra `firstStudySession` va `actualStudySchedules`.
- `assign-class`, `transfer-class` dung `weeklyPattern`; neu bo qua thi hoc vien hoc toan bo lich class.
- Class schedule cong khai cho FE o cac API class dang dung `weeklyScheduleSlots`, khong dung `schedulePattern` nua.
- `import-active` chi la buoc nap du lieu; muon bat dau theo doi tu buoi sap toi van phai `assign-class`.
- PDF preview/history/payment setting da co API rieng trong chinh controller nay, khong can goi qua module khac.
