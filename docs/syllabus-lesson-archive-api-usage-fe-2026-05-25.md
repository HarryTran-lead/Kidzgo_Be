# FE API Usage - Syllabus Archive And Lesson JSON

Updated: 2026-05-25  
Scope: cac API FE can biet sau khi BE doi sang flow `ZIP upload -> BE parse -> DB JSON -> FE render`.

Base paths:

- `/api/syllabuses`
- `/api/lessons`

Roles:

- Import/archive APIs: `Admin`, `ManagementStaff`
- Lesson read API: `Teacher`, `Admin`, `ManagementStaff`

---

## 1. Huong dung FE

FE khong parse `DOCX` hay `XLSX` truc tiep.

Flow chuan:

1. Admin upload `ZIP`
2. BE extract va scan folder
3. BE bo qua file tam bat dau bang `~$`
4. BE parse:
   - syllabus master tu `xlsx/xls/docx`
   - lesson plan tu `docx`
5. BE save DB + file URL
6. FE chi render JSON tu API

---

## 2. Archive Structure Supported

Archive hien duoc support o dang `ZIP`.

Vi du:

```text
PPCT GET STARTER/
  get_ready_for_starters_import_ready.xlsx

UNIT STARTER/
  lesson starter ....docx

UNIT 1/
  ...lesson 1.docx
  ...lesson 2.docx
  ...lesson 3.docx

...

UNIT 15/
  ...

REVISION/
  Revision 01.docx
  Revision 02.docx
  Revision 03.docx
```

Rule quan trong:

- File bat dau bang `~$` se bi ignore
- Syllabus trong folder `PPCT...` co the la `.xlsx`, `.xls`, hoac `.docx`
- Lesson plan/revision phai la `.docx`

Luu y:

- `RAR` chua duoc support trong code hien tai
- Neu can upload `RAR`, FE nen yeu cau user doi sang `ZIP`

---

## 3. Import Archive

### `POST /api/syllabuses/import-archive`

Dung cho 1 goi curriculum day du.

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: string`
- `overwriteExisting: boolean = true`

Multipart:

- `file: .zip`

Behavior moi:

- BE uu tien syllabus Excel trong folder `PPCT`
- Neu trong `PPCT` co ca `xlsx/xls` va `docx`, BE uu tien `xlsx/xls` truoc
- BE nhan lesson plan DOCX trong `UNIT ...` va `REVISION`
- BE luu file URL goc vao DB:
  - syllabus: `attachmentUrl`
  - lesson plan: `attachmentUrl`
- BE tra metadata nguon parse de FE debug nhanh neu chon nham file
- BE fail-fast neu curriculum table bi parse thieu row so voi lesson count tu source

Success response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "uuid",
    "selectedSyllabusEntryName": "PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx",
    "selectedSyllabusSourceType": "SyllabusDocument",
    "selectedSyllabusParserVersion": "excel-v1",
    "importedLessonPlans": 49,
    "skippedFiles": 2,
    "importedEntries": [
      {
        "entryName": "PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx",
        "sourceFolder": "PPCT GET STARTER",
        "sourceType": "SyllabusDocument",
        "parserVersion": "excel-v1",
        "isPrimarySyllabusSource": true,
        "created": true,
        "title": "get_ready_for_starters_import_ready"
      },
      {
        "entryName": "UNIT 1/Unit 1 lesson 1.docx",
        "sourceFolder": "UNIT 1",
        "sourceType": "UnitLesson",
        "parserVersion": "docx-v1",
        "isPrimarySyllabusSource": false,
        "moduleId": "uuid",
        "moduleName": "Unit 1",
        "lessonPlanTemplateId": "uuid",
        "sessionTemplateId": "uuid",
        "sessionIndex": 1,
        "sessionOrder": 1,
        "created": true,
        "title": "UNIT 1: I LOVE ANIMALS!"
      }
    ],
    "skippedEntries": [
      "REVISION/~$Revision 01.docx: ..."
    ]
  }
}
```

