# Tai Lieu API FE - Registration Discount Related - 2026-04-28

Tai lieu nay tong hop cac API trong `RegistrationController.cs` co lien quan truc tiep den pricing snapshot / discount campaign.

Pham vi tai lieu:

- Tao registration moi co tinh discount snapshot
- Import registration dang hoc giua chung co tinh discount snapshot
- Xem danh sach / chi tiet registration co field discount
- Cap nhat registration trong truong hop doi tuition plan va re-calculate pricing
- Nang goi hoc co discount va carry-over
- Preview / generate enrollment confirmation PDF co hien pricing sau discount

Ghi chu:

- Tai lieu nay chi cover nhom API lien quan discount.
- Cac API registration khac trong controller van ton tai, nhung khong duoc liet ke chi tiet trong file nay.

## Tong quan role va pham vi du lieu

Tat ca API trong pham vi tai lieu deu co `[Authorize]`.

| Role | Du lieu duoc xem | Pham vi du lieu | Hanh dong duoc phep |
| --- | --- | --- | --- |
| Admin | Toan bo registration va PDF preview/generate | `all` | `view`, `create`, `edit`, `upgrade`, `generate_pdf` |
| ManagementStaff | Toan bo registration va PDF preview/generate | `all` | `view`, `create`, `edit`, `upgrade`, `generate_pdf` |
| Teacher | Khong duoc truy cap | `none` | `none` |
| Parent | Khong duoc truy cap trong controller nay | `none` | `none` |
| Student | Khong duoc truy cap | `none` | `none` |
| Anonymous | Khong duoc truy cap | `none` | `none` |

Ghi chu:

