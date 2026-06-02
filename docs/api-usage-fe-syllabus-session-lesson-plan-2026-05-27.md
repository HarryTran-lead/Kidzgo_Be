# FE API Usage - Syllabus, Session, Lesson Plan, Teaching Log

Updated: 2026-05-30
Scope: 1 file FE doc gop cho flow `teacher/admin` quanh `syllabus`, `session`, `lesson plan document`, `lesson plan template`, va `teaching log`.

Base paths:

- `/api/branches`
- `/api/packages`
- `/api/package-curriculum-mappings`
- `/api/syllabuses`
- `/api/lesson-plans`
- `/api/lesson-plan-templates`
- `/api/sessions`

---

## 1. Muc tieu FE

Sau cac thay doi hien tai, FE nen chay theo 1 flow don gian:

1. Di tu `classId` hoac `sessionId`
2. Lay `syllabusId` tu API runtime, khong tu suy dien local
3. Render lesson plan document theo session
4. Submit/update `teaching log`
5. Reload session va lesson plan document vi backend co the resync runtime cua cac session tuong lai

---

## 2. End-to-End Teacher Flow

Flow khuyen nghi cho man teacher:

1. Vao man class lesson plan
2. Goi `GET /api/lesson-plans/classes/{classId}/syllabus`
3. Chon session trong danh sach
4. Goi `GET /api/sessions/{sessionId}/lesson-plan-document`
5. Render document + teaching log state hien tai
6. Neu chua co teaching log, goi `POST /api/sessions/{sessionId}/teaching-log`
7. Neu da co teaching log va chua bi lock, goi `PUT /api/sessions/{sessionId}/teaching-log`
8. Sau submit/update, reload:
   - `GET /api/sessions/{sessionId}`
   - `GET /api/sessions/{sessionId}/lesson-plan-document`
   - neu dang hien session list/class progression thi reload danh sach session cua class

Ly do phai reload:

- Backend co `RecalculateAndResyncAsync(...)`
- `teaching log` co the doi runtime progression
- future sessions co the bi doi lesson/template sau khi current session duoc mark `completed`, `partial`, `skipped`

---

## 3. Branch -> Syllabus Assignment

### `GET /api/branches/{branchId}/syllabuses`

Dung de lay danh sach syllabus branch dang duoc phep dung.

Role:

- `Admin`
- `ManagementStaff`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "syllabuses": [
      {
        "curriculumAssignmentId": "uuid",
        "syllabusId": "uuid",
        "programId": "uuid",
        "programName": "Starters",
        "levelId": "uuid",
        "levelName": "Level 1",
        "code": "STARTERS",
        "version": "v3",
        "title": "Get Ready for Starters",
        "effectiveFrom": "2026-05-01T00:00:00Z",
        "effectiveTo": null,
        "isActive": true
      }
    ]
  }
}
```

FE use case:

- Sau khi user chon `branch`, FE goi API nay
- Filter tiep theo `programId` + `levelId`
- Render dropdown `syllabus`

### `PUT /api/branches/{branchId}/syllabuses`

API de assign syllabus thu cong vao branch.

Role:

- `Admin`
- `ManagementStaff`

Request body:

```json
{
  "syllabusId": "uuid",
  "effectiveFrom": "2026-05-27T00:00:00Z",
  "effectiveTo": null,
  "isActive": true
}
```

Behavior:

- Neu assignment da ton tai theo `branchId + syllabusId` thi backend update
- Neu chua ton tai thi backend create moi
- `programId` va `levelId` duoc backend lay tu syllabus, FE khong gui

### `DELETE /api/branches/{branchId}/syllabuses/{assignmentId}`

Xoa 1 branch-syllabus assignment theo `curriculumAssignmentId`.

Role:

- `Admin`
- `ManagementStaff`

Behavior:

- Chi xoa duoc khi syllabus do khong con bi class operational tai branch su dung
- Neu con class `Planned`, `Recruiting`, `Active`, `Full` hoac `Suspended`, backend tra conflict

---

## 4. Branch -> Program Assignment

### `GET /api/branches/{branchId}/programs`

Lay danh sach program dang active tai branch.

Role:

- `Admin`
- `ManagementStaff`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "programs": [
      {
        "branchProgramId": "uuid",
        "programId": "uuid",
        "programName": "Starters",
        "programCode": "STARTERS",
        "isActive": true,
        "defaultMakeupClassId": null
      }
    ]
  }
}
```

