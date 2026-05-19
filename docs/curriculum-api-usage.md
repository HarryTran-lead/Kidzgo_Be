# Curriculum API Usage

## Scope

File nay hien bao gom toan bo nhom API da thay doi trong phase curriculum o thread nay:

- `Syllabus` CRUD
- `Syllabus` import Word
- `Syllabus` import zip archive
- `LessonPlanTemplate` CRUD
- `LessonPlanTemplate` list/detail
- `LessonPlanTemplate` import bang cu `csv/xls/xlsx`
- `LessonPlanTemplate` import Word don le theo lesson

File nay chua mo ta cac nhom API khac ngoai scope curriculum hien tai nhu:

- `TuitionPlan`
- `Class`
- `Module`
- `TeachingLog`
- `Session`

## Overview

Flow hien tai tach 3 phan:

- `Syllabus`: curriculum structure cua `Program + Level + Version`
- `LessonPlanTemplate`: lesson plan chuan theo tung lesson trong module
- `Import`: import file Word don le hoac zip tong

Tat ca API duoi day yeu cau `Authorization: Bearer <token>`.

Role duoc phep:

- `ManagementStaff`
- `Admin`

Response success chuan:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Response loi business chuan:

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

## Syllabus APIs

### 1. Create Syllabus

- Method: `POST`
- URL: `/api/syllabuses`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `application/json`

Request body:

```json
{
  "programId": "11111111-1111-1111-1111-111111111111",
  "levelId": "22222222-2222-2222-2222-222222222222",
  "code": "GET_READY_STARTER",
  "version": "v1",
  "title": "The Syllabus of Get Ready for Starters",
  "edition": "Second edition",
  "effectiveFrom": "2026-05-19T00:00:00+07:00",
  "effectiveTo": null,
  "pacingSchemeJson": null,
  "overview": "Curriculum overview...",
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

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "programId": "11111111-1111-1111-1111-111111111111",
    "levelId": "22222222-2222-2222-2222-222222222222",
    "code": "GET_READY_STARTER",
    "version": "v1",
    "title": "The Syllabus of Get Ready for Starters",
    "isActive": true
  }
}
```

Common errors:

- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.DuplicateVersion`

### 2. List Syllabuses

- Method: `GET`
- URL: `/api/syllabuses`
- Role: `ManagementStaff`, `Admin`

Query params:

- `programId?: Guid`
- `levelId?: Guid`
- `searchTerm?: string`
- `isActive?: boolean`
- `includeDeleted?: boolean`
- `pageNumber?: int`
- `pageSize?: int`

Example:

```text
GET /api/syllabuses?programId=11111111-1111-1111-1111-111111111111&levelId=22222222-2222-2222-2222-222222222222&pageNumber=1&pageSize=10
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabuses": {
      "items": [
        {
          "id": "33333333-3333-3333-3333-333333333333",
          "programId": "11111111-1111-1111-1111-111111111111",
          "programName": "Get Ready",
          "levelId": "22222222-2222-2222-2222-222222222222",
          "levelName": "Starter",
          "code": "GET_READY_STARTER",
          "version": "v1",
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
}
```

### 3. Get Syllabus Detail

- Method: `GET`
- URL: `/api/syllabuses/{id}`
- Role: `ManagementStaff`, `Admin`

Success response shape:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "programId": "11111111-1111-1111-1111-111111111111",
    "programName": "Get Ready",
    "levelId": "22222222-2222-2222-2222-222222222222",
    "levelName": "Starter",
    "code": "GET_READY_STARTER",
    "version": "v1",
    "title": "The Syllabus of Get Ready for Starters",
    "edition": "Second edition",
    "effectiveFrom": "2026-05-19T00:00:00+07:00",
    "effectiveTo": null,
    "pacingSchemeJson": null,
    "overview": "Curriculum overview...",
    "overallObjectives": null,
    "specificObjectives": null,
    "ethicsAndAttitudes": null,
    "bookOverview": null,
    "totalPeriods": 72,
    "minutesPerPeriod": 90,
    "totalLessons": 72,
    "sourceFileName": "The Syllabus of Get Ready for Starters full (1).docx",
    "attachmentUrl": null,
    "rawContentJson": "{...}",
    "isActive": true,
    "units": [],
    "lessons": [],
    "resources": [],
    "sessionTemplates": []
  }
}
```

Common errors:

- `Syllabus.NotFound`

### 4. Update Syllabus

- Method: `PUT`
- URL: `/api/syllabuses/{id}`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `application/json`

Request body: giong create, tru `programId` va `levelId`.

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "code": "GET_READY_STARTER",
    "version": "v2",
    "title": "The Syllabus of Get Ready for Starters",
    "isActive": true
  }
}
```

Common errors:

- `Syllabus.NotFound`
- `Syllabus.DuplicateVersion`

### 5. Import One Syllabus Word File

- Method: `POST`
- URL: `/api/syllabuses/import-word`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `multipart/form-data`

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: string`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .docx`

Expected file:

- file syllabus tu folder `PPCT ...`

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "33333333-3333-3333-3333-333333333333",
    "importedUnits": 15,
    "importedLessons": 72,
    "importedResources": 4,
    "importedSessionTemplates": 72
  }
}
```