- Hien tai khong co filter `own` hay `department`; `Admin` va `ManagementStaff` dang xem du lieu theo scope `all`.

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
  "title": "Registration.TuitionPlanNotFound",
  "status": 404,
  "detail": "Tuition plan with ID ... was not found"
}
```

Validation pipeline co the tra them danh sach `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "GreaterThanValidator",
      "description": "'Remaining Sessions' must be greater than '0'."
    }
  ]
}
```

## Danh sach API

### 1. POST `/api/registrations`

Dung de tao registration moi cho hoc vien va snapshot pricing tai thoi diem dang ky.

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
| `secondaryProgramId` | `Guid?` | No | Chuong trinh phu cung registration |
| `secondaryProgramSkillFocus` | `string?` | No | Skill focus cho secondary program |
| `expectedStartDate` | `DateTime?` | No | Ngay du kien bat dau |
| `preferredSchedule` | `string?` | No | Nhu cau lich hoc |
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
    "registrationDate": "2026-04-28T09:00:00Z",
    "expectedStartDate": "2026-05-01T00:00:00Z",
    "preferredSchedule": "Thu 3, Thu 5 buoi toi",
    "note": "Hoc vien uu tien hoc toi",
    "status": "New",
    "operationType": "Initial",
    "classId": null,
    "className": null,
    "secondaryClassId": null,
    "secondaryClassName": null,
    "discountCampaignId": "guid",
    "discountCampaignName": "Holiday 30-4",
    "discountType": "Percentage",
    "discountValue": 10,
    "originalTuitionAmount": 30000000,
    "discountAmount": 3000000,
    "carryOverCreditAmount": 0,
    "finalTuitionAmount": 27000000,
    "createdAt": "2026-04-28T09:00:00Z"
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
| 404 | `Registration.TuitionPlanNotFound` | Tuition plan khong hop le cho branch/program |
| 400 | `Registration.SecondaryProgramDuplicated` | Secondary program trung primary |
| 400 | `Registration.SecondarySupplementaryRequiresSeparateRegistration` | Secondary program supplementary phai tao registration rieng |
| 409 | `Registration.AlreadyExists` | Hoc sinh da co registration active voi program nay |
| 401 | Unauthorized | Chua dang nhap |
| 403 | Forbidden | Role khong hop le |

### 2. POST `/api/registrations/import-active`

Dung de import hoc sinh dang hoc giua chung tu he thong cu va snapshot pricing tai ngay import.

Roles: `Admin`, `ManagementStaff`

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
    "registrationDate": "2026-04-28T09:00:00Z",
    "expectedStartDate": "2026-02-01T00:00:00Z",
    "actualStartDate": "2026-02-10T18:00:00Z",
    "preferredSchedule": "Thu 2, Thu 4",
    "note": "Import tu he thong cu",
    "status": "New",
    "operationType": "Renewal",
    "totalSessions": 36,
    "usedSessions": 12,
    "remainingSessions": 24,
    "discountCampaignId": "guid",
    "discountCampaignName": "Renewal 5-2026",
    "discountType": "FixedAmount",
    "discountValue": 1000000,
    "originalTuitionAmount": 18000000,
    "discountAmount": 1000000,
    "carryOverCreditAmount": 0,
    "finalTuitionAmount": 17000000,
    "createdAt": "2026-04-28T09:00:00Z"
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

Ghi chu:

- Discount cua import-active hien tai duoc xet theo ngay import (`registrationDate`), khong theo `actualStartDate`.

### 3. GET `/api/registrations`

Dung de lay danh sach registration co kem pricing snapshot.

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
          "registrationDate": "2026-04-28T09:00:00Z",
          "expectedStartDate": "2026-05-01T00:00:00Z",
          "actualStartDate": null,
          "preferredSchedule": "Thu 2, Thu 4",
          "note": null,
          "status": "New",
          "operationType": "Initial",
          "classId": null,
          "className": null,
          "secondaryClassId": null,
          "secondaryClassName": null,
          "totalSessions": 48,
          "usedSessions": 0,
          "remainingSessions": 48,
          "discountCampaignId": "guid",
          "discountCampaignName": "Holiday 30-4",
          "discountType": "Percentage",
          "discountValue": 10,
          "originalTuitionAmount": 30000000,
          "discountAmount": 3000000,
          "carryOverCreditAmount": 0,
          "finalTuitionAmount": 27000000,
          "createdAt": "2026-04-28T09:00:00Z"
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

Dung de lay chi tiet registration, bao gom pricing snapshot, first study session va actual study schedules.

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
    "registrationDate": "2026-04-28T09:00:00Z",
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
    "operationType": "Initial",
    "discountCampaignId": "guid",
    "discountCampaignName": "Holiday 30-4",
    "discountType": "Percentage",
    "discountValue": 10,
    "originalTuitionAmount": 30000000,
    "discountAmount": 3000000,
    "carryOverCreditAmount": 0,
    "finalTuitionAmount": 27000000,
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
    "createdAt": "2026-04-28T09:00:00Z",
    "updatedAt": "2026-04-28T10:00:00Z"
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

Dung de cap nhat registration khi chua complete/cancel. Neu doi `tuitionPlanId` trong luc chua xep lop, backend se re-calculate pricing snapshot.

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
    "operationType": "Initial",
    "discountCampaignId": "guid",
    "discountCampaignName": "Holiday 30-4",
    "discountType": "Percentage",
    "discountValue": 10,
    "originalTuitionAmount": 30000000,
    "discountAmount": 3000000,
    "carryOverCreditAmount": 0,
    "finalTuitionAmount": 27000000,
    "updatedAt": "2026-04-28T10:30:00Z"
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

Ghi chu:

- Khi doi tuition plan, pricing duoc tinh lai theo `registration.RegistrationDate` goc cua registration, khong theo ngay update hien tai.

### 6. POST `/api/registrations/{id}/upgrade`

Dung de nang goi hoc cho registration dang active va tinh lai pricing theo operation `Upgrade`.

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
    "discountCampaignId": "guid",
    "discountCampaignName": "Upgrade Summer 2026",
    "discountType": "FixedAmount",
    "discountValue": 1000000,
    "originalTuitionAmount": 9000000,
    "discountAmount": 1000000,
    "carryOverCreditAmount": 500000,
    "finalTuitionAmount": 7500000,
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

- Upgrade hien tai la `in-place` tren chinh registration hien co.
- Discount upgrade duoc tinh tren gia khoa moi truoc khi can tru `carryOverCreditAmount`.
- `carryOverCreditAmount` = `remainingSessions * oldUnitPriceSession`.

### 7. GET `/api/registrations/{id}/enrollment-confirmation-pdf`

Dung de preview du lieu PDF enrollment confirmation, bao gom pricing sau discount.

Roles: `Admin`, `ManagementStaff`

Query params:

| Field | Type | Required | Default | Mo ta |
| --- | --- | --- | --- | --- |
| `track` | `string` | No | `primary` | `primary` hoac `secondary` |
| `formType` | `string` | No | `auto` | `auto`, `newStudent`, `continuingStudent` |

Response success:

```json
{
  "isSuccess": true,
  "data": {
    "registrationId": "guid",
    "enrollmentId": "guid",
    "trackRequested": "primary",
    "trackResolved": "primary",
    "formTypeRequested": "auto",
    "formTypeResolved": "newStudent",
    "canGenerate": true,
    "paymentSettingScope": "branch",
    "warnings": [],
    "activePdf": {
      "pdfRecordId": "guid",
      "pdfUrl": "https://...",
      "generatedAt": "2026-04-28T11:00:00Z",
      "generatedBy": "guid",
      "generatedByName": "Staff A",
      "isActive": true,
      "hasSnapshot": true
    },
    "preview": {
      "studentName": "Nguyen Van A",
      "branchName": "HCM",
      "programName": "Apple",
      "classCode": "APPLE-A2",
      "classTitle": "Apple A2",
      "tuitionPlanName": "Goi 48 buoi",
      "totalSessions": 48,
      "tuitionAmount": 30000000,
      "unitPriceSession": 625000,
      "discountAmount": 3000000,
      "carryOverCreditAmount": 0,
      "materialFee": 0,
      "totalPayment": 27000000,
      "currency": "VND",
      "paymentMethod": "Chuyen khoan",
      "paymentQrUrl": "https://..."
    }
  }
}
```

Response loi hay gap:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Registration khong ton tai |
| 400 | `Registration.InvalidEnrollmentConfirmationPdfFormType` | `formType` khong hop le |
| 404 | `Registration.EnrollmentNotFound` | Chua co enrollment active cho track can preview |
| 400 | Validation/NotFound khac | Thieu payment setting hoac thieu du lieu bat buoc de build preview |

Ghi chu:

- `preview.tuitionAmount` hien thi gia goc snapshot cua registration.
- `preview.totalPayment` = `tuitionAmount - discountAmount - carryOverCreditAmount + materialFee`.

### 8. POST `/api/registrations/{id}/enrollment-confirmation-pdf`

Dung de generate file PDF enrollment confirmation va tra ve pricing da ap dung.

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
    "pdfGeneratedAt": "2026-04-28T11:30:00Z",
    "reusedExistingPdf": false,
    "enrollDate": "2026-05-03",
    "firstStudyDate": "2026-05-03",
    "studentName": "Nguyen Van A",
    "classCode": "APPLE-A2",
    "classTitle": "Apple A2",
    "programName": "Apple",
    "tuitionPlanName": "Goi 48 buoi",
    "tuitionAmount": 30000000,
    "discountAmount": 3000000,
    "carryOverCreditAmount": 0,
    "finalTuitionAmount": 27000000,
    "currency": "VND"
  }
}
```