### `DELETE /api/branches/{branchId}/programs/{programId}`

Go assignment program ra khoi branch.

Role:

- `Admin`
- `ManagementStaff`

Behavior:

- Backend block neu branch/program van con class operational
- Backend block neu branch/program van con registration active/chua ket thuc
- Neu qua validate, assignment bi remove khoi `BranchPrograms`

---

## 5. Package -> Curriculum Mapping

Package hien tai duoc backend map voi `TuitionPlan`.

### `POST /api/package-curriculum-mappings`

Tao mapping giua package va syllabus.

Role:

- `Admin`
- `ManagementStaff`

Request body:

```json
{
  "packageId": "uuid",
  "syllabusId": "uuid"
}
```

Validation:

- `packageId` phai la `tuition plan` ton tai, chua bi delete
- `syllabusId` phai ton tai, active, chua bi delete
- syllabus phai cung `programId` va `levelId` voi package

### `GET /api/packages/{packageId}/syllabuses`

Lay danh sach syllabus dang duoc map vao package.

Role:

- `Admin`
- `ManagementStaff`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "tuitionPlanId": "uuid",
    "tuitionPlanName": "Starters 24 sessions",
    "syllabuses": [
      {
        "mappingId": "uuid",
        "syllabusId": "uuid",
        "programId": "uuid",
        "programName": "Starters",
        "levelId": "uuid",
        "levelName": "Level 1",
        "code": "STARTERS",
        "version": "v4",
        "title": "Get Ready for Starters",
        "isActive": true
      }
    ]
  }
}
```

---

## 6. Syllabus Discovery APIs

### `GET /api/syllabuses`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Dung de list syllabus cho search/filter/picker.

### `GET /api/syllabuses/versions`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Query params:

- `branchId: Guid?`
- `programId: Guid?`
- `levelId: Guid?`
- `activeOnly: boolean = true`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "versions": [
      {
        "syllabusId": "uuid",
        "programId": "uuid",
        "programName": "Starters",
        "levelId": "uuid",
        "levelName": "Level 1",
        "code": "STARTERS",
        "version": "v3",
        "title": "Get Ready for Starters",
        "edition": "2nd Edition",
        "isActive": true
      }
    ]
  }
}
```

FE use case:

- preload dropdown syllabus version
- tim syllabus theo `branchId/programId/levelId`

### `GET /api/syllabuses/{id}`

Dung de lay syllabus detail.

### `GET /api/syllabuses/{id}/versions`

Lay tat ca versions trong cung family cua 1 syllabus.

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Family duoc xac dinh theo:

- `programId`
- `levelId`
- `code`

### `POST /api/syllabuses/{id}/versions`

Clone 1 syllabus thanh version moi.

Role:

- `ManagementStaff`
- `Admin`

Request body:

```json
{
  "version": "v4",
  "title": "Get Ready for Starters",
  "edition": "2nd Edition",
  "effectiveFrom": "2026-06-01T00:00:00Z",
  "effectiveTo": null,
  "promoteNow": false
}
```

Behavior:

- Backend clone metadata + units + lessons + resources + session templates tu syllabus goc
- Syllabus moi bat dau o `documentStatus = Draft`
- Neu `promoteNow = true`, backend se promote version moi ngay sau khi clone

### `POST /api/syllabuses/{id}/versions/{versionId}/promote`

Promote 1 version thanh active version cua ca family.

Role:

- `ManagementStaff`
- `Admin`

Behavior:

- Tat ca sibling versions cung family bi `IsActive = false`
- Target version duoc `IsActive = true`
- Backend repoint active `BranchSyllabusAssignment` va active `PackageCurriculumMapping` tu active version cu sang version moi

### `GET /api/syllabuses/{id}/document`

Dung de lay syllabus document render JSON.

### `GET /api/syllabuses/{id}/unit-lesson-plans`

Dung de debug/import review danh sach lesson plan Word da bind vao syllabus.

### `GET /api/syllabuses/import-configuration`

Dung de lay import rule config cho 1 `programId + levelId`.

---

