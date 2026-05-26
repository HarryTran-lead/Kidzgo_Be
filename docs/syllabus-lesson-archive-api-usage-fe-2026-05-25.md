# FE API Usage - Syllabus Archive And Lesson JSON

Updated: 2026-05-25  
Scope: cac API FE can biet sau khi BE doi sang flow `ZIP upload -> BE parse -> DB JSON -> FE render`.

Base paths:

- `/api/syllabuses`
- `/api/lessons`

Roles:

- Import/archive APIs: `Admin`, `ManagementStaff`
- Lesson read API: `Teacher`, `Admin`, `ManagementStaff`

Archive mau canonical dung cho test/import/regression:

- `C:\Users\ADMIN\Downloads\LESSON PLAN GET READY STARTER 2ED.zip`
- Day la file chuan thay cho cac ban timestamped/export lai truoc do
- Khi can doi chieu archive thuc te, uu tien file nay truoc

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

Voi archive canonical hien tai:

- BE scan de quy tat ca entry ben trong zip
- BE co the gap folder goc long nhau hoac file duplicate theo ten khac nhau
- FE khong nen gia dinh entryName luon bat dau truc tiep tu `PPCT ...` hoac `UNIT ...`

---

## 2. Archive Structure Supported

Archive hien duoc support o dang `ZIP`.

Archive canonical hien tai duoc ky vong co noi dung tuong duong file:

- `LESSON PLAN GET READY STARTER 2ED.zip`

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
- Archive co the co nhieu root folder long nhau; BE van scan duoc neu entry hop le

Luu y:

- `RAR` chua duoc support trong code hien tai
- Neu can upload `RAR`, FE nen yeu cau user doi sang `ZIP`

---

## 3. Import Archive

### `POST /api/syllabuses/import-archive`

Dung cho 1 goi curriculum day du.

Query params:

- `branchId: Guid?`
- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: string`
- `overwriteExisting: boolean = true`

Multipart:

- `file: .zip`

Behavior moi:

- Neu co `branchId`, BE se tu dong upsert mapping `CurriculumAssignment`
  cho `branchId + programId + levelId + syllabusId`
- Mapping nay giup syllabus vua import dung duoc ngay cho luong `Create Class`
- BE uu tien syllabus Excel trong folder `PPCT`
- Neu trong `PPCT` co ca `xlsx/xls` va `docx`, BE uu tien `xlsx/xls` truoc
- BE nhan lesson plan DOCX trong `UNIT ...` va `REVISION`
- BE phan loai `sourceType` theo entry path:
  - `SyllabusDocument`
  - `UnitLesson`
  - `RevisionLesson`
- BE gan `parserVersion` theo extension:
  - `.xlsx/.xls` -> `excel-v1`
  - `.docx` -> `docx-v1`
  - `.pdf` -> `pdf-v1`
- BE luu file URL goc vao DB:
  - syllabus: `attachmentUrl`
  - lesson plan: `attachmentUrl`
- BE tra metadata nguon parse de FE debug nhanh neu chon nham file
- BE fail-fast neu curriculum table bi parse thieu row so voi lesson count tu source

Success response sample voi archive canonical:

```json
{
  "isSuccess": true,
  "data": {
    "archiveFileName": "LESSON PLAN GET READY STARTER 2ED.zip",
    "archiveParserVersion": "zip-v1",
    "syllabusId": "uuid",
    "selectedSyllabusEntryName": "LESSON PLAN GET READY STARTER 2ED-20260518T163746Z-3-001/LESSON PLAN GET READY STARTER 2ED/PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx",
    "selectedSyllabusNormalizedEntryName": "LESSON PLAN GET READY STARTER 2ED-20260518T163746Z-3-001/LESSON PLAN GET READY STARTER 2ED/PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx",
    "selectedSyllabusFileName": "get_ready_for_starters_import_ready.xlsx",
    "selectedSyllabusSourceType": "SyllabusDocument",
    "selectedSyllabusParserVersion": "excel-v1",
    "importedLessonPlans": 49,
    "skippedFiles": 1,
    "importedEntries": [
      {
        "entryName": "LESSON PLAN GET READY STARTER 2ED-20260518T163746Z-3-001/LESSON PLAN GET READY STARTER 2ED/PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx",
        "normalizedEntryName": "LESSON PLAN GET READY STARTER 2ED-20260518T163746Z-3-001/LESSON PLAN GET READY STARTER 2ED/PPCT GET STARTER/get_ready_for_starters_import_ready.xlsx",
        "fileName": "get_ready_for_starters_import_ready.xlsx",
        "sourceFolder": "PPCT GET STARTER",
        "sourceType": "SyllabusDocument",
        "parserVersion": "excel-v1",
        "isPrimarySyllabusSource": true,
        "created": true,
        "title": "get_ready_for_starters_import_ready"
      },
      {
        "entryName": "LESSON PLAN GET READY STARTER 2ED-20260518T163746Z-3-001/LESSON PLAN GET READY STARTER 2ED/UNIT 1/Unit 1 lesson 1.docx",
        "normalizedEntryName": "LESSON PLAN GET READY STARTER 2ED-20260518T163746Z-3-001/LESSON PLAN GET READY STARTER 2ED/UNIT 1/Unit 1 lesson 1.docx",
        "fileName": "Unit 1 lesson 1.docx",
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
      "LESSON PLAN GET READY STARTER 2ED/REVISION/Revision 99.docx: Could not resolve session index from import configuration"
    ],
    "skippedItems": [
      {
        "entryName": "LESSON PLAN GET READY STARTER 2ED/REVISION/Revision 99.docx",
        "normalizedEntryName": "LESSON PLAN GET READY STARTER 2ED/REVISION/Revision 99.docx",
        "fileName": "Revision 99.docx",
        "sourceFolder": "REVISION",
        "sourceType": "RevisionLesson",
        "parserVersion": "docx-v1",
        "reason": "Could not resolve session index from import configuration"
      }
    ]
  }
}
```

Parser/debug fields:

- `selectedSyllabusEntryName`: entry duoc chon lam nguon syllabus chinh trong zip
- `archiveFileName`: ten file zip goc FE da upload
- `archiveParserVersion`: parser cap archive, hien tai la `zip-v1`
- `selectedSyllabusNormalizedEntryName`: duong dan entry sau khi BE normalize slash/space
- `selectedSyllabusFileName`: ten file syllabus sau khi tach khoi path
- `selectedSyllabusSourceType`: luon la `SyllabusDocument` neu chon thanh cong
- `selectedSyllabusParserVersion`: parser thuc te da dung cho syllabus source
- `importedEntries[].normalizedEntryName`: path sau khi normalize, dung de compare/debug
- `importedEntries[].fileName`: ten file goc cua entry
- `importedEntries[].sourceType`: loai entry sau khi BE classify theo path
- `importedEntries[].parserVersion`: parser da dung cho tung entry
- `importedEntries[].isPrimarySyllabusSource`: chi `true` cho entry syllabus chinh
- `skippedEntries[]`: danh sach text gon de toast/log nhanh
- `skippedItems[].normalizedEntryName`: path normalize cho item bi bo qua
- `skippedItems[].fileName`: ten file bi bo qua
- `skippedItems[].parserVersion`: parser du kien cua file bi bo qua
- `skippedItems[]`: danh sach co cau truc de FE render table debug

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

---

## 9. API Changes For FE

Phan nay tong hop cac API da doi trong tab chat nay ma FE can update.

### `POST /api/classes`

`CreateClass` bat buoc chay theo syllabus runtime.

Request body da co them:

- `syllabusId: Guid`

Sample:

```json
{
  "branchId": "uuid",
  "programId": "uuid",
  "levelId": "uuid",
  "syllabusId": "uuid",
  "startModuleId": "uuid",
  "startSessionIndex": 1,
  "code": "STARTERS-Q7-01",
  "title": "Starters Q7 Morning",
  "roomId": "uuid",
  "mainTeacherId": "uuid",
  "assistantTeacherId": "uuid",
  "slotTypeId": "uuid",
  "startDate": "2026-06-01",
  "endDate": "2026-08-31",
  "capacity": 16,
  "sessionsToGenerate": 24,
  "skipHolidays": true,
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "SA",
      "startTime": "08:00",
      "durationMinutes": 90
    }
  ],
  "description": "Starters 3rd edition class"
}
```

Response class object hien co them:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

Y nghia:

- FE phai cho user chon `Program -> Level -> Syllabus`
- Khong duoc coi syllabus la implicit theo level nua

### `POST /api/classes/preview-sessions`

Preview session cung dung chung `CreateClassRequest`, nen da can:

- `syllabusId`

FE phai gui cung syllabus ma user da chon, neu khong preview va create co the lech nhau.

### `PUT /api/classes/{id}`

`UpdateClass` da co them:

- `syllabusId`

Neu FE cho doi syllabus cua class, backend se validate lai branch/program/level/syllabus va resync runtime theo syllabus moi.

### `GET /api/classes`

Danh sach class da co them cac field de FE render syllabus runtime:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

UI goi y:

- list class nen hien badge kieu `Starters - v3`

### `GET /api/classes/{id}`

Chi tiet class da co them:

- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`