Response loi:

| HTTP | Code/message | Khi nao |
| --- | --- | --- |
| 404 | `Registration.NotFound` | Registration khong ton tai |
| 400 | `Registration.InvalidEnrollmentConfirmationPdfFormType` | `formType` khong hop le |
| 404 | `Registration.EnrollmentNotFound` | Chua co enrollment active cho track |
| 500/400 | `Registration.EnrollmentConfirmationPdfGenerationFailed` | Loi render PDF, luu file, hoac storage |

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

### OperationType

| Value | Y nghia |
| --- | --- |
| `Initial` | Dang ky lan dau |
| `Renewal` | Hoc vien da tung co registration truoc do; lan nay duoc xem la renewal |
| `Upgrade` | Nang goi hoc |
| `Transfer` | Chuyen lop |
| `TransferBranch` | Chuyen chi nhanh |
| `Retake` | Thi lai |

### DiscountType

| Value | Y nghia |
| --- | --- |
| `Percentage` | Giam theo phan tram tren hoc phi goc |
| `FixedAmount` | Giam theo so tien co dinh |

### Enrollment confirmation formType

| Value | Y nghia |
| --- | --- |
| `auto` | Backend tu resolve thanh `newStudent` hoac `continuingStudent` |
| `newStudent` | Form danh cho hoc vien moi |
| `continuingStudent` | Form danh cho hoc vien tiep tuc / renewal / upgrade |