## 7. Class -> Syllabus For Teacher Flow

### `GET /api/lesson-plans/classes/{classId}/syllabus`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

API nay la entry point chinh de FE teacher lay `syllabusId` ngay trong class flow.

Response fields FE nen dung o root:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`
- `sourceFileName`
- `attachmentUrl`

Response fields FE nen dung trong `sessions[]`:

- `sessionId`
- `sessionIndex`
- `syllabusId`
- `moduleId`
- `sessionIndexInModule`
- `lessonPlanId`
- `templateId`
- `plannedLessonTitle`
- `plannedLessonPlanTemplateId`
- `actualLessonPlanTemplateId`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "classId": "uuid",
    "classCode": "STARTERS-Q7-01",
    "classTitle": "Starters Q7 Morning",
    "syllabusId": "uuid",
    "syllabusCode": "STARTERS",
    "syllabusVersion": "v3",
    "syllabusTitle": "Get Ready for Starters",
    "sourceFileName": "The Syllabus of Get Ready for Starters full.xlsx",
    "attachmentUrl": "https://cdn.example.com/syllabuses/get-ready-for-starters-full.xlsx",
    "programId": "uuid",
    "levelId": "uuid",
    "programName": "Starters",
    "syllabusMetadata": "2nd Edition",
    "sessions": [
      {
        "sessionId": "uuid",
        "sessionIndex": 8,
        "syllabusId": "uuid",
        "moduleId": "uuid",
        "sessionIndexInModule": 2,
        "lessonPlanId": "uuid",
        "templateId": "uuid",
        "plannedLessonTitle": "UNIT 1: I LOVE ANIMALS! - Lesson 3"
      }
    ]
  }
}
```

FE use case:

1. Teacher vao class lesson plan page
2. FE goi API nay
3. Lay `syllabusId`
4. Dung `sessions[]` de render danh sach session
5. Khi user mo 1 session, goi `GET /api/sessions/{sessionId}/lesson-plan-document`

---

## 8. Session -> Lesson Plan Document

### `GET /api/sessions/{sessionId}/lesson-plan-document`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

API nay resolve lesson plan document dung voi session runtime hien tai.

Response fields FE nen dung o root:

- `sessionId`
- `classId`
- `syllabusId`
- `moduleId`
- `moduleName`
- `sessionIndexInModule`
- `lessonPlanTemplateId`
- `plannedLessonPlanTemplateId`
- `actualLessonPlanTemplateId`
- `plannedLessonTitle`
- `actualLessonTitle`
- `teachingLogId`
- `teachingLogStatus`
- `teachingProgressStatus`

Response fields FE nen dung trong `document`:

- `id`
- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`
- `moduleId`
- `moduleCode`
- `moduleName`
- `lessonPlanUnitId`
- `lessonPlanUnitName`
- `title`
- `sessionIndex`
- `sessionOrder`
- `procedure`
- `sourceFileName`
- `attachment`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "sessionId": "uuid",
    "classId": "uuid",
    "syllabusId": "uuid",
    "moduleId": "uuid",
    "moduleName": "Unit 1",
    "sessionIndexInModule": 2,
    "lessonPlanTemplateId": "uuid",
    "plannedLessonPlanTemplateId": "uuid",
    "actualLessonPlanTemplateId": null,
    "plannedLessonTitle": "UNIT 1: I LOVE ANIMALS! - Lesson 3",
    "actualLessonTitle": null,
    "teachingLogId": null,
    "teachingLogStatus": null,
    "teachingProgressStatus": null,
    "document": {
      "id": "uuid",
      "syllabusId": "uuid",
      "syllabusCode": "STARTERS",
      "syllabusVersion": "v3",
      "syllabusTitle": "Get Ready for Starters",
      "programId": "uuid",
      "programName": "Starters",
      "levelId": "uuid",
      "levelName": "Level 1",
      "moduleId": "uuid",
      "moduleCode": "UNIT-1",
      "moduleName": "Unit 1",
      "title": "UNIT 1: I LOVE ANIMALS! - Lesson 3",
      "sessionIndex": 3,
      "sessionOrder": 3,
      "procedure": "...",
      "sourceFileName": "Unit 1 lesson 3.docx"
    }
  }
}
```

FE use case:

- Day la API FE nen dung de render lesson plan document o man session detail
- Khong can doan `syllabusId` hay `templateId` bang cache local
- Neu `teachingLogId != null`, FE co the bat che do edit teaching log

---

## 9. Teaching Log APIs

Teaching log dung base path:

- `POST /api/sessions/{sessionId}/teaching-log`
- `GET /api/sessions/{sessionId}/teaching-log`
- `PUT /api/sessions/{sessionId}/teaching-log`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

### 7.1 Request Model

Request body cho `POST` va `PUT`:

```json
{
  "actualLessonPlanTemplateId": "uuid-or-null",
  "actualTeachingType": "Normal",
  "progressStatus": "completed",
  "actualContent": "Covered warm-up, vocabulary and speaking drill.",
  "actualHomework": "Workbook page 12",
  "teacherNote": "Students finished as planned."
}
```

Field meaning:

- `actualLessonPlanTemplateId`
  - optional
  - neu `null`, backend se fallback sang planned template
- `actualTeachingType`
  - optional string
  - parse khong duoc thi backend fallback `Normal`
- `progressStatus`
  - bat buoc
  - valid input values:
    - `completed`
    - `partial`
    - `not_started`
    - `skipped`
- `actualContent`
  - noi dung thuc te da day
- `actualHomework`
  - bai tap duoc giao
- `teacherNote`
  - ghi chu giao vien
  - bat buoc co y nghia khi `progressStatus = skipped`

Valid `actualTeachingType` values tu enum:

- `Normal`
- `Review`
- `Test`
- `Makeup`
- `Event`
- `Other`

### 7.2 Progress Mapping FE Can Not Ignore

Request va response khong dung cung 1 naming.

Request `progressStatus`:

- `completed`
- `partial`
- `not_started`
- `skipped`

Backend map sang domain status:

- `completed` -> `Completed`
- `partial` -> `Partial`
- `not_started` -> `Planned`
- `skipped` -> `Skipped`

Rule runtime:

- `Completed` consume lesson
- `Skipped` consume lesson
- `Partial` khong consume lesson
- `Planned` khong consume lesson

He qua cho FE:

- Neu FE luu enum local, can mapping 2 chieu
- Khong duoc assume response se tra lai `not_started`
- Response `teachingProgressStatus`/`progressStatus` hien tai se la `Completed|Partial|Planned|Skipped`

### 7.3 `POST /api/sessions/{sessionId}/teaching-log`

Dung khi session chua co teaching log.

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "teachingLogId": "uuid",
    "sessionId": "uuid",
    "plannedLessonPlanTemplateId": "uuid",
    "actualLessonPlanTemplateId": "uuid",
    "actualTeachingType": "Normal",
    "progressStatus": "Completed",
    "classId": "uuid",
    "currentModuleId": "uuid",
    "currentSessionIndex": 8,
    "currentLessonPlanTemplateId": "uuid",
    "updatedFutureSessionCount": 5
  }
}
```

Behavior quan trong:

- Session bi set `Completed`
- `ActualDatetime` duoc backend fill neu dang null
- Teaching log duoc tao voi `Status = Submitted`
- Backend resync future sessions trong class

### 7.4 `GET /api/sessions/{sessionId}/teaching-log`

Dung de reload teaching log detail, nhat la sau khi submit/update.

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "teachingLogId": "uuid",
    "sessionId": "uuid",
    "plannedLessonPlanTemplateId": "uuid",
    "plannedLessonTitle": "UNIT 1: I LOVE ANIMALS! - Lesson 3",
    "actualLessonPlanTemplateId": "uuid",
    "actualLessonTitle": "UNIT 1: I LOVE ANIMALS! - Lesson 3",
    "teachingLogStatus": "Submitted",
    "progressStatus": "Completed",
    "actualTeachingType": "Normal",
    "actualContent": "Covered warm-up, vocabulary and speaking drill.",
    "actualHomework": "Workbook page 12",
    "teacherNote": "Students finished as planned.",
    "submittedBy": "uuid",
    "submittedAt": "2026-05-30T09:10:00Z",
    "updatedAt": "2026-05-30T09:10:00Z"
  }
}
```

FE use case:

- open edit modal/form teaching log
- reload sau save
- hien audit metadata `submittedAt`, `updatedAt`

