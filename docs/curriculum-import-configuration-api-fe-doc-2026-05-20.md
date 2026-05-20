# FE API Doc - Curriculum Import Configuration

Updated date: 2026-05-20

## 1. Scope

Tai lieu nay mo ta API FE can dung de:

- cau hinh rule map `Unit` / `Revision` vao `Module`
- cau hinh so lesson plan ky vong cho moi `Unit`
- bat buoc cau hinh truoc khi goi import curriculum zip

Tai lieu nay chi cover nhom API moi lien quan den:

- `GET /api/syllabuses/import-configuration`
- `PUT /api/syllabuses/import-configuration`
- luong goi `POST /api/syllabuses/import-archive` sau khi da cau hinh
- luong goi `POST /api/syllabuses/import-lesson-plan-words` de import nhieu file Word lesson plan khong can zip

## 2. Auth And Response

Tat ca endpoint trong file nay deu can:

- `Authorization: Bearer <token>`

Role duoc phep:

- `Admin`
- `ManagementStaff`

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
  "title": "Syllabus.InvalidImportConfiguration",
  "status": 400,
  "detail": "Each module can only appear once in curriculum import rules.",
  "errors": [
    {
      "code": "Syllabus.InvalidImportConfiguration",
      "description": "Each module can only appear once in curriculum import rules."
    }
  ]
}
```

Auth errors:

- `401`: token thieu, sai, het han
- `403`: da authenticate nhung khong du role

## 3. FE Flow

FE can di theo thu tu nay:

1. Lay danh sach `Module` cua `Level`.
2. Goi `PUT /api/syllabuses/import-configuration` de luu rule mapping.
3. Co the goi `GET /api/syllabuses/import-configuration` de load lai config da luu.
4. Chi sau khi da co config moi goi `POST /api/syllabuses/import-archive`.

Neu bo qua buoc 2, backend co the tra:

- `Syllabus.ImportConfigurationNotFound`

## 4. Base Path

Base path:

- `/api/syllabuses`

## 5. Get Import Configuration

### GET `/api/syllabuses/import-configuration`

Dung de load cau hinh import hien tai cua 1 `Program + Level`.

Query params:

- `programId: Guid`
- `levelId: Guid`

Success data:

```json
{
  "id": "4cbcd1c0-72ac-4d75-b5ae-4a0995e74e31",
  "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
  "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
  "regularUnitLessonPlanCount": 3,
  "starterUnitLessonPlanCount": 2,
  "revisionLessonPlanCount": 1,
  "isActive": true,
  "rules": [
    {
      "id": "1b8f5f2f-fc6a-43e6-b9dc-17dff46f837b",
      "moduleId": "11111111-1111-1111-1111-111111111111",
      "moduleCode": "STARTERS_STATE01",
      "moduleName": "Starter01",
      "moduleOrder": 1,
      "includeStarterUnit": true,
      "unitFrom": 1,
      "unitTo": 5,
      "revisionNumber": 1,
      "orderIndex": 1,
      "expectedLessonPlanCount": 18
    },
    {
      "id": "55e5fdde-fcd2-4d3f-89bf-46599752b4c9",
      "moduleId": "22222222-2222-2222-2222-222222222222",
      "moduleCode": "STARTERS_STATE02",
      "moduleName": "Starter02",
      "moduleOrder": 2,
      "includeStarterUnit": false,
      "unitFrom": 6,
      "unitTo": 10,
      "revisionNumber": 2,
      "orderIndex": 2,
      "expectedLessonPlanCount": 16
    },
    {
      "id": "92f4b665-72a8-4c45-8a48-8a2e59274a12",
      "moduleId": "33333333-3333-3333-3333-333333333333",
      "moduleCode": "STARTERS_STATE03",
      "moduleName": "Starter03",
      "moduleOrder": 3,
      "includeStarterUnit": false,
      "unitFrom": 11,
      "unitTo": 15,
      "revisionNumber": 3,
      "orderIndex": 3,
      "expectedLessonPlanCount": 16
    }
  ]
}
```

Field meanings:

- `regularUnitLessonPlanCount`: so file lesson plan ky vong cho moi `Unit` thuong
- `starterUnitLessonPlanCount`: so file lesson plan ky vong cho `Unit Starter`
- `revisionLessonPlanCount`: so file lesson plan ky vong cho moi `Revision`
- `includeStarterUnit`: module nay co gom `Unit Starter` hay khong
- `unitFrom`, `unitTo`: khoang `Unit` map vao module nay
- `revisionNumber`: `Revision` nao map vao module nay
- `expectedLessonPlanCount`: backend tinh san tong so lesson plan du kien cho module

Common errors:

- `Syllabus.ImportConfigurationNotFound`

## 6. Upsert Import Configuration

### PUT `/api/syllabuses/import-configuration`

Dung de tao moi hoac cap nhat config import cho 1 `Program + Level`.

Query params:

- `programId: Guid`
- `levelId: Guid`

Request body:

```json
{
  "regularUnitLessonPlanCount": 3,
  "starterUnitLessonPlanCount": 2,
  "revisionLessonPlanCount": 1,
  "isActive": true,
  "rules": [
    {
      "moduleId": "11111111-1111-1111-1111-111111111111",
      "includeStarterUnit": true,
      "unitFrom": 1,
      "unitTo": 5,
      "revisionNumber": 1,
      "orderIndex": 1
    },
    {
      "moduleId": "22222222-2222-2222-2222-222222222222",
      "includeStarterUnit": false,
      "unitFrom": 6,
      "unitTo": 10,
      "revisionNumber": 2,
      "orderIndex": 2
    },
    {
      "moduleId": "33333333-3333-3333-3333-333333333333",
      "includeStarterUnit": false,
      "unitFrom": 11,
      "unitTo": 15,
      "revisionNumber": 3,
      "orderIndex": 3
    }
  ]
}
```

Success data:

```json
{
  "id": "4cbcd1c0-72ac-4d75-b5ae-4a0995e74e31",
  "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
  "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
  "regularUnitLessonPlanCount": 3,
  "starterUnitLessonPlanCount": 2,
  "revisionLessonPlanCount": 1,
  "isActive": true,
  "rules": [
    {
      "id": "1b8f5f2f-fc6a-43e6-b9dc-17dff46f837b",
      "moduleId": "11111111-1111-1111-1111-111111111111",
      "moduleCode": "STARTERS_STATE01",
      "moduleName": "Starter01",
      "moduleOrder": 1,
      "includeStarterUnit": true,
      "unitFrom": 1,
      "unitTo": 5,
      "revisionNumber": 1,
      "orderIndex": 1,
      "expectedLessonPlanCount": 18
    }
  ]
}
```

Business rules FE nen validate truoc:

- `regularUnitLessonPlanCount > 0`
- `starterUnitLessonPlanCount > 0`
- `revisionLessonPlanCount > 0`
- `rules` khong duoc rong
- moi `moduleId` chi duoc xuat hien 1 lan
- moi `orderIndex` phai unique
- chi 1 rule duoc `includeStarterUnit = true`
- `unitFrom` va `unitTo` phai di cung nhau
- neu co `unitFrom/unitTo` thi phai `> 0` va `unitFrom <= unitTo`
- cac khoang `Unit` khong duoc overlap
- `revisionNumber` neu co thi phai `> 0` va unique

Common errors:

- `Syllabus.LevelNotFound`
- `Syllabus.LevelDoesNotBelongToProgram`
- `Syllabus.InvalidImportConfiguration`

Note:

- Backend se cap nhat luon `Module.PlannedSessionCount` dua theo config nay.
- `PUT` la upsert. Neu config da ton tai, backend se replace rule cu bang rule moi.

## 7. Example For Starters

Neu level `Starters` co 3 module:

- `Starter01`
- `Starter02`
- `Starter03`

Va syllabus quy uoc:

- `Unit Starter`, `Unit 1..5`, `Revision 1` thuoc `Starter01`
- `Unit 6..10`, `Revision 2` thuoc `Starter02`
- `Unit 11..15`, `Revision 3` thuoc `Starter03`

Thi FE co the dung config nhu sau:

```json
{
  "regularUnitLessonPlanCount": 3,
  "starterUnitLessonPlanCount": 2,
  "revisionLessonPlanCount": 1,
  "isActive": true,
  "rules": [
    {
      "moduleId": "module-id-starter01",
      "includeStarterUnit": true,
      "unitFrom": 1,
      "unitTo": 5,
      "revisionNumber": 1,
      "orderIndex": 1
    },
    {
      "moduleId": "module-id-starter02",
      "includeStarterUnit": false,
      "unitFrom": 6,
      "unitTo": 10,
      "revisionNumber": 2,
      "orderIndex": 2
    },
    {
      "moduleId": "module-id-starter03",
      "includeStarterUnit": false,
      "unitFrom": 11,
      "unitTo": 15,
      "revisionNumber": 3,
      "orderIndex": 3
    }
  ]
}
```

Khi ghi vao request that, FE phai thay `module-id-starter01/02/03` bang `Guid` thuc te lay tu API `Module`.

## 8. Import Archive Flow

### POST `/api/syllabuses/import-archive`

Endpoint nay van giu contract cu:

- `multipart/form-data`
- query:
  - `programId`
  - `levelId`
  - `code`
  - `version`
  - `overwriteExisting`
- form-data:
  - `file`

Frontend flow de xuat:

1. User chon `Program` va `Level`.
2. FE load `Module`.
3. FE cho admin map rule.
4. FE `PUT import-configuration`.
5. FE upload zip vao `POST import-archive`.
6. FE show:
   - `syllabusId`
   - `importedLessonPlans`
   - `importedEntries`
   - `skippedFiles`
   - `skippedEntries`

Neu chua config ma import ngay, backend se tra:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Syllabus.ImportConfigurationNotFound",
  "status": 404,
  "detail": "Curriculum import configuration for Program '...' and Level '...' was not found",
  "errors": [
    {
      "code": "Syllabus.ImportConfigurationNotFound",
      "description": "Curriculum import configuration for Program '...' and Level '...' was not found"
    }
  ]
}
```