## Luong chuyen trang thai

Luong tong quat cua registration:

1. Staff tao/import registration -> `New`
2. Sau cac luong xep lop backend co the dua ve `WaitingForClass`, `ClassAssigned`, `Studying`
3. Trong qua trinh hoc co the `Paused`
4. Ket thuc khoa hoc -> `Completed`
5. Huy -> `Cancelled`

Luong pricing/discount:

1. FE tao discount campaign o controller rieng
2. Khi tao registration moi hoac import-active:
   - backend resolve `Initial`/`Renewal`
   - dung `RegistrationDate` de tim campaign match
   - snapshot pricing vao registration
3. Khi update doi tuition plan:
   - backend tinh lai pricing theo `RegistrationDate` goc
4. Khi upgrade:
   - backend tinh theo operation `Upgrade`
   - campaign duoc xet tai thoi diem upgrade
   - co them `carryOverCreditAmount`
5. PDF preview/generate doc pricing snapshot da luu, khong tu resolve lai campaign moi

## Permission matrix theo role

| API | Admin | ManagementStaff | Teacher | Parent | Student | Anonymous |
| --- | --- | --- | --- | --- | --- | --- |
| `POST /api/registrations` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/import-active` | Yes | Yes | No | No | No | No |
| `GET /api/registrations` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}` | Yes | Yes | No | No | No | No |
| `PUT /api/registrations/{id}` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/{id}/upgrade` | Yes | Yes | No | No | No | No |
| `GET /api/registrations/{id}/enrollment-confirmation-pdf` | Yes | Yes | No | No | No | No |
| `POST /api/registrations/{id}/enrollment-confirmation-pdf` | Yes | Yes | No | No | No | No |

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
| Khong duoc update registration da complete/cancel | Update | 400 `Registration.InvalidStatus` |
| `actualStartDate` import khong duoc o tuong lai | Import-active | 400 `Registration.ActualStartDateInFuture` |
| `usedSessions + remainingSessions` phai bang tong buoi cua tuition plan | Import-active | 400 `Registration.ImportSessionCountMismatch` |
| `remainingSessions > 0` voi import-active | Import-active | 400 validation pipeline |
| Upgrade chi ap dung cho registration active | Upgrade | 400 `Registration.NoActiveRegistrationForUpgrade` |
| `formType` PDF chi nhan `auto`, `newStudent`, `continuingStudent` | Preview/generate PDF | 400 `Registration.InvalidEnrollmentConfirmationPdfFormType` |

## Luu y FE quan trong

- Cac field pricing snapshot moi trong registration response:
  - `discountCampaignId`
  - `discountCampaignName`
  - `discountType`
  - `discountValue`
  - `originalTuitionAmount`
  - `discountAmount`
  - `carryOverCreditAmount`
  - `finalTuitionAmount`
- Discount campaign duoc chon theo:
  - campaign `isActive = true`
  - ngay dang ky nam trong `[startDate, endDate]`
  - match scope `branch/program/tuitionPlan`
  - match `Initial` / `Renewal` / `Upgrade`
  - neu nhieu campaign cung match thi lay campaign `priority` cao nhat
- `RegistrationDate` la field nghiep vu dung de xet discount; no khac voi `CreatedAt` la field audit.
- O create/import-active, backend hien tai set `RegistrationDate` va `CreatedAt` cung bang `now`.
- O update doi tuition plan, backend re-calculate discount theo `RegistrationDate` goc cua registration.
- O upgrade, backend tinh `carryOverCreditAmount` rieng va ap discount cho operation `Upgrade`.
- Sau khi pricing da duoc snapshot vao registration, viec campaign bi sua/tat ve sau khong tu dong thay doi registration cu.
