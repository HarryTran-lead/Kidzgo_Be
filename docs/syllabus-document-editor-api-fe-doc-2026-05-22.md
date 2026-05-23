# FE API Doc - Syllabus Manual, Import, Editor

> Updated: 2026-05-22  
> Scope: cac API moi cho flow `manual syllabus + import preview/commit + document editor`.

---

## 1. Tong Quan

BE hien ho tro 3 nhom nhu cau moi:

1. Tao syllabus thu cong theo document model
2. Import preview tu file Word truoc khi save
3. Chinh sua syllabus theo `sections[]`, `row`, `cell` tren document editor

Document model FE can render:

- Metadata o dau trang
- `sections[]` theo thu tu `orderIndex`
- `table.rows[].cells[]` da co `rowSpan` / `colSpan`
- Optimistic concurrency theo `version`

---

## 2. Auth Va Envelope

Base path:

- `/api/syllabuses`

Roles:

- `Admin`
- `ManagementStaff`

Success envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Error envelope:

```json
{
  "success": false,
  "isSuccess": false,
  "data": null,
  "message": "Version conflict. Please reload document. Expected version 9, current version 11.",
  "errors": [
    {
      "code": "Syllabus.VersionConflict",
      "description": "Version conflict. Please reload document. Expected version 9, current version 11."
    }
  ]
}
```

---

## 3. Document Model

### 3.1 SyllabusDocument

```json
{
  "id": "uuid",
  "programId": "uuid",
  "levelId": "uuid",
  "code": "STARTERS_V2",
  "title": "THE SYLLABUS OF GET READY FOR STARTERS",
  "edition": "Second edition",
  "status": "Draft",
  "sourceType": "Manual",
  "sourceFileName": null,
  "parserVersion": null,
  "version": 1,
  "summary": {
    "totalUnits": 15,
    "totalSessions": 50,
    "totalLessons": 50,
    "totalPeriods": 100,
    "minutesPerPeriod": 45
  },
  "sections": [],
  "warnings": []
}
```

`status`:

- `Draft`
- `Published`
- `Archived`

`sourceType`:

- `Manual`
- `Imported`
- `Hybrid`

### 3.2 Section Types

`sections[].type`:

- `heading`
- `narrative`
- `list`
- `table`

### 3.3 Table Layout

```json
{
  "sectionId": "uuid",
  "type": "table",
  "title": "Curriculum",
  "orderIndex": 5,
  "editable": true,
  "table": {
    "columns": [
      { "key": "periods", "label": "Periods", "width": 120, "sticky": false },
      { "key": "topics", "label": "Topics", "width": 240, "sticky": true }
    ],
    "rows": [
      {
        "rowId": "uuid",
        "orderIndex": 1,
        "group": {
          "blockLabel": "Starter",
          "topicGroupId": "topic-1",
          "topicRowSpan": 2
        },
        "cells": [
          { "columnKey": "periods", "value": "1-2", "rowSpan": 1, "colSpan": 1, "align": "center", "bold": true },
          { "columnKey": "topics", "value": "Starter: HELLO!", "rowSpan": 2, "colSpan": 1, "align": "left", "bold": true }
        ]
      }
    ]
  }
}
```

Important:

- FE render theo `sections[]`, khong tu suy ra thu tu
- FE render bang theo `columns[]` va `rows[].cells[]`
- `rowSpan` / `colSpan` da do BE tra ve
- Moi mutation editor phai gui `expectedVersion`

---

## 4. Flow FE Nen Dung

### 4.1 Tao syllabus thu cong

1. Goi `POST /api/syllabuses`
2. Lay `data.id`
3. Redirect vao editor
4. Goi `GET /api/syllabuses/{id}/document`
5. Dung cac API `metadata`, `sections`, `rows`, `cells` de edit tiep

### 4.2 Import preview

1. Upload file bang `POST /api/syllabuses/import-preview`
2. Render `data.document`
3. Hien `data.warnings`
4. Neu user dong y, moi goi `POST /api/syllabuses/import-commit`

### 4.3 Edit document

1. Luon giu `document.version` moi nhat
2. Moi lan save, gui `expectedVersion = document.version`
3. Neu BE tra `Syllabus.VersionConflict`, FE reload lai `GET /document`

---

## 5. Create Manual

### `POST /api/syllabuses`

Request:

```json
{
  "programId": "uuid",
  "levelId": "uuid",
  "code": "STARTERS_V2",
  "title": "The Syllabus Of Get Ready For Starters",
  "edition": "Second edition",
  "status": "Draft",
  "sourceType": "Manual",
  "minutesPerPeriod": 45
}
```

Response:

- `200/201`: `SyllabusDocument`

Notes:

- `status` FE nen gui `Draft`
- `sourceType` FE nen gui `Manual`
- `version` trong response la version optimistic concurrency cua editor, khong phai field version cu cua curriculum

Common errors:

- `404 Syllabus.LevelNotFound`
- `400 Syllabus.LevelDoesNotBelongToProgram`
- `409 Syllabus.DuplicateCode`

---

## 6. Import Preview

### `POST /api/syllabuses/import-preview`

Multipart fields:

- `programId: Guid`
- `levelId: Guid`
- `file: .docx`

Response:

```json
{
  "document": {
    "id": "00000000-0000-0000-0000-000000000000",
    "programId": "uuid",
    "levelId": "uuid",
    "code": "THE_SYLLABUS_FILE_NAME",
    "title": "THE SYLLABUS OF GET READY FOR STARTERS",
    "edition": "Second edition",
    "status": "Draft",
    "sourceType": "Imported",
    "sourceFileName": "The Syllabus of Get Ready for Starters.docx",
    "parserVersion": "docx-v1",
    "version": 1,
    "summary": {
      "totalUnits": 19,
      "totalSessions": 50,
      "totalLessons": 50,
      "totalPeriods": 100,
      "minutesPerPeriod": 45
    },
    "sections": [],
    "warnings": []
  },
  "warnings": [
    {
      "code": "MISSING_COLUMN",
      "severity": "Warning",
      "message": "Teachers book column missing in row 15",
      "sectionRef": "uuid",
      "rowRef": "uuid",
      "cellRef": "teachersBook"
    }
  ]
}
```

Notes:

- Preview hien tai parse `.docx`
- Preview chua save DB
- `document.id` trong preview co the la `Guid.Empty`

Common errors:

- `404 Syllabus.LevelNotFound`
- `400 Syllabus.LevelDoesNotBelongToProgram`
- `400 Syllabus.ImportParseFailed`

---

## 7. Import Commit

### `POST /api/syllabuses/import-commit`

Multipart fields:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `title: string?`
- `edition: string?`
- `file: .docx`
- `asDraft: boolean`, default `true`

Response:

- `200`: object co `document`

```json
{
  "document": {
    "id": "uuid",
    "programId": "uuid",
    "levelId": "uuid",
    "code": "STARTERS_V2",
    "title": "THE SYLLABUS OF GET READY FOR STARTERS",
    "edition": "Second edition",
    "status": "Draft",
    "sourceType": "Imported",
    "sourceFileName": "The Syllabus of Get Ready for Starters full (1).docx",
    "parserVersion": "docx-v1",
    "version": 1,
    "summary": {
      "totalUnits": 19,
      "totalSessions": 50,
      "totalLessons": 50,
      "totalPeriods": 100,
      "minutesPerPeriod": 45
    },
    "sections": [
      {
        "sectionId": "uuid",
        "type": "heading",
        "title": "THE SYLLABUS OF GET READY FOR STARTERS",
        "orderIndex": 1,
        "content": "Second edition"
      },
      {
        "sectionId": "uuid",
        "type": "table",
        "title": "Curriculum",
        "orderIndex": 4,
        "table": {
          "columns": [
            { "key": "periods", "label": "Periods", "width": 120, "sticky": false },
            { "key": "topics", "label": "Topics", "width": 240, "sticky": true },
            { "key": "lessons", "label": "Lessons", "width": 90, "sticky": false },
            { "key": "contents", "label": "Contents", "width": 360, "sticky": false },
            { "key": "structures", "label": "Structures", "width": 260, "sticky": false },
            { "key": "studentsBook", "label": "Students book", "width": 140, "sticky": false },
            { "key": "teachersBook", "label": "Teacher's book", "width": 140, "sticky": false }
          ],
          "rows": [
            {
              "rowId": "uuid",
              "orderIndex": 1,
              "group": {
                "blockLabel": "Starter",
                "topicGroupId": "topic-1",
                "topicRowSpan": 2
              },
              "cells": [
                { "columnKey": "periods", "value": "1-2", "rowSpan": 1, "colSpan": 1, "align": "center", "bold": true },
                { "columnKey": "topics", "value": "Starter: HELLO!", "rowSpan": 2, "colSpan": 1, "align": "left", "bold": true },
                { "columnKey": "lessons", "value": "1", "rowSpan": 1, "colSpan": 1, "align": "center", "bold": false },
                { "columnKey": "contents", "value": "...", "rowSpan": 1, "colSpan": 1, "align": "left", "bold": false },
                { "columnKey": "structures", "value": "...", "rowSpan": 1, "colSpan": 1, "align": "left", "bold": false },
                { "columnKey": "studentsBook", "value": "p.4", "rowSpan": 1, "colSpan": 1, "align": "center", "bold": false },
                { "columnKey": "teachersBook", "value": "p.8", "rowSpan": 1, "colSpan": 1, "align": "center", "bold": false }
              ]
            }
          ]
        }
      }
    ],
    "warnings": []
  }
}
```

