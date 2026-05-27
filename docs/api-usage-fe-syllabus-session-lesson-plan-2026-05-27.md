# FE API Usage - Syllabus, Session, Lesson Plan

Updated: 2026-05-27
Scope: cac API moi hoac da doi contract de FE dung cho flow teacher/admin quanh `syllabus`, `session`, `lesson plan`, `lesson plan template`.

Base paths:

- `/api/branches`
- `/api/syllabuses`
- `/api/lesson-plans`
- `/api/lesson-plan-templates`
- `/api/sessions`

---

## 1. Muc tieu FE

Sau cac thay doi nay, FE co the chay chung mot flow cho admin va teacher:

1. Tim `syllabusId` tu class/session flow
2. Goi tiep syllabus APIs de lay document/template/version
3. Neu can, branch co the duoc assign syllabus thu cong bang API rieng

---

## 2. Branch -> Syllabus Assignment

### `GET /api/branches/{branchId}/syllabuses`

Dung de lay danh sach syllabus branch dang duoc phep dung.

Role:

- `Admin`
- `ManagementStaff`

Response:

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

API moi de assign syllabus thu cong vao branch.

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

FE note:

- API nay phu hop cho man admin/staff quan ly syllabus theo branch
- Teacher khong can goi API nay

---

## 3. Syllabus Discovery APIs

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

Response:

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
- tim syllabus theo `branch/program/level`

### `GET /api/syllabuses/{id}`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Dung de lay syllabus detail.

### `GET /api/syllabuses/{id}/document`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Dung de lay syllabus document render JSON.

### `GET /api/syllabuses/{id}/unit-lesson-plans`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Dung de debug/import review danh sach lesson plan Word da bind vao syllabus.

### `GET /api/syllabuses/import-configuration`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Dung de lay import rule config cho 1 `programId + levelId`.

---

## 4. Class -> Syllabus For Teacher Flow

### `GET /api/lesson-plans/classes/{classId}/syllabus`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

API nay da doi contract de FE teacher lay duoc `syllabusId` ngay trong class flow.

Response fields moi o root:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

Response fields moi trong tung `sessions[]`:

- `syllabusId`

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
4. Goi tiep:
   - `GET /api/syllabuses/{syllabusId}`
   - `GET /api/syllabuses/{syllabusId}/document`
   - `GET /api/lesson-plan-templates?...`

---

## 5. Session -> Lesson Plan Document

### `GET /api/sessions/{sessionId}/lesson-plan-document`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

API nay da doi contract de FE resolve duoc syllabus/template dung voi session hien tai.

Response fields moi o root:

- `syllabusId`

Response fields moi trong `document`:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

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
    "plannedLessonTitle": "UNIT 1: I LOVE ANIMALS! - Lesson 3",
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

- Man session detail cua teacher chi can goi API nay de render lesson plan document
- Khong can doan `syllabusId` bang cache local hay mapping rieng

---

## 6. Lesson Plan Template APIs

### `GET /api/lesson-plan-templates`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

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

### `GET /api/lesson-plan-templates/{id}`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

FE use case:

- mo detail 1 template cu the
- debug mapping session -> template

Note:

- Backend da uu tien canonical mapping o read path, nen session/lesson-plan/document flow se co xu huong tro ve template dung hon truoc

---

## 7. Lesson Plan APIs

### `GET /api/lesson-plans/{lessonPlanId}`

Role:

- `Teacher`
- `ManagementStaff`
- `Admin`

Response field FE nen dung:

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

- Backend da co logic resolve template canonical cho cac case mapping cu bi lech

### `GET /api/sessions/{sessionId}`

Role:

- current controller khong dat role attribute rieng tren action, nhung dang nam trong controller `[Authorize]`

Field FE nen dung khi can debug:

- `session.lessonPlanId`
- `session.lessonPlanTemplateId`
- `session.plannedLessonPlanTemplateId`
- `session.sessionIndexInModule`
- `session.plannedLessonTitle`
- `session.moduleId`
- `session.moduleName`

---

## 8. Import APIs FE Can Still Use

### `POST /api/syllabuses/import-archive`

Khong phai API moi, nhung response da co them metadata de FE debug import:

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
- import mapping hien duoc uu tien theo `file name + import configuration`, khong con tin mu quang vao header loi trong file Word

---

## 9. Quick FE Checklist

1. Neu dang o flow teacher theo class/session, luon lay `syllabusId` tu API response moi, khong tu suy ra.
2. Sau khi co `syllabusId`, goi tiep syllabus APIs hoac lesson-plan-template APIs.
3. Neu can man assign syllabus cho branch, dung `PUT /api/branches/{branchId}/syllabuses`.
4. Man admin/staff create class nen load danh sach syllabus branch tu `GET /api/branches/{branchId}/syllabuses` hoac `GET /api/syllabuses/versions`.
5. Man debug import nen hien them `selectedSyllabusFileName`, `selectedSyllabusParserVersion`, `skippedItems[]`.