Common errors:

- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.DuplicateVersion`
- `Syllabus.UnsupportedImportFileType`
- `Syllabus.InvalidImportFile`

### 6. Import One Curriculum Zip Archive

- Method: `POST`
- URL: `/api/syllabuses/import-archive`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `multipart/form-data`

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: string`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .zip`

Expected zip structure:

- root zip contains folder `PPCT ...`
- root zip contains folders `UNIT 1`, `UNIT 2`, ...
- optional `REVISION`

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "33333333-3333-3333-3333-333333333333",
    "importedLessonPlans": 45,
    "skippedFiles": 2,
    "skippedEntries": [
      "UNIT 8/lesson draft.docx",
      "REVISION/Revision 03.docx: SessionIndex 25 must be between 1 and 24"
    ]
  }
}
```

Notes:

- importer se co import file syllabus trong folder `PPCT`
- importer se co map tung file lesson docx sang `Module` theo ten folder `UNIT n` hoac `REVISION`
- file khong map duoc module se vao `skippedEntries`

Common errors:

- `Syllabus.UnsupportedImportFileType`
- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.InvalidImportFile`

## LessonPlanTemplate APIs

### 7. Create LessonPlanTemplate

- Method: `POST`
- URL: `/api/lesson-plan-templates`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `application/json`

Request body:

```json
{
  "moduleId": "66666666-6666-6666-6666-666666666666",
  "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1",
  "syllabusContent": "Raw syllabus content",
  "objectives": "By the end of the lesson, students will be able to...",
  "languageContent": "Language content text",
  "vocabulary": "cat, dog, bird",
  "grammar": "This is a ...",
  "teachingMethodology": "Communicative approach",
  "teacherMaterials": "Flashcards, projector",
  "studentMaterials": "Workbook, pencil",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "44444444-4444-4444-4444-444444444444",
    "moduleId": "66666666-6666-6666-6666-666666666666",
    "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
    "sessionIndex": 1,
    "sessionOrder": 1,
    "syllabusMetadata": "UNIT 1",
    "syllabusContent": "Raw syllabus content",
    "objectives": "By the end of the lesson, students will be able to...",
    "languageContent": "Language content text",
    "vocabulary": "cat, dog, bird",
    "grammar": "This is a ...",
    "teachingMethodology": "Communicative approach",
    "teacherMaterials": "Flashcards, projector",
    "studentMaterials": "Workbook, pencil",
    "procedure": "Warm-up, practice, production",
    "evaluation": "Observe and check answers",
    "sourceFileName": "Unit 1 lesson 1.docx",
    "attachment": null,
    "isActive": true,
    "createdAt": "2026-05-19T03:00:00Z",
    "updatedAt": "2026-05-19T03:00:00Z"
  }
}
```

Common errors:

- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.SessionIndexRequired`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.DuplicateSessionIndex`

### 8. List LessonPlanTemplates

- Method: `GET`
- URL: `/api/lesson-plan-templates`
- Role: `ManagementStaff`, `Admin`

Query params:

- `moduleId?: Guid`
- `title?: string`
- `isActive?: boolean`
- `includeDeleted?: boolean`
- `pageNumber?: int`
- `pageSize?: int`

Example:

```text
GET /api/lesson-plan-templates?moduleId=66666666-6666-6666-6666-666666666666&pageNumber=1&pageSize=10
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "templates": {
      "items": [
        {
          "id": "44444444-4444-4444-4444-444444444444",
          "moduleId": "66666666-6666-6666-6666-666666666666",
          "moduleCode": "UNIT_1",
          "moduleName": "Unit 1",
          "levelId": "22222222-2222-2222-2222-222222222222",
          "levelName": "Starter",
          "programId": "11111111-1111-1111-1111-111111111111",
          "programName": "Get Ready",
          "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
          "sessionIndex": 1,
          "sessionOrder": 1,
          "syllabusMetadata": "UNIT 1",
          "syllabusContent": "Raw syllabus content",
          "objectives": "By the end of the lesson, students will be able to...",
          "languageContent": "Language content text",
          "vocabulary": "cat, dog, bird",
          "grammar": "This is a ...",
          "teachingMethodology": "Communicative approach",
          "teacherMaterials": "Flashcards, projector",
          "studentMaterials": "Workbook, pencil",
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
}
```

### 9. Get LessonPlanTemplate Detail

- Method: `GET`
- URL: `/api/lesson-plan-templates/{id}`
- Role: `ManagementStaff`, `Admin`

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "44444444-4444-4444-4444-444444444444",
    "moduleId": "66666666-6666-6666-6666-666666666666",
    "moduleCode": "UNIT_1",
    "moduleName": "Unit 1",
    "levelId": "22222222-2222-2222-2222-222222222222",
    "levelName": "Starter",
    "programId": "11111111-1111-1111-1111-111111111111",
    "programName": "Get Ready",
    "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1",
    "sessionIndex": 1,
    "sessionOrder": 1,
    "syllabusMetadata": "UNIT 1",
    "syllabusContent": "Raw syllabus content",
    "objectives": "By the end of the lesson, students will be able to...",
    "languageContent": "Language content text",
    "vocabulary": "cat, dog, bird",
    "grammar": "This is a ...",
    "teachingMethodology": "Communicative approach",
    "teacherMaterials": "Flashcards, projector",
    "studentMaterials": "Workbook, pencil",
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
}
```

Common errors:

- `LessonPlanTemplate.NotFound`

### 10. Update LessonPlanTemplate

- Method: `PUT`
- URL: `/api/lesson-plan-templates/{id}`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `application/json`

Request body:

```json
{
  "moduleId": "66666666-6666-6666-6666-666666666666",
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
  "teacherMaterials": "Flashcards, projector",
  "studentMaterials": "Workbook, pencil",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observe and check answers",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": null,
  "isActive": true
}
```

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "44444444-4444-4444-4444-444444444444",
    "moduleId": "66666666-6666-6666-6666-666666666666",
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
    "teacherMaterials": "Flashcards, projector",
    "studentMaterials": "Workbook, pencil",
    "procedure": "Warm-up, practice, production",
    "evaluation": "Observe and check answers",
    "sourceFileName": "Unit 1 lesson 1.docx",
    "attachment": null,
    "isActive": true,
    "updatedAt": "2026-05-19T03:10:00Z"
  }
}
```

Common errors:

- `LessonPlanTemplate.NotFound`
- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.DuplicateSessionIndex`