Notes:

- `asDraft = true`: document save o trang thai `Draft`
- `asDraft = false`: document save o trang thai `Published`
- Import commit se save syllabus + document model
- FE nen dung `data.document` ngay sau import thay vi tu rebuild tu summary cu
- `document.sections[]` phai la full parse payload, khong phai summary rut gon

Common errors:

- `409 Syllabus.DuplicateCode`
- `400 Syllabus.ImportParseFailed`
- `404 Syllabus.LevelNotFound`

---

## 8. Get Document Detail

### `GET /api/syllabuses/{id}/document`

Response:

- `200`: `SyllabusDocument`

FE nen dung endpoint nay lam source chinh cho editor.

Common errors:

- `404 Syllabus.NotFound`

---

## 9. Get Syllabus Detail

### `GET /api/syllabuses/{id}`

Endpoint nay van huu ich cho trang detail/overview sau import.

Response ngoai cac field legacy se co them:

- `summary`
- `document`
- `units`
- `lessons`
- `resources`
- `sessionTemplates`

```json
{
  "id": "uuid",
  "programId": "uuid",
  "programName": "Kids English",
  "levelId": "uuid",
  "levelName": "Starters",
  "code": "STARTERS_V2",
  "version": "doc-20260524093000123",
  "title": "THE SYLLABUS OF GET READY FOR STARTERS",
  "edition": "Second edition",
  "totalPeriods": 100,
  "minutesPerPeriod": 45,
  "totalLessons": 50,
  "sourceFileName": "The Syllabus of Get Ready for Starters full (1).docx",
  "rawContentJson": "{...full parsed object...}",
  "summary": {
    "totalUnits": 19,
    "totalSessions": 50,
    "totalLessons": 50,
    "totalPeriods": 100,
    "minutesPerPeriod": 45
  },
  "document": {
    "id": "uuid",
    "programId": "uuid",
    "levelId": "uuid",
    "code": "STARTERS_V2",
    "title": "THE SYLLABUS OF GET READY FOR STARTERS",
    "edition": "Second edition",
    "status": "Draft",
    "sourceType": "Imported",
    "sourceFileName": "The Syllabus of Get Ready for Starters full (1).docx",
    "parserVersion": "docx-v1",
    "version": 1,
    "summary": {
      "totalUnits": 19,
      "totalSessions": 50,
      "totalLessons": 50,
      "totalPeriods": 100,
      "minutesPerPeriod": 45
    },
    "sections": [],
    "warnings": []
  },
  "lessons": [
    {
      "id": "uuid",
      "moduleId": "uuid-or-null",
      "moduleName": "Starter",
      "periodFrom": 1,
      "periodTo": 2,
      "topic": "Starter: HELLO!",
      "lessonNumber": 1,
      "contentSummary": "...",
      "structureSummary": "...",
      "studentBookPages": "p.4",
      "teacherBookPages": "p.8",
      "orderIndex": 1
    }
  ],
  "sessionTemplates": [
    {
      "id": "uuid",
      "lessonPlanTemplateId": "uuid-or-null",
      "lessonPlanTemplateTitle": "Starter - Lesson 1",
      "sessionIndex": 1,
      "sessionIndexInModule": 1,
      "lessonNumber": 1,
      "title": "Starter - Lesson 1",
      "topic": "Starter: HELLO!",
      "orderIndex": 1
    }
  ]
}
```

FE note:

- Dung `data.document.sections` de render full syllabus document
- Dung `data.summary` cho header counters
- Dung `data.lessons` va `data.sessionTemplates` cho side panel/list neu can
- `data.document.summary`, `data.summary`, `GET /document.summary` va list counters phai khop nhau cho cung `syllabusId`

---

## 10. Update Metadata

### `PATCH /api/syllabuses/{id}/metadata`

Request:

```json
{
  "expectedVersion": 7,
  "code": "STARTERS_V2",
  "title": "The Syllabus Of Get Ready For Starters",
  "edition": "Second edition",
  "minutesPerPeriod": 45
}
```

Response:

- `200`: `SyllabusDocument`

Common errors:

- `404 Syllabus.NotFound`
- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `409 Syllabus.DuplicateCode`

---

## 11. Section APIs

### `POST /api/syllabuses/{id}/sections`

Request:

```json
{
  "expectedVersion": 7,
  "section": {
    "type": "narrative",
    "title": "Specific objectives",
    "orderIndex": 3,
    "content": "..."
  }
}
```

Response:

- `200`: `SyllabusDocument`

