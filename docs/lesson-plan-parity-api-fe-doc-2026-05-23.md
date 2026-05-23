# Lesson Plan Parity API Usage For FE

Updated: 2026-05-24
Scope: teacher/admin lesson-plan parity theo `sessionId`
Base path: `/api/sessions`, `/api/lesson-plans`, `/api/lesson-plan-templates`
Roles:

- `Teacher`
- `Admin`
- `ManagementStaff`

## 1. Muc tieu

FE can render cung mot lesson-plan document cho cung `sessionId` tren ca Teacher va Admin.

Backend da bo sung 2 huong dung:

1. Dung `GET /api/sessions/{sessionId}/lesson-plan-document` de lay document da resolve san
2. Hoac tiep tuc flow cu bang session detail/list + template detail, nhung voi linkage fields da dong bo

Khuyen nghi FE:

- Uu tien `GET /api/sessions/{sessionId}/lesson-plan-document`
- Chi fallback sang cac endpoint khac neu can support data cu

## 2. Envelope

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
  "success": false,
  "isSuccess": false,
  "data": null,
  "message": "Lesson plan template linkage is missing for session 'uuid' in class 'uuid' and module 'uuid'.",
  "errors": [
    {
      "code": "Session.LessonPlanTemplateMissing",
      "description": "Lesson plan template linkage is missing for session 'uuid' in class 'uuid' and module 'uuid'."
    }
  ]
}
```

## 3. Thu tu FE nen dung

### Cach khuyen nghi

1. Gọi `GET /api/sessions/{sessionId}/lesson-plan-document`
2. Render `data.document`
3. Dung cac field linkage o root de hien title/status/runtime info

### Cach tuong thich voi flow cu

1. Gọi `GET /api/sessions/{sessionId}`
2. Lay `session.lessonPlanTemplateId`
3. Gọi `GET /api/lesson-plan-templates/{lessonPlanTemplateId}`
4. Render document tu template

## 4. Session Detail

### `GET /api/sessions/{sessionId}`

Field quan trong FE can dung:

```json
{
  "session": {
    "id": "uuid",
    "classId": "uuid",
    "moduleId": "uuid",
    "moduleName": "Unit 4",
    "lessonPlanTemplateId": "uuid",
    "plannedLessonPlanTemplateId": "uuid",
    "actualLessonPlanTemplateId": "uuid-or-null",
    "sessionIndexInModule": 3,
    "plannedLessonTitle": "Unit 4 - Lesson 3",
    "actualLessonTitle": "Unit 4 - Lesson 3",
    "teachingLogId": "uuid-or-null",
    "teachingLogStatus": "Submitted",
    "teachingProgressStatus": "Partial",
    "actualTeachingType": "Review"
  }
}
```

Rule:

- `lessonPlanTemplateId` la canonical template ID FE nen dung de render document
- `plannedLessonPlanTemplateId` la lesson planned source
- `actualLessonPlanTemplateId` la lesson thuc te neu teacher da doi lesson
- `actualLessonPlanTemplateId = null` nghia la session van dung planned lesson
- Backend khong tra `Guid.Empty` lam linkage hop le

## 5. Session List

### `GET /api/sessions`

Field moi FE can dung tren list:

```json
{
  "sessions": {
    "items": [
      {
        "id": "uuid",
        "classId": "uuid",
        "moduleId": "uuid",
        "moduleName": "Unit 4",
        "lessonPlanTemplateId": "uuid",
        "plannedLessonPlanTemplateId": "uuid",
        "actualLessonPlanTemplateId": "uuid-or-null",
        "sessionIndexInModule": 3,
        "plannedLessonTitle": "Unit 4 - Lesson 3",
        "actualLessonTitle": "Unit 4 - Lesson 3",
        "teachingLogId": "uuid-or-null",
        "teachingLogStatus": "Submitted",
        "teachingProgressStatus": "Partial",
        "actualTeachingType": "Review"
      }
    ]
  }
}
```

Consistency expectation:

- `lessonPlanTemplateId` o list va detail phai giong nhau cho cung `sessionId`
- FE co the cache linkage ngay tu list, nhung khi vao trang chi tiet van nen tin `GET /api/sessions/{id}` hoac endpoint dedicated

## 6. Dedicated Lesson Plan Document

### `GET /api/sessions/{sessionId}/lesson-plan-document`

Day la endpoint FE nen uu tien.

Response:

```json
{
  "sessionId": "uuid",
  "classId": "uuid",
  "moduleId": "uuid",
  "moduleName": "Unit 4",
  "sessionIndexInModule": 3,
  "lessonPlanTemplateId": "uuid",
  "plannedLessonPlanTemplateId": "uuid",
  "actualLessonPlanTemplateId": "uuid-or-null",
  "plannedLessonTitle": "Unit 4 - Lesson 3",
  "actualLessonTitle": "Unit 4 - Lesson 3",
  "teachingLogId": "uuid-or-null",
  "teachingLogStatus": "Submitted",
  "teachingProgressStatus": "Partial",
  "document": {
    "id": "uuid",
    "moduleId": "uuid",
    "moduleCode": "UNIT_4",
    "moduleName": "Unit 4",
    "lessonPlanUnitId": "uuid-or-null",
    "lessonPlanUnitName": "Unit 4",
    "orderIndexInUnit": 3,
    "levelId": "uuid",
    "levelName": "Starters",
    "programId": "uuid",
    "programName": "Kids English",
    "title": "Unit 4 - Lesson 3",
    "sessionIndex": 3,
    "sessionOrder": 3,
    "syllabusMetadata": "...",
    "syllabusContent": "...",
    "objectives": "...",
    "languageContent": "...",
    "vocabulary": "...",
    "grammar": "...",
    "teachingMethodology": "...",
    "teacherMaterials": "...",
    "studentMaterials": "...",
    "procedure": "...",
    "evaluation": "...",
    "sourceFileName": "Unit 4 lesson 3.docx",
    "attachment": "https://...",
    "isActive": true,
    "createdBy": "uuid-or-null",
    "createdByName": "Admin A",
    "createdAt": "2026-05-23T08:00:00Z",
    "updatedAt": "2026-05-23T08:30:00Z"
  }
}
```

FE usage:

1. Render document tu `data.document`
2. Dung `lessonPlanTemplateId` o root lam resolved template id
3. Dung `plannedLessonTitle`, `actualLessonTitle`, `teachingLogStatus`, `teachingProgressStatus` de hien runtime badge/title

## 7. Template Detail

### `GET /api/lesson-plan-templates/{id}`

Teacher da co the doc endpoint nay neu template thuoc session/class ma teacher co quyen access.

Response shape van giong truoc:

```json
{
  "id": "uuid",
  "moduleId": "uuid",
  "moduleCode": "UNIT_4",
  "moduleName": "Unit 4",
  "title": "Unit 4 - Lesson 3",
  "sessionIndex": 3,
  "sessionOrder": 3,
  "syllabusMetadata": "...",
  "syllabusContent": "...",
  "objectives": "...",
  "languageContent": "...",
  "vocabulary": "...",
  "grammar": "...",
  "teachingMethodology": "...",
  "teacherMaterials": "...",
  "studentMaterials": "...",
  "procedure": "...",
  "evaluation": "..."
}
```

FE chi nen goi endpoint nay khi:

- dang dung flow cu theo `lessonPlanTemplateId`
- hoac can mo man hinh template detail rieng

## 8. Class Lesson Plan Syllabus

### `GET /api/lesson-plans/classes/{classId}/syllabus`

Endpoint nay van dung duoc cho runtime mapping theo class.

Field lien quan den parity:

```json
{
  "classId": "uuid",
  "classCode": "CLS_STARTERS_02",
  "classTitle": "Starters 02",
  "programId": "uuid",
  "programName": "Kids English",
  "syllabusMetadata": "...",
  "sessions": [
    {
      "sessionId": "uuid",
      "sessionIndex": 12,
      "moduleId": "uuid",
      "sessionIndexInModule": 3,
      "sessionDate": "2026-06-10T18:00:00",
      "rowRef": "session:uuid",
      "unitName": "Unit 4",
      "lessonTitle": "Unit 4 - Lesson 3",
      "lessonPlanId": "uuid-or-null",
      "templateId": "uuid",
      "plannedLessonPlanTemplateId": "uuid",
      "actualLessonPlanTemplateId": "uuid-or-null",
      "templateTitle": "Unit 4 - Lesson 3",
      "plannedLessonTitle": "Unit 4 - Lesson 3",
      "actualLessonTitle": "Unit 4 - Lesson 3",
      "templateSyllabusContent": "...",
      "plannedContent": "...",
      "actualContent": "...",
      "actualHomework": "...",
      "teacherNotes": "...",
      "canEdit": true
    }
  ]
}
```

Rule:

- `templateId` tren endpoint nay da duoc resolve theo cung logic voi session detail
- `rowRef`, `unitName`, `lessonTitle` la mapping fields FE co the dung de highlight overview -> detail ma khong can local merge

## 9. Error Codes FE Nen Handle

- `Session.LessonPlanTemplateMissing`
- `Session.LessonPlanTemplateInconsistent`
- `Session.CurriculumMappingMissing`
- `Session.LessonPlanDocumentNotFound`

Goi y UI:

### `Session.LessonPlanTemplateMissing`

- Hien message: session chua co lesson-plan mapping hop le
- Co the fallback sang plain text neu FE con support legacy

### `Session.LessonPlanTemplateInconsistent`

- Hien message data linkage dang lech giua cac nguon
- Khong nen tu merge local

### `Session.CurriculumMappingMissing`

- Hien message runtime mapping khong resolve duoc theo `moduleId + sessionIndexInModule`

### `Session.LessonPlanDocumentNotFound`

- Hien message template ID da resolve nhung document khong ton tai hoac da bi xoa

## 10. FE Implementation Notes

1. Neu co `GET /api/sessions/{sessionId}/lesson-plan-document`, dung endpoint nay lam source chinh.
2. Neu van dung flow cu, luon uu tien `session.lessonPlanTemplateId` thay vi tu so sanh `planned` va `actual`.
3. Khong xem `Guid.Empty` la template ID hop le.
4. `actualLessonPlanTemplateId` chi dung de hien runtime change; khong bat FE phai bo qua `lessonPlanTemplateId`.
5. Khi reload session sau submit/update teaching log, overwrite local linkage bang response moi nhat tu BE.
6. Plain-text fallback chi nen dung cho legacy session that su khong resolve duoc document.

## 11. Chot Ngan Cho FE

FE chi can nho 5 diem:

1. `lessonPlanTemplateId` tren session payload da la resolved canonical ID.
2. Teacher va Admin phai nhin cung document cho cung `sessionId`.
3. Endpoint de dung nhat la `GET /api/sessions/{sessionId}/lesson-plan-document`.
4. `plannedLessonPlanTemplateId` va `actualLessonPlanTemplateId` la field bo sung de hien runtime state.
5. Neu BE tra 1 trong 4 error code moi, FE nen hien explicit state thay vi silently fallback.

---

## 12. BE Syllabus Parity Completion Checklist

Updated: 2026-05-24
Scope: admin full syllabus + syllabus tung buoi theo `classId` va `sessionId`

### 12.1 Muc tieu

FE hien co 2 lop hien thi:

- Full syllabus: xem toan bo syllabus cua lop/chuong trinh
- Session syllabus: drill-down vao tung buoi hoc

De hoan thien parity, BE can bao dam mapping giua syllabus tong va session detail la on dinh, khong phu thuoc merge local o FE.

### 12.2 BE can bo sung hoac xac nhan

#### Full syllabus payload

Endpoint full syllabus cua lop can tra du:

- `classId`, `classCode`, `classTitle`
- `programId`, `programName`
- `syllabusMetadata`
- `sessions[]`

Moi session nen co toi thieu:

- `sessionId`
- `sessionIndex`
- `moduleId`
- `sessionIndexInModule`
- `sessionDate`
- `lessonPlanId`
- `templateId`
- `templateTitle`
- `plannedContent`
- `actualContent`
- `actualHomework`
- `teacherNotes`
- `canEdit`

#### Canonical session document

Can endpoint on dinh:

- `GET /api/sessions/{sessionId}/lesson-plan-document`

Response nen co:

- root linkage fields: `sessionId`, `classId`, `moduleId`, `lessonPlanTemplateId`, `plannedLessonPlanTemplateId`, `actualLessonPlanTemplateId`
- runtime fields: `plannedLessonTitle`, `actualLessonTitle`, `teachingLogStatus`, `teachingProgressStatus`
- `document` la object template da resolve san

#### Mapping full syllabus -> session

BE nen tra them mot mapping chuan de FE highlight tu full syllabus sang session detail, vi du:

- `sessionId`
- `moduleId`
- `sessionIndexInModule`
- `templateId`
- `periodFrom` / `periodTo` hoac `rowRef`
- `unitName`
- `lessonTitle`

Neu chua muon them endpoint moi, mapping nay phai xuat hien nhat quan trong payload syllabus hien co.

#### Khong dung 404 cho case du lieu thieu

Neu session ton tai nhung chua resolve duoc document, BE nen tra business error thay vi 404 ha tang.

Khuyen nghi error code:

- `Session.LessonPlanTemplateMissing`
- `Session.LessonPlanTemplateInconsistent`
- `Session.CurriculumMappingMissing`
- `Session.LessonPlanDocumentNotFound`

### 12.3 Quy tac du lieu

- `lessonPlanTemplateId` phai la canonical id cho cung mot `sessionId`
- `plannedLessonPlanTemplateId` chi la planned source
- `actualLessonPlanTemplateId` chi la runtime override khi co thay doi thuc te
- Khong dung `Guid.Empty` lam linkage hop le
- Full syllabus va session detail phai tro ve cung mot template cho cung `sessionId`

### 12.4 Acceptance criteria

1. Admin mo full syllabus thay toan bo syllabus cua lop.
2. Click tung buoi tu syllabus tong thi mo dung session detail.
3. Teacher va Admin nhin cung document voi cung `sessionId`.
4. Endpoint dedicated tra `200` on dinh tren moi truong local/dev/staging.
5. Khi khong resolve duoc mapping, BE tra error nghiep vu ro rang.

### 12.5 Ghi chu cho FE/BE phoi hop

FE hien da co fallback nhung chi nen xem la phuong an tam. Muc tieu cuoi cung van la:

- Full syllabus dung cho overview
- Session document dung cho drill-down
- Canonical template linkage phai dong bo giua list, detail va dedicated endpoint