### 11. Import LessonPlanTemplates From Table File

- Method: `POST`
- URL: `/api/lesson-plan-templates/import`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `multipart/form-data`

Query params:

- `moduleId: Guid`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .csv | .xls | .xlsx`

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 24,
    "modules": [
      {
        "moduleId": "66666666-6666-6666-6666-666666666666",
        "moduleName": "Unit 1",
        "importedSessions": 24
      }
    ]
  }
}
```

Common errors:

- `LessonPlanTemplate.UnsupportedImportFileType`
- `LessonPlanTemplate.ImportFileRequiresModuleId`
- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.InvalidImportFile`
- `LessonPlanTemplate.SessionIndexOutOfRange`

### 12. Import One Lesson Plan Word File

- Method: `POST`
- URL: `/api/lesson-plan-templates/import-word`
- Role: `ManagementStaff`, `Admin`
- Content-Type: `multipart/form-data`

Query params:

- `moduleId: Guid`
- `overwriteExisting?: boolean = true`

Form-data:

- `file: .docx`

Expected file:

- mot lesson docx tu folder `UNIT n` hoac `REVISION`

Importer currently maps:

- `Objectives`
- `Language content`
- `Vocabulary`
- `Grammar`
- `Teaching methodology`
- `Materials for teacher`
- `Materials for students`
- `Procedure`
- `Evaluation`
- `Homework`

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "lessonPlanTemplateId": "44444444-4444-4444-4444-444444444444",
    "sessionTemplateId": "55555555-5555-5555-5555-555555555555",
    "sessionIndex": 1,
    "title": "UNIT 1: I LOVE ANIMALS! - Lesson 1"
  }
}
```

Common errors:

- `LessonPlanTemplate.ModuleNotFound`
- `LessonPlanTemplate.SessionIndexOutOfRange`
- `LessonPlanTemplate.DuplicateSessionIndex`
- `Syllabus.UnsupportedImportFileType`
- `Syllabus.InvalidImportFile`

## Frontend Notes

- Cac endpoint import deu dung `multipart/form-data`, key file luon la `file`
- Query params `programId`, `levelId`, `moduleId` la bat buoc o import endpoints tuong ung
- FE nen hien thi `title`, `detail`, va mang `errors` tu response loi
- Voi import archive, FE nen hien thi rieng `skippedEntries` de user biet file nao khong import duoc
- File nay hien da bao phu toan bo API changed cua scope curriculum/lesson-plan trong phase nay
