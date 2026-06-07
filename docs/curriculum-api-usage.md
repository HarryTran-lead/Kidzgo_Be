# Curriculum API Usage

Updated date: 2026-05-20

## 1. Scope

Tai lieu nay mo ta dung contract hien tai cua backend cho nhom curriculum:

- `Syllabus`
- `LessonPlanTemplate`
- `LessonPlan`

Tai lieu nay khong cover cac nhom API khac nhu `Program`, `Level`, `Module`, `Class`, `Session`.

## 2. Auth And Response

Tat ca endpoint trong file nay deu can `Authorization: Bearer <token>`.

Role theo backend:

- `Syllabus`: `Admin`, `ManagementStaff`
- `LessonPlanTemplate`: `Admin`, `ManagementStaff`
- `LessonPlan`: `Teacher`, `Admin`, `ManagementStaff`

Success envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error business envelope:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Syllabus.InvalidImportFile",
  "status": 400,
  "detail": "The Word document body is empty.",
  "errors": [
    {
      "code": "Syllabus.InvalidImportFile",
      "description": "The Word document body is empty."
    }
  ]
}
```

Auth errors:

- `401`: token thieu, sai, het han
- `403`: da authenticate nhung khong du role/quyen

## 3. Paging Shape

Backend dung wrapper `Page<T>` voi shape:

```json
{
  "items": [],
  "pageNumber": 1,
  "totalPages": 1,
  "totalCount": 0,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

## 4. Syllabus APIs

Base path: `/api/syllabuses`

### 4.1 Create syllabus

- Method: `POST`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `application/json`

Request body:

```json
{
  "programId": "guid",
  "levelId": "guid",
  "code": "GET_READY_STARTER",
  "version": 1,
  "title": "The Syllabus of Get Ready for Starters",
  "edition": "Second edition",
  "effectiveFrom": "2026-05-19T00:00:00+07:00",
  "effectiveTo": null,
  "pacingSchemeJson": null,
  "overview": "Curriculum overview",
  "overallObjectives": null,
  "specificObjectives": null,
  "ethicsAndAttitudes": null,
  "bookOverview": null,
  "totalPeriods": 72,
  "minutesPerPeriod": 90,
  "totalLessons": 72,
  "sourceFileName": null,
  "attachmentUrl": null,
  "rawContentJson": null,
  "isActive": true
}
```

Success data:

```json
{
  "id": "guid",
  "programId": "guid",
  "levelId": "guid",
  "code": "GET_READY_STARTER",
  "version": 1,
  "title": "The Syllabus of Get Ready for Starters",
  "isActive": true
}
```

Common errors:

- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.DuplicateVersion`

### 4.2 List syllabuses

- Method: `GET`
- Role: `Admin`, `ManagementStaff`

Query params:

- `programId?: Guid`
- `levelId?: Guid`
- `searchTerm?: string`
- `isActive?: boolean`
- `includeDeleted?: boolean = false`
- `pageNumber?: int = 1`
- `pageSize?: int = 10`

Success data:

```json
{
  "syllabuses": {
    "items": [
      {
        "id": "guid",
        "programId": "guid",
        "programName": "Get Ready",
        "levelId": "guid",
        "levelName": "Starter",
        "code": "GET_READY_STARTER",
        "version": 1,
        "title": "The Syllabus of Get Ready for Starters",
        "isActive": true,
        "unitCount": 15,
        "sessionTemplateCount": 72,
        "createdAt": "2026-05-19T03:00:00Z"
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 4.3 Get syllabus detail

- Method: `GET`
- URL: `/api/syllabuses/{id}`
- Role: `Admin`, `ManagementStaff`

Success data:

```json
{
  "id": "guid",
  "programId": "guid",
  "programName": "Get Ready",
  "levelId": "guid",
  "levelName": "Starter",
  "code": "GET_READY_STARTER",
  "version": 1,
  "title": "The Syllabus of Get Ready for Starters",
  "edition": "Second edition",
  "effectiveFrom": "2026-05-19T00:00:00+07:00",
  "effectiveTo": null,
  "pacingSchemeJson": null,
  "overview": "Curriculum overview",
  "overallObjectives": null,
  "specificObjectives": null,
  "ethicsAndAttitudes": null,
  "bookOverview": null,
  "totalPeriods": 72,
  "minutesPerPeriod": 90,
  "totalLessons": 72,
  "sourceFileName": "ppct.docx",
  "attachmentUrl": null,
  "rawContentJson": "{...}",
  "isActive": true,
  "units": [
    {
      "id": "guid",
      "moduleId": "guid",
      "moduleName": "Unit 1",
      "name": "UNIT 1: HELLO",
      "allocatedPeriods": 6,
      "lessonCount": 3,
      "orderIndex": 1,
      "notes": null
    }
  ],
  "lessons": [
    {
      "id": "guid",
      "moduleId": "guid",
      "moduleName": "Unit 1",
      "periodFrom": 1,
      "periodTo": 2,
      "topic": "UNIT 1: HELLO",
      "lessonNumber": 1,
      "contentSummary": "Content",
      "structureSummary": "Structure",
      "studentBookPages": "4-5",
      "teacherBookPages": "10-11",
      "orderIndex": 1
    }
  ],
  "resources": [
    {
      "id": "guid",
      "documentName": "Teacher Book",
      "abbreviation": "TB",
      "intendedUsers": "Teacher",
      "notes": null,
      "orderIndex": 1
    }
  ],
  "sessionTemplates": [
    {
      "id": "guid",
      "moduleId": "guid",
      "moduleName": "Unit 1",
      "lessonPlanTemplateId": "guid",
      "sessionIndex": 1,
      "sessionIndexInModule": 1,
      "lessonNumber": 1,
      "title": "UNIT 1: HELLO",
      "topic": "UNIT 1: HELLO",
      "objectiveSummary": "Content",
      "vocabularySummary": "hello, bye",
      "grammarSummary": "This is ...",
      "orderIndex": 1
    }
  ]
}
```

Common errors:

- `Syllabus.NotFound`

### 4.4 Update syllabus

- Method: `PUT`
- URL: `/api/syllabuses/{id}`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `application/json`

Request body:

```json
{
  "code": "GET_READY_STARTER",
  "version": 2,
  "title": "The Syllabus of Get Ready for Starters",
  "edition": "Second edition",
  "effectiveFrom": "2026-05-19T00:00:00+07:00",
  "effectiveTo": null,
  "pacingSchemeJson": null,
  "overview": "Updated overview",
  "overallObjectives": null,
  "specificObjectives": null,
  "ethicsAndAttitudes": null,
  "bookOverview": null,
  "totalPeriods": 72,
  "minutesPerPeriod": 90,
  "totalLessons": 72,
  "sourceFileName": null,
  "attachmentUrl": null,
  "rawContentJson": null,
  "isActive": true
}
```

Success data:

```json
{
  "id": "guid",
  "code": "GET_READY_STARTER",
  "version": 2,
  "title": "The Syllabus of Get Ready for Starters",
  "isActive": true
}
```

Common errors:

- `Syllabus.NotFound`
- `Syllabus.DuplicateVersion`

### 4.5 Import one syllabus Word file

- Method: `POST`
- URL: `/api/syllabuses/import-word`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `multipart/form-data`

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: int`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .docx`

Success data:

```json
{
  "syllabusId": "guid",
  "importedUnits": 15,
  "importedLessons": 72,
  "importedResources": 4,
  "importedSessionTemplates": 72
}
```

Common errors:

- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.DuplicateVersion`
- `Syllabus.UnsupportedImportFileType`
- `Syllabus.InvalidImportFile`
- inline `400` body `{ "error": "No file provided" }` neu khong gui file

Notes:

- Importer se fail neu parse ra `0` lesson.
- `TotalPeriods` duoc tinh tu lesson periods da parse.

### 4.6 Import one curriculum zip archive

- Method: `POST`
- URL: `/api/syllabuses/import-archive`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `multipart/form-data`

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: int`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .zip`

Expected zip structure:

- co file syllabus `.docx` nam trong folder co chua `PPCT`
- co cac folder `UNIT 1`, `UNIT 2`, ...
- co the co `REVISION`

Success data:

```json
{
  "syllabusId": "guid",
  "importedLessonPlans": 45,
  "skippedFiles": 2,
  "skippedEntries": [
    "UNIT 8/lesson draft.docx",
    "REVISION/Revision 03.docx: SessionIndex 25 must be between 1 and 24"
  ]
}
```

Common errors:

- `Syllabus.UnsupportedImportFileType`
- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.InvalidImportFile`
- inline `400` body `{ "error": "No file provided" }` neu khong gui file

Notes:

- Importer se tim syllabus file bang rule `path contains "PPCT" && endsWith(".docx")`.
- Cac file lesson docx se duoc map sang `Module` dua tren ten folder `UNIT n` / `REVISION`.
- File khong map duoc module se khong fail ca request, ma vao `skippedEntries`.

## 5. LessonPlanTemplate APIs

Base path: `/api/lesson-plan-templates`

### 5.1 Create lesson plan template

- Method: `POST`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `application/json`

Request body:

```json
{
  "moduleId": "guid",
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1",
  "syllabusContent": "Raw syllabus content",
  "objectives": "By the end of the lesson...",
  "languageContent": "Language content",
  "vocabulary": "cat, dog, bird",
  "grammar": "This is a ...",
  "teachingMethodology": "Communicative approach",
  "teacherMaterials": "Flashcards",
  "studentMaterials": "Workbook",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null
}
```

Success data:

```json
{
  "id": "guid",
  "moduleId": "guid",
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1",
  "syllabusContent": "Raw syllabus content",
  "objectives": "By the end of the lesson...",
  "languageContent": "Language content",
  "vocabulary": "cat, dog, bird",
  "grammar": "This is a ...",
  "teachingMethodology": "Communicative approach",
  "teacherMaterials": "Flashcards",
  "studentMaterials": "Workbook",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null,
  "isActive": true,
  "createdAt": "2026-05-19T03:00:00Z",
  "updatedAt": "2026-05-19T03:00:00Z"
}
```

Common errors:

- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.SessionIndexRequired`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.DuplicateSessionIndex`

### 5.2 Get lesson plan template detail

- Method: `GET`
- URL: `/api/lesson-plan-templates/{id}`
- Role: `Admin`, `ManagementStaff`

Success data:

```json
{
  "id": "guid",
  "moduleId": "guid",
  "moduleCode": "UNIT_1",
  "moduleName": "Unit 1",
  "levelId": "guid",
  "levelName": "Starter",
  "programId": "guid",
  "programName": "Get Ready",
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1",
  "syllabusContent": "Raw syllabus content",
  "objectives": "By the end of the lesson...",
  "languageContent": "Language content",
  "vocabulary": "cat, dog, bird",
  "grammar": "This is a ...",
  "teachingMethodology": "Communicative approach",
  "teacherMaterials": "Flashcards",
  "studentMaterials": "Workbook",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null,
  "isActive": true,
  "createdBy": null,
  "createdByName": null,
  "createdAt": "2026-05-19T03:00:00Z",
  "updatedAt": "2026-05-19T03:00:00Z",
  "usedCount": 0
}
```

Common errors:

- `LessonPlanTemplate.NotFound`

### 5.3 List lesson plan templates

- Method: `GET`
- Role: `Admin`, `ManagementStaff`

Query params:

- `moduleId?: Guid`
- `title?: string`
- `isActive?: boolean`
- `includeDeleted?: boolean = false`
- `pageNumber?: int = 1`
- `pageSize?: int = 10`

Success data:

```json
{
  "templates": {
    "items": [
      {
        "id": "guid",
        "moduleId": "guid",
        "moduleCode": "UNIT_1",
        "moduleName": "Unit 1",
        "levelId": "guid",
        "levelName": "Starter",
        "programId": "guid",
        "programName": "Get Ready",
        "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
        "sessionIndex": 1,
        "sessionOrder": 1,
        "syllabusMetadata": "UNIT 1",
        "syllabusContent": "Raw syllabus content",
        "objectives": "By the end of the lesson...",
        "languageContent": "Language content",
        "vocabulary": "cat, dog, bird",
        "grammar": "This is a ...",
        "teachingMethodology": "Communicative approach",
        "teacherMaterials": "Flashcards",
        "studentMaterials": "Workbook",
        "procedure": "Warm-up, practice, production",
        "evaluation": "Observe and check answers",
        "sourceFileName": "Unit 1 lesson 1.docx",
        "attachment": null,
        "isActive": true,
        "createdBy": null,
        "createdByName": null,
        "createdAt": "2026-05-19T03:00:00Z",
        "updatedAt": "2026-05-19T03:00:00Z",
        "usedCount": 0
      }
    ],
    "pageNumber": 1,
    "totalPages": 1,
    "totalCount": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

### 5.4 Update lesson plan template

- Method: `PUT`
- URL: `/api/lesson-plan-templates/{id}`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `application/json`

Request body:

```json
{
  "moduleId": "guid",
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1",
  "syllabusContent": "Raw syllabus content",
  "objectives": "Updated objectives",
  "languageContent": "Updated language content",
  "vocabulary": "cat, dog, bird",
  "grammar": "This is a ...",
  "teachingMethodology": "Communicative approach",
  "teacherMaterials": "Flashcards",
  "studentMaterials": "Workbook",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null,
  "isActive": true
}
```

Success data:

```json
{
  "id": "guid",
  "moduleId": "guid",
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1",
  "syllabusContent": "Raw syllabus content",
  "objectives": "Updated objectives",
  "languageContent": "Updated language content",
  "vocabulary": "cat, dog, bird",
  "grammar": "This is a ...",
  "teachingMethodology": "Communicative approach",
  "teacherMaterials": "Flashcards",
  "studentMaterials": "Workbook",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null,
  "isActive": true,
  "updatedAt": "2026-05-19T03:10:00Z"
}
```

Common errors:

- `LessonPlanTemplate.NotFound`
- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.DuplicateSessionIndex`

### 5.5 Import lesson plan templates from table file

- Method: `POST`
- URL: `/api/lesson-plan-templates/import`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `multipart/form-data`

Query params:

- `moduleId?: Guid`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .csv | .xls | .xlsx`

Success data:

```json
{
  "importedCount": 24,
  "modules": [
    {
      "moduleId": "guid",
      "moduleName": "Unit 1",
      "importedSessions": 24
    }
  ]
}
```

Common errors:

- `LessonPlanTemplate.UnsupportedImportFileType`
- `LessonPlanTemplate.ImportFileRequiresModuleId`
- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.InvalidImportFile`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.ModuleMappingNotFound`
- inline `400` body `{ "error": "No file provided" }` neu khong gui file

### 5.6 Import one lesson plan Word file

- Method: `POST`
- URL: `/api/lesson-plan-templates/import-word`
- Role: `Admin`, `ManagementStaff`
- Content-Type: `multipart/form-data`

Query params:

- `moduleId: Guid`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .docx`

Success data:

```json
{
  "lessonPlanTemplateId": "guid",
  "sessionTemplateId": "guid",
  "sessionIndex": 1,
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1"
}
```

Common errors:

- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.DuplicateSessionIndex`
- `Syllabus.UnsupportedImportFileType`
- `Syllabus.InvalidImportFile`
- inline `400` body `{ "error": "No file provided" }` neu khong gui file

Notes:

- `sessionTemplateId` la `Guid?`, co the `null`.
- Importer parse tu headings trong Word nhu `Objectives`, `Language content`, `Vocabulary`, `Grammar`, `Procedure`, `Evaluation`, `Homework`.

## 6. LessonPlan APIs

Base path: `/api/lesson-plans`

### 6.1 Create lesson plan

- Method: `POST`
- Role: `Teacher`, `Admin`, `ManagementStaff`
- Content-Type: `application/json`

Request body:

```json
{
  "classId": "guid",
  "sessionId": "guid",
  "templateId": "guid",
  "plannedContent": "Planned content",
  "actualContent": "Actual content",
  "actualHomework": "Homework",
  "teacherNotes": "Teacher notes",
  "completionPercent": 80,
  "carryForwardContent": "Need continue next session"
}
```

Success data:

```json
{
  "id": "guid",
  "classId": "guid",
  "sessionId": "guid",
  "templateId": "guid",
  "plannedContent": "Planned content",
  "actualContent": "Actual content",
  "actualHomework": "Homework",
  "teacherNotes": "Teacher notes",
  "completionPercent": 80,
  "carryForwardContent": "Need continue next session",
  "submittedBy": null,
  "submittedAt": null,
  "createdAt": "2026-05-19T03:00:00Z"
}
```

### 6.2 Get lesson plan detail

- Method: `GET`
- URL: `/api/lesson-plans/{id}`
- Role: `Teacher`, `Admin`, `ManagementStaff`

Success data:

```json
{
  "id": "guid",
  "classId": "guid",
  "classCode": "CLS001",
  "sessionId": "guid",
  "sessionTitle": "Session 20/05/2026 19:00",
  "sessionDate": "2026-05-20T19:00:00",
  "templateId": "guid",
  "templateLevel": "Starter",
  "templateSessionIndex": 1,
  "plannedContent": "Planned content",
  "actualContent": "Actual content",
  "actualHomework": "Homework",
  "teacherNotes": "Teacher notes",
  "completionPercent": 80,
  "carryForwardContent": "Need continue next session",
  "submittedBy": "guid",
  "submittedByName": "Teacher A",
  "submittedAt": "2026-05-20T21:00:00",
  "createdAt": "2026-05-19T03:00:00Z"
}
```

### 6.3 Get class lesson plan syllabus

- Method: `GET`
- URL: `/api/lesson-plans/classes/{classId}/syllabus`
- Role: `Teacher`, `Admin`, `ManagementStaff`

Success data:

```json
{
  "classId": "guid",
  "classCode": "CLS001",
  "classTitle": "Get Ready Starter A",
  "programId": "guid",
  "programName": "Get Ready",
  "syllabusMetadata": "{...}",
  "sessions": [
    {
      "sessionId": "guid",
      "sessionIndex": 1,
      "moduleId": "guid",
      "sessionIndexInModule": 1,
      "sessionDate": "2026-05-20T19:00:00",
      "plannedTeacherId": "guid",
      "plannedTeacherName": "Teacher A",
      "actualTeacherId": "guid",
      "actualTeacherName": "Teacher A",
      "lessonPlanId": "guid",
      "templateId": "guid",
      "templateTitle": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
      "templateSyllabusContent": "Raw syllabus content",
      "plannedContent": "Planned content",
      "actualContent": "Actual content",
      "actualHomework": "Homework",
      "teacherNotes": "Teacher notes",
      "canEdit": true
    }
  ]
}
```

### 6.4 Update lesson plan

- Method: `PUT`
- URL: `/api/lesson-plans/{id}`
- Role: `Teacher`, `Admin`, `ManagementStaff`
- Content-Type: `application/json`

Request body:

```json
{
  "templateId": "guid",
  "plannedContent": "Updated planned content",
  "actualContent": "Updated actual content",
  "actualHomework": "Updated homework",
  "teacherNotes": "Updated notes",
  "completionPercent": 90,
  "carryForwardContent": "Need continue next session"
}
```

Success data:

```json
{
  "id": "guid",
  "sessionId": "guid",
  "templateId": "guid",
  "plannedContent": "Updated planned content",
  "actualContent": "Updated actual content",
  "actualHomework": "Updated homework",
  "teacherNotes": "Updated notes",
  "completionPercent": 90,
  "carryForwardContent": "Need continue next session"
}
```

## 7. Frontend Notes

- Tat ca endpoint import deu dung `multipart/form-data`.
- Key file luon la `file`.
- Khong manually set `Content-Type: multipart/form-data`; de browser tu gan `boundary`.
- `Syllabus` va `LessonPlanTemplate` chi cho `Admin`, `ManagementStaff`.
- `LessonPlan` cho `Teacher`, `Admin`, `ManagementStaff`.
- FE nen hien thi day du `title`, `detail`, `errors[]` tu response loi.
- Voi `import-archive`, FE nen hien thi `skippedEntries` rieng cho user.