### `PATCH /api/syllabuses/{id}/sections/{sectionId}`

Request:

```json
{
  "expectedVersion": 8,
  "title": "Overview",
  "content": "..."
}
```

Response:

- `200`: `SyllabusDocument`

### `PATCH /api/syllabuses/{id}/sections/reorder`

Request:

```json
{
  "expectedVersion": 10,
  "orders": [
    { "sectionId": "uuid-1", "orderIndex": 1 },
    { "sectionId": "uuid-2", "orderIndex": 2 }
  ]
}
```

Response:

- `200`: `SyllabusDocument`

Common errors:

- `404 Syllabus.NotFound`
- `404 Syllabus.SectionNotFound`
- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `400 Syllabus.InvalidTableLayout`

---

## 12. Table APIs

### `PATCH /api/syllabuses/{id}/sections/{sectionId}/rows/{rowId}/cells/{columnKey}`

Request:

```json
{
  "expectedVersion": 9,
  "value": "Starter: HELLO!",
  "rowSpan": 2,
  "colSpan": 1,
  "align": "left",
  "bold": true
}
```

Response:

- `200`: `SyllabusDocument`

### `POST /api/syllabuses/{id}/sections/{sectionId}/rows`

Request:

```json
{
  "expectedVersion": 9,
  "orderIndex": 22,
  "cells": [
    { "columnKey": "periods", "value": "21-22" },
    { "columnKey": "topics", "value": "Unit 4: FOOD" },
    { "columnKey": "lessons", "value": "3" }
  ]
}
```

Response:

- `200`: `SyllabusDocument`

### `DELETE /api/syllabuses/{id}/sections/{sectionId}/rows/{rowId}?expectedVersion=10`

Response:

- `200`: `SyllabusDocument`

Common errors:

- `404 Syllabus.NotFound`
- `404 Syllabus.SectionNotFound`
- `404 Syllabus.RowNotFound`
- `404 Syllabus.CellNotFound`
- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `400 Syllabus.InvalidTableLayout`

---

## 13. Publish Va Archive

### `POST /api/syllabuses/{id}/publish`

Request:

```json
{
  "expectedVersion": 11
}
```

Response:

- `200`: `SyllabusDocument`

Notes:

- Sau khi publish, document se o `status = Published`
- Ban published khong duoc edit truc tiep nua

Common errors:

- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `400 Syllabus.PublishValidationFailed`

### `POST /api/syllabuses/{id}/archive`

Request:

```json
{
  "expectedVersion": 12,
  "reason": "Replaced by v3"
}
```

Response:

- `200`: `SyllabusDocument`

Notes:

- Sau khi archive, document se o `status = Archived`
- FE nen coi ban nay la read-only

---

## 14. Warning Codes

FE nen support it nhat cac warning sau:

- `LOW_CONFIDENCE_TOPIC`
- `MISSING_COLUMN`
- `MIXED_TABLE_LAYOUT`
- `UNSUPPORTED_MERGED_CELL`
- `UNREADABLE_TEXT`

Model:

```json
{
  "code": "MISSING_COLUMN",
  "severity": "Warning",
  "message": "Teachers book column missing in row 15",
  "sectionRef": "uuid",
  "rowRef": "uuid",
  "cellRef": "teachersBook"
}
```

---

## 15. Luu Y Cho FE

1. Dung `GET /api/syllabuses/{id}/document` lam endpoint chinh cho editor, khong dung `GET /api/syllabuses/{id}` cho UI editor moi.
2. Moi mutation thanh cong deu nen overwrite local state bang `data` moi nhat tu response.
3. Khi gap `Syllabus.VersionConflict`, FE nen reload document thay vi merge local.
4. Khi gap `Syllabus.PublishedReadOnly`, FE nen khoa editor va hien thong bao read-only.
5. FE khong nen tu tinh `rowSpan` / `colSpan`; render dung gia tri BE tra ve.
6. Sau import DOCX, `POST /import-commit`, `GET /{id}` va `GET /{id}/document` deu phai tro ve cung full document, khong chi summary rut gon.

---

## 16. API Cu Van Con

Nhom API cu van con dung cho flow curriculum/import hien tai:

- `GET /api/syllabuses`
- `GET /api/syllabuses/{id}`
- `PUT /api/syllabuses/{id}`
- `GET /api/syllabuses/import-configuration`
- `PUT /api/syllabuses/import-configuration`
- `POST /api/syllabuses/import-word`
- `POST /api/syllabuses/import-archive`
- `POST /api/syllabuses/import-lesson-plan-words`
- `GET /api/syllabuses/{id}/unit-lesson-plans`

Cho document editor moi, FE uu tien dung nhom API moi trong tai lieu nay.