### 7.5 `PUT /api/sessions/{sessionId}/teaching-log`

Dung khi teaching log da ton tai va van duoc sua.

Request body giong `POST`.

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "teachingLogId": "uuid",
    "sessionId": "uuid",
    "classId": "uuid",
    "plannedLessonPlanTemplateId": "uuid",
    "actualLessonPlanTemplateId": "uuid",
    "actualTeachingType": "Review",
    "progressStatus": "Partial",
    "currentModuleId": "uuid",
    "currentSessionIndex": 8,
    "currentLessonPlanTemplateId": "uuid",
    "updatedFutureSessionCount": 4
  }
}
```

Behavior:

- Backend update teaching log hien tai
- Neu chua co lesson progress row thi backend tu tao
- Backend resync future sessions lai 1 lan nua
- Endpoint nay bi chan neu teaching log da `Approved` hoac `Locked`

### 7.6 Error Cases FE Nen Map

Teaching log:

- `409 Session.TeachingLogAlreadyExists`
- `404 Session.TeachingLogNotFound`
- `409 Session.TeachingLogLocked`
- `400 Session.InvalidTeachingProgressStatus`
- `400 Session.MissingLessonTemplateForTeachingLog`
- `400 Session.SkippedRequiresReason`

Session/runtime lien quan:

- `404 Session.NotFound`
- `400 Session.Cancelled`

FE handling goi y:

- `TeachingLogAlreadyExists`
  - doi tu mode create sang mode edit
- `TeachingLogNotFound`
  - doi tu mode edit sang mode create
- `TeachingLogLocked`
  - disable form, chi cho read-only
- `InvalidTeachingProgressStatus`
  - check enum mapping client
- `SkippedRequiresReason`
  - bat buoc user nhap `teacherNote`

### 7.6.1 Backend Backfill Note Cho Case Dang Loi

Neu gap case session runtime bi thieu linkage va FE khong save duoc teaching log, uu tien backend backfill theo thu tu nay:

1. Toi thieu de teaching log save duoc:
   - `session.plannedLessonPlanTemplateId` hoac `session.lessonPlanTemplateId` cua session `d5191119-39cf-4be1-bfbd-00952a832ef3` phai co gia tri hop le.
2. Neu backend dang derive tu `lesson_plans`:
   - `lesson_plans.templateId` cua lesson plan `ff036679-55d7-43a8-ad2f-ab841b1f8cd4` cung phai duoc set.
3. De flow chuan chay lai hoan chinh, khong chi save duoc:
   - runtime mapping cua session nay cung nen duoc backfill lai `syllabusId` de endpoint `GET /api/sessions/{sessionId}/lesson-plan-document` khong con tra `404`.

### 7.7 FE Submit Flow Khuyen Nghi

1. Goi `GET /api/sessions/{sessionId}/lesson-plan-document`
2. Neu `teachingLogId == null`, render form create
3. Build request body:
   - default `actualLessonPlanTemplateId = plannedLessonPlanTemplateId`
   - default `actualTeachingType = Normal`
4. Goi `POST /api/sessions/{sessionId}/teaching-log`
5. Sau thanh cong, reload:
   - `GET /api/sessions/{sessionId}`
   - `GET /api/sessions/{sessionId}/teaching-log`
   - `GET /api/sessions/{sessionId}/lesson-plan-document`
6. Neu man hinh co session list trong class, reload list do backend co the da resync future sessions

### 7.8 FE Edit Flow Khuyen Nghi

1. Goi `GET /api/sessions/{sessionId}/teaching-log`
2. Bind form tu response
3. Neu `teachingLogStatus` la `Approved` hoac `Locked`, render read-only
4. Goi `PUT /api/sessions/{sessionId}/teaching-log`
5. Reload lai y nhu flow submit

---

## 10. Session Detail API Huu Ich Cho FE

### `GET /api/sessions/{sessionId}`

Action nay nam trong controller `[Authorize]`.

Field FE nen dung khi debug runtime:

- `session.lessonPlanId`
- `session.lessonPlanTemplateId`
- `session.plannedLessonPlanTemplateId`
- `session.actualLessonPlanTemplateId`
- `session.sessionIndexInModule`
- `session.plannedLessonTitle`
- `session.actualLessonTitle`
- `session.moduleId`
- `session.moduleName`
- `session.teachingLogId`
- `session.teachingLogStatus`
- `session.teachingProgressStatus`
- `session.actualTeachingType`
- `session.actualContent`
- `session.actualHomework`
- `session.teacherNote`

Use case:

- Dong bo state sau submit/update teaching log
- Debug mismatch giua lesson plan document va teaching log

---

## 11. Lesson Plan Template APIs

### `GET /api/lesson-plan-templates`

Query params chinh:

- `syllabusId: Guid?`
- `moduleId: Guid?`
- `title: string?`
- `isActive: bool?`
- `includeDeleted: bool = false`
- `pageNumber: int = 1`
- `pageSize: int = 10`

FE use case:

- sau khi co `syllabusId`, list template theo syllabus/module
- cho user doi `actualLessonPlanTemplateId` neu can day khac template planned

### `GET /api/lesson-plan-templates/{id}`

FE use case:

- mo detail 1 template cu the
- debug mapping session -> template

Note:

- Read path hien tai uu tien canonical mapping, nen session/lesson-plan/document flow se on dinh hon truoc

---

## 12. Lesson Plan APIs

### `GET /api/lesson-plans/{lessonPlanId}`

Field FE nen dung:

- `id`
- `classId`
- `sessionId`
- `templateId`
- `templateSessionIndex`
- `plannedContent`
- `actualContent`
- `actualHomework`
- `teacherNotes`

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid",
    "classId": "uuid",
    "sessionId": "uuid",
    "templateId": "uuid",
    "templateSessionIndex": 3,
    "plannedContent": "...",
    "actualContent": null,
    "actualHomework": null,
    "teacherNotes": null
  }
}
```

