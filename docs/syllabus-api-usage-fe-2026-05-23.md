# Syllabus API Usage For FE

Updated: 2026-05-23
Scope: manual create + import preview/commit + document editor + archive audit
Base path: `/api/syllabuses`
Roles: `Admin`, `ManagementStaff`

## 1. Muc tieu FE

FE co the dung bo API nay cho 4 flow:

1. Tao syllabus thu cong
2. Import preview tu `docx` hoac `pdf`
3. Commit import thanh syllabus draft/published
4. Edit document theo `sections[]`, `rows`, `cells`

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
  "message": "Version conflict. Please reload document. Expected version 9, current version 11.",
  "errors": [
    {
      "code": "Syllabus.VersionConflict",
      "description": "Version conflict. Please reload document. Expected version 9, current version 11."
    }
  ]
}
```

## 3. SyllabusDocument

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

`sections[].type`:

- `heading`
- `narrative`
- `list`
- `table`

## 4. FE workflow

### 4.1 Manual create

1. `POST /api/syllabuses`
2. Lay `data.id`
3. `GET /api/syllabuses/{id}/document`
4. Edit bang cac API `metadata`, `sections`, `rows`, `cells`

### 4.2 Import preview + commit

1. Upload `docx` hoac `pdf` bang `POST /api/syllabuses/import-preview`
2. Render `data.document`
3. Hien `data.warnings`
4. Neu user dong y, goi `POST /api/syllabuses/import-commit`

### 4.3 Document editor

1. Luon luu `document.version` moi nhat
2. Moi mutation gui `expectedVersion`
3. Neu BE tra `Syllabus.VersionConflict`, FE goi lai `GET /document`

## 5. Create manual syllabus

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

Notes:

- FE nen gui `status = Draft`
- FE nen gui `sourceType = Manual`
- Response tra ve `SyllabusDocument`

Common errors:

- `404 Syllabus.LevelNotFound`
- `400 Syllabus.LevelDoesNotBelongToProgram`
- `409 Syllabus.DuplicateCode`

## 6. Import preview

### `POST /api/syllabuses/import-preview`

Multipart:

- `programId: Guid`
- `levelId: Guid`
- `file: .docx | .pdf`

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
    "sourceFileName": "The Syllabus of Get Ready for Starters.pdf",
    "parserVersion": "pdf-v1",
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

- Preview khong save DB
- `document.id` preview co the la `Guid.Empty`
- `parserVersion` hien tai co the la `docx-v1` hoac `pdf-v1`
- PDF parse la heuristic text parse. Neu PDF khong co selectable text/table text qua vo, BE co the tra `Syllabus.ImportParseFailed`

Common errors:

- `404 Syllabus.LevelNotFound`
- `400 Syllabus.LevelDoesNotBelongToProgram`
- `400 Syllabus.ImportParseFailed`

## 7. Import commit

### `POST /api/syllabuses/import-commit`

Multipart:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `title: string?`
- `edition: string?`
- `file: .docx | .pdf`
- `asDraft: boolean`, default `true`

Notes:

- `asDraft = true` -> save `Draft`
- `asDraft = false` -> save `Published`
- Response tra ve `SyllabusDocument`

Common errors:

- `409 Syllabus.DuplicateCode`
- `400 Syllabus.ImportParseFailed`
- `404 Syllabus.LevelNotFound`

## 8. Get document detail

### `GET /api/syllabuses/{id}/document`

Dung endpoint nay lam nguon du lieu chinh cho editor.

Common errors:

- `404 Syllabus.NotFound`

## 9. Update metadata

### `PATCH /api/syllabuses/{id}/metadata`

Request:

```json
{
  "expectedVersion": 7,
  "title": "The Syllabus Of Get Ready For Starters",
  "edition": "Second edition",
  "minutesPerPeriod": 45
}
```

Hoac neu FE muon doi code:

```json
{
  "expectedVersion": 7,
  "code": "STARTERS_V3",
  "title": "The Syllabus Of Get Ready For Starters",
  "edition": "Second edition",
  "minutesPerPeriod": 45
}
```

Notes:

- `code` hien tai la optional. Neu khong gui, backend giu nguyen `code` cu

Common errors:

- `404 Syllabus.NotFound`
- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `409 Syllabus.DuplicateCode`

## 10. Section APIs

### `POST /api/syllabuses/{id}/sections`

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

### `PATCH /api/syllabuses/{id}/sections/{sectionId}`

```json
{
  "expectedVersion": 8,
  "title": "Overview",
  "content": "..."
}
```

### `PATCH /api/syllabuses/{id}/sections/reorder`

```json
{
  "expectedVersion": 10,
  "orders": [
    { "sectionId": "uuid-1", "orderIndex": 1 },
    { "sectionId": "uuid-2", "orderIndex": 2 }
  ]
}
```

Common errors:

- `404 Syllabus.NotFound`
- `404 Syllabus.SectionNotFound`
- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `400 Syllabus.InvalidTableLayout`

## 11. Table editor APIs

### `PATCH /api/syllabuses/{id}/sections/{sectionId}/rows/{rowId}/cells/{columnKey}`

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

### `POST /api/syllabuses/{id}/sections/{sectionId}/rows`

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

### `DELETE /api/syllabuses/{id}/sections/{sectionId}/rows/{rowId}?expectedVersion=10`

Notes:

- FE khong can gui full document
- Chi patch granular phan dang edit
- `rowSpan` va `colSpan` phai > 0

Common errors:

- `404 Syllabus.NotFound`
- `404 Syllabus.SectionNotFound`
- `404 Syllabus.RowNotFound`
- `404 Syllabus.CellNotFound`
- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `400 Syllabus.InvalidTableLayout`

## 12. Publish va archive

### `POST /api/syllabuses/{id}/publish`

```json
{
  "expectedVersion": 11
}
```

### `POST /api/syllabuses/{id}/archive`

```json
{
  "expectedVersion": 12,
  "reason": "Replaced by v3"
}
```

Notes:

- Sau khi publish, document chuyen sang `Published`
- Ban `Published` khong duoc edit truc tiep nua
- Truoc khi publish, phai co it nhat 1 table hop le

Common errors:

- `409 Syllabus.VersionConflict`
- `409 Syllabus.PublishedReadOnly`
- `400 Syllabus.PublishValidationFailed`

## 13. Import full archive

### `POST /api/syllabuses/import-archive`

Query:

- `programId`
- `levelId`
- `code`
- `version`
- `overwriteExisting=true|false`

Multipart:

- `file: .zip`

Muc dich:

- Import 1 zip co `PPCT ...`, `UNIT ...`, `REVISION`
- Syllabus file trong zip hien tai van la `docx`
- Lesson plan trong zip hien tai van la `docx`

Response shape FE nen dung:

```json
{
  "syllabusId": "uuid",
  "importedLessonPlans": 50,
  "skippedFiles": 2,
  "importedEntries": [
    {
      "entryName": "LESSON PLAN GET READY STARTER 2ED/UNIT 1/Unit 1 I love animals lesson 1 done.docx",
      "sourceFolder": "UNIT 1",
      "sourceType": "UnitLesson",
      "moduleId": "uuid",
      "moduleName": "Unit 1",
      "lessonPlanTemplateId": "uuid",
      "sessionTemplateId": "uuid",
      "sessionIndex": 1,
      "sessionOrder": 1,
      "created": true,
      "title": "Unit 1 - Lesson 1"
    },
    {
      "entryName": "LESSON PLAN GET READY STARTER 2ED/PPCT/The Syllabus of Get Ready for Starters full.docx",
      "sourceFolder": "PPCT",
      "sourceType": "SyllabusDocument",
      "moduleId": null,
      "moduleName": null,
      "lessonPlanTemplateId": null,
      "sessionTemplateId": null,
      "sessionIndex": null,
      "sessionOrder": null,
      "created": true,
      "title": "The Syllabus of Get Ready for Starters full"
    }
  ],
  "skippedEntries": [
    "LESSON PLAN GET READY STARTER 2ED/REVISION/Revision 99.docx: Could not resolve session index from import configuration"
  ],
  "skippedItems": [
    {
      "entryName": "LESSON PLAN GET READY STARTER 2ED/REVISION/Revision 99.docx",
      "sourceFolder": "REVISION",
      "sourceType": "RevisionLesson",
      "reason": "Could not resolve session index from import configuration"
    }
  ]
}
```

Notes:

- `skippedEntries` la list string de FE toast/log nhanh
- `skippedItems` la list structured de FE render audit table
- `sourceType` co the la:
  - `SyllabusDocument`
  - `UnitLesson`
  - `RevisionLesson`

## 14. Warning model

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

Codes thuong gap:

- `LOW_CONFIDENCE_TOPIC`
- `MISSING_COLUMN`
- `MIXED_TABLE_LAYOUT`
- `UNSUPPORTED_MERGED_CELL`
- `UNREADABLE_TEXT`

## 15. FE implementation notes

1. Render document theo `sections[]` va `orderIndex`, khong suy dien them.
2. Table render theo `columns[]` va `rows[].cells[]`; backend da tra `rowSpan`/`colSpan`.
3. Khi gap `Syllabus.PublishedReadOnly`, khoa editor va chuyen document sang read-only mode.
4. Khi gap `Syllabus.VersionConflict`, reload `GET /document` roi merge lai UI state neu can.
5. Voi import PDF, nen cho phep user fallback sang file Word neu parse that bai.