Common errors:

- `400 Syllabus.UnsupportedImportFileType`
- `400 Syllabus.InvalidImportFile`
- `404 Syllabus.ImportConfigurationNotFound`
- `409 Syllabus.DuplicateVersion`

FE notes:

- Sau import thanh cong, luu `syllabusId`
- Sau do goi `GET /api/syllabuses/{syllabusId}`
- FE nen hien `selectedSyllabusEntryName` + `selectedSyllabusParserVersion` o man debug/import result
- Neu `selectedSyllabusParserVersion = excel-v1` thi syllabus dang render tu Excel source
- FE khong doc file nén de tu render

---

## 4. Syllabus Detail

### `GET /api/syllabuses/{id}`

API nay van la API detail chinh cho admin.

Noi dung moi quan trong:

- `document.sections[]` da duoc build tu data parse/import
- `attachmentUrl` la file syllabus goc neu import archive da upload len storage
- lesson plan goc da duoc link qua `sessionTemplates[]`

FE dung khi can:

- hien tong quan syllabus
- debug import result
- mo editor/admin detail

---

## 5. Lesson JSON For FE

### `GET /api/lessons/{lessonCode}`

Day la endpoint FE nen dung de render lesson plan tren web.

FE khong can doc `DOCX`.

Example:

`GET /api/lessons/unit_1_lesson_1`

Response:

```json
{
  "isSuccess": true,
  "data": {
    "courseCode": "get_ready_for_starters",
    "unitCode": "unit_1",
    "lessonCode": "unit_1_lesson_1",
    "type": "lesson",
    "title": "UNIT 1: I LOVE ANIMALS!",
    "lessonNo": 1,
    "objectives": [
      "Children can identify animals",
      "Children can describe pictures"
    ],
    "languageContent": {
      "vocabulary": [
        "cat",
        "dog"
      ],
      "grammar": [
        "Present simple tense with be"
      ]
    },
    "materials": {
      "teacher": [
        "Flashcards",
        "Teacher book"
      ],
      "students": [
        "Student book",
        "Workbook"
      ]
    },
    "procedure": [
      {
        "stageNo": 1,
        "stage": "Warm-up",
        "details": [
          "Greeting students",
          "Review old vocabulary"
        ]
      }
    ],
    "homework": [
      "Do workbook page 6"
    ],
    "evaluation": "Observe speaking performance in pair work",
    "sourceFileUrl": "/storage/curriculum/.../lesson-1.docx"
  }
}
```

Field meaning:

- `type`:
  - `lesson`
  - `revision`
- `sourceFileUrl`:
  - FE dung de tao nut `Download original DOCX`
  - FE khong can parse file nay

---

## 6. Lesson Code Rule

BE dang expose lesson theo `lessonCode`.

Pattern hien tai:

- `unit_starter_lesson_1`
- `unit_1_lesson_1`
- `unit_1_lesson_2`
- `revision_1_lesson_1`

FE rule:

- khong tu generate code neu chua co source on dinh
- nen lay `lessonCode` tu danh sach/module/session payload cua BE khi co
- chi dung hardcode pattern tren cho debug tam thoi

---

## 7. FE Download Behavior

Neu can tai file goc:

- syllabus file: dung `attachmentUrl` tu syllabus detail
- lesson plan file: dung `sourceFileUrl` tu `GET /api/lessons/{lessonCode}`

UI de xuat:

- Render lesson plan bang JSON
- Them 1 button `Download original DOCX`

---

## 8. Quick FE Checklist

1. Man import curriculum chi upload `ZIP`
2. Khong cho FE doc archive structure
3. Sau import thanh cong, refresh syllabus detail bang `syllabusId`
4. Man lesson detail goi `GET /api/lessons/{lessonCode}`
5. Nut download dung `sourceFileUrl`
6. Neu user upload `RAR`, FE can bao doi sang `ZIP`