Note:

- API nay huu ich cho debug va parity check
- Man teacher session page nen uu tien `GET /api/sessions/{sessionId}/lesson-plan-document`

---

## 13. Import APIs FE Van Dung

### `POST /api/syllabuses/import-archive`

Response da co them metadata de FE debug import:

- `archiveFileName`
- `archiveParserVersion`
- `syllabusId`
- `selectedSyllabusEntryName`
- `selectedSyllabusNormalizedEntryName`
- `selectedSyllabusFileName`
- `selectedSyllabusSourceType`
- `selectedSyllabusParserVersion`
- `importedEntries[]`
- `skippedItems[]`

Neu gui them `branchId`, backend se auto gan syllabus vao branch sau import.

### `POST /api/syllabuses/import-lesson-plan-words`

Query bat buoc:

- `programId`
- `levelId`
- `syllabusId`
- `moduleId?`
- `overwriteExisting`

FE note:

- `syllabusId` la bat buoc
- import mapping hien duoc uu tien theo `file name + import configuration`

---

## 14. Audit Notes

Nhung diem audit FE co the xem la da duoc backend ho tro o thoi diem update doc nay:

- `GET /api/classes/{id}/capacity`
- `GET /api/sessions?branchId=...`
- `GET /api/classes/{id}` da co:
  - `scheduleText`
  - `completedSessions`
  - `progressPercent`
- Lesson plan units da co controller rieng: `LessonPlanUnitController`
- Migration cho lesson plan units da ton tai trong repo: `20260520165014_AddLessonPlanUnits`
- Student branch management da duoc tach thanh doc rieng:
  - `docs/student-branch-management-plan-2026-06-02.md`

---

## 15. Quick FE Checklist

1. Neu dang o flow teacher theo class/session, luon lay `syllabusId` tu API response moi.
2. Uu tien `GET /api/sessions/{sessionId}/lesson-plan-document` de render man session.
3. Xem `teachingLogId` de quyet dinh create hay edit.
4. Mapping dung `progressStatus` input va output, vi 2 ben dang khac format.
5. Sau moi lan submit/update teaching log, reload session + lesson-plan-document + session list neu dang hien runtime progression.
6. Neu `updatedFutureSessionCount > 0`, FE nen coi cac session tuong lai co the da doi planned lesson/template.
7. Neu user chon `skipped`, bat buoc cho nhap `teacherNote`.
8. Neu teaching log `Approved` hoac `Locked`, FE chi render read-only.