Response sau khi import archive co them `importedEntries` de debug mapping:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "78448399-1933-49f1-a3fd-492a922d674f",
    "importedLessonPlans": 50,
    "skippedFiles": 0,
    "importedEntries": [
      {
        "entryName": "LESSON PLAN GET READY STARTER 2ED/UNIT 2/Unit 2 at home lesson 1 done.docx",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "lessonPlanTemplateId": "f4758abc-a965-4205-8f8f-d3b776463b55",
        "sessionTemplateId": "bd548eca-a5ea-4f7b-a583-9172f1115dc6",
        "sessionIndex": 5,
        "created": true,
        "title": "UNIT 2: AT HOME - Lesson 1"
      }
    ],
    "skippedEntries": []
  }
}
```

Field `created`:

- `true`: backend tao moi `LessonPlanTemplate`
- `false`: backend ghi de template da ton tai cung `ModuleId + SessionIndex`

Neu sau khi import zip ma `importedEntries.sessionIndex` chi quanh `1..3`, API dang chay code cu hoac FE dang goi endpoint import Word don theo `moduleId`, khong phai endpoint archive/config-aware.

## 9. Import Lesson Plan Word Files Without Zip

### POST `/api/syllabuses/import-lesson-plan-words`

Dung khi admin muon import thu cong nhieu file Word lesson plan theo list, khong import ca zip.

Endpoint nay khong import syllabus Word. Neu can import syllabus Word rieng, dung endpoint cu:

- `POST /api/syllabuses/import-word`

Query params:

- `programId: Guid`
- `levelId: Guid`
- `overwriteExisting: boolean`, default `true`
- `moduleId: Guid`, optional

Form-data:

- `files: File[]`

Recommended FE flow:

1. Admin chon `Program + Level`.
2. FE dam bao da co import configuration cho `Program + Level`.
3. Admin chon nhieu file `.docx` lesson plan.
4. FE submit tat ca file bang form key `files`.
5. Backend tu doc ten file de map `Unit/Revision/Lesson` vao module va session index.

Mapping mode:

- Neu khong gui `moduleId`, backend bat buoc co active import configuration va tu map theo rule.
- Neu gui `moduleId`, backend import tat ca file vao module do va session index lay theo `Lesson n` trong file Word. Mode nay dung khi admin muon override module thu cong.

Ten file can co pattern de backend map dung:

- `Unit starter ... lesson 1.docx`
- `Unit 4 Food lesson 2.docx`
- `Unit 10 Your day lesson 3.docx`
- `Revision 1.docx`
- `Revision 2 lesson 1.docx`

Response example:

```json
{
  "isSuccess": true,
  "data": {
    "importedLessonPlans": 3,
    "skippedFiles": 1,
    "importedEntries": [
      {
        "fileName": "Unit 4 Food lesson 1 done.docx",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "lessonPlanTemplateId": "f4758abc-a965-4205-8f8f-d3b776463b55",
        "sessionTemplateId": "bd548eca-a5ea-4f7b-a583-9172f1115dc6",
        "sessionIndex": 11,
        "title": "UNIT 4: Food - Lesson 1"
      }
    ],
    "skippedEntries": [
      "Bad file.pdf: Unsupported file type '.pdf'. Only .docx is supported"
    ]
  }
}
```

Important behavior:

- Backend tinh `sessionIndex` tuyet doi trong module theo config.
- Vi du module gom `Unit Starter`, `Unit 1..5`, `Revision 1`; config `starter=1`, `regular=3`, `revision=1`.
- `Unit 1 lesson 1` -> session `2`.
- `Unit 1 lesson 3` -> session `4`.
- `Unit 2 lesson 1` -> session `5`.
- `Revision 1` -> session sau cac unit trong rule.
- Neu backend khong resolve duoc `sessionIndex`, file se vao `skippedEntries`, khong fallback ve `Lesson 1` de tranh overwrite nham.

Curl example:

```bash
curl -X POST \
  "https://localhost:7235/api/syllabuses/import-lesson-plan-words?programId=48eba459-7a08-4461-b1f9-acec097c6185&levelId=fab421d5-89e0-43e7-b058-ab37f9d48a87&overwriteExisting=true" \
  -H "Authorization: Bearer <token>" \
  -F "files=@Unit 4 Food lesson 1 done.docx;type=application/vnd.openxmlformats-officedocument.wordprocessingml.document" \
  -F "files=@Unit 4 Food lesson 2 done.docx;type=application/vnd.openxmlformats-officedocument.wordprocessingml.document"
```

## 10. FE Recommendations

- FE nen co man hinh config rieng cho `Program + Level`.
- FE nen disable nut import zip neu config chua duoc luu.
- FE nen disable nut import lesson plan Word list neu config chua duoc luu, tru khi FE gui ro `moduleId`.
- FE nen hien `expectedLessonPlanCount` de admin tu check rule co hop ly khong.
- FE nen validate overlap range ngay tren form truoc khi submit.
- FE nen luu `moduleId` theo dropdown chon tu danh sach module, khong cho user go text tu do.
- FE nen cho preview summary truoc khi save, vi config nay anh huong truc tiep den mapping khi import zip.