FE detail page nen hien ro:

- syllabus dang day
- version dang chay

### `GET /api/branches/{id}/syllabuses`

API moi cho man create/update class.

Dung de lay cac syllabus ma branch duoc phep dung.

Response item:

- `curriculumAssignmentId`
- `syllabusId`
- `programId`
- `programName`
- `levelId`
- `levelName`
- `code`
- `version`
- `title`
- `effectiveFrom`
- `effectiveTo`
- `isActive`

FE use case:

1. user chon branch
2. FE goi API nay
3. loc syllabus theo `programId` va `levelId`
4. render dropdown syllabus version

### `GET /api/syllabuses/versions`

API moi de list version syllabus.

Query params:

- `branchId: Guid?`
- `programId: Guid?`
- `levelId: Guid?`
- `activeOnly: boolean = true`

Response item:

- `syllabusId`
- `programId`
- `programName`
- `levelId`
- `levelName`
- `code`
- `version`
- `title`
- `edition`
- `isActive`

FE use case:

- preload dropdown syllabus version
- search/filter theo branch/program/level

### `POST /api/syllabuses/import-lesson-plan-words`

API nay da co them query param bat buoc:

- `syllabusId`

Full query hien tai:

- `programId`
- `levelId`
- `syllabusId`
- `moduleId?`
- `overwriteExisting`

Y nghia:

- import lesson plan word gio khong chi theo module nua
- phai bind vao syllabus runtime cu the

### `POST /api/syllabuses/import-archive`

Response da co them parser/debug fields moi:

- `archiveFileName`
- `archiveParserVersion`
- `selectedSyllabusNormalizedEntryName`
- `selectedSyllabusFileName`
- `importedEntries[].normalizedEntryName`
- `importedEntries[].fileName`
- `skippedItems[].normalizedEntryName`
- `skippedItems[].fileName`
- `skippedItems[].parserVersion`

Behavior moi:

- backend nhan them `branchId?`; neu co thi se gan syllabus vao branch ngay sau import
- backend tu chon entry lesson uu tien hon neu archive co duplicate
- cac entry duplicate bi dua vao `skippedItems[]` de FE debug

---

## 10. FE Impact Summary

Sau thay doi nay, FE can update it nhat cac man sau:

1. Create Class: them dropdown `Syllabus`
2. Update Class: cho edit `Syllabus`
3. Preview Sessions: gui kem `syllabusId`
4. Class List / Detail: render `syllabusCode`, `syllabusVersion`, `syllabusTitle`
5. Import Lesson Plan Words: them `syllabusId`
6. Import Archive/Import Word/Import Commit: neu muon tao class ngay sau import thi gui kem `branchId`
7. Import Archive Result: doc them parser/debug fields moi
