# FE API Doc - Curriculum Import And Configuration

Updated date: 2026-06-05

Base path: `/api/syllabuses`

Auth:

- `Authorization: Bearer <token>`
- Roles: `Admin`, `ManagementStaff`

Success envelope:

```json
{
  "isSuccess": true,
  "data": {}
}
```

## 0. Update 2026-06-05

Backend da bo khái niệm `Unit Starter` rieng trong import configuration.

- Khong con field `starterUnitLessonPlanCount`
- Khong con field `includeStarterUnit`
- `Unit 0` duoc xem la 1 unit so binh thuong trong range `unitFrom..unitTo`
- FE can gui rule kieu `unitFrom = 0`, `unitTo = 5` neu module dau gom `Unit 0` den `Unit 5`
- Ten file Word/zip nen dung `Unit 0 lesson 1.docx`, `Unit 0 lesson 2.docx`
- Backend van co the doc duoc noi dung legacy `Unit Starter` ben trong file Word de phuc vu import ngoc, nhung canonical output va config moi deu dung `Unit 0`

Luu y:

- Cac example cu ben duoi co the van con chu `Unit Starter`. Khi implement FE moi, uu tien follow cac bullet cap nhat o muc nay.

## 1. Correct Flow

Flow import full zip:

1. Chon `Program` va `Level`.
2. Load danh sach `Module` cua `Level`.
3. Load config hien co bang `GET /api/syllabuses/import-configuration`.
4. Tao/cap nhat config bang `PUT /api/syllabuses/import-configuration`.
5. Upload zip bang `POST /api/syllabuses/import-archive`.
6. Lay `syllabusId` tu response.
7. Refresh UI bang `GET /api/syllabuses/{syllabusId}` hoac `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.

Flow import thu cong lesson plan Word:

1. Dam bao da co syllabus. Co the tao bang `POST /api/syllabuses/import-word` neu chua import zip.
2. Dam bao da co config neu FE khong gui `moduleId`.
3. Upload list Word bang `POST /api/syllabuses/import-lesson-plan-words`.
4. Refresh UI bang `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.

Important:

- `import-archive` bat buoc can active config.
- `import-lesson-plan-words` bat buoc can active config khi khong gui `moduleId`.
- `import-archive` se scan zip de lay count thuc te trong archive. Neu module dau map `unitFrom = 0`, `unitTo = 5` thi `Unit 0` duoc tinh nhu 1 unit thuong.
- `import-lesson-plan-words` khong scan ca zip, nen backend van resolve session theo config dang active.
- Nen gui `overwriteExisting=true` khi admin import lai cung curriculum.

## 2. Import Configuration

### GET `/api/syllabuses/import-configuration`

Load config cua `Program + Level`.

Query params:

- `programId: Guid`
- `levelId: Guid`

Response data:

```json
{
  "id": "4cbcd1c0-72ac-4d75-b5ae-4a0995e74e31",
  "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
  "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
  "regularUnitLessonPlanCount": 3,
  "revisionLessonPlanCount": 1,
  "isActive": true,
  "rules": [
    {
      "id": "1b8f5f2f-fc6a-43e6-b9dc-17dff46f837b",
      "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
      "moduleCode": "STATERS_STATE01",
      "moduleName": "Stater01",
      "moduleOrder": 1,
      "unitFrom": 0,
      "unitTo": 5,
      "revisionNumber": 1,
      "orderIndex": 1,
      "expectedLessonPlanCount": 19
    }
  ]
}
```

Common error:

- `404 Syllabus.ImportConfigurationNotFound`

### PUT `/api/syllabuses/import-configuration`

Create/update config. Backend replace rule cu bang rule moi.

Query params:

- `programId: Guid`
- `levelId: Guid`

Request body cho Starters:

```json
{
  "regularUnitLessonPlanCount": 3,
  "revisionLessonPlanCount": 1,
  "isActive": true,
  "rules": [
    {
      "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
      "unitFrom": 0,
      "unitTo": 5,
      "revisionNumber": 1,
      "orderIndex": 1
    },
    {
      "moduleId": "5f1d6276-9099-431d-8486-091d9ab4a365",
      "unitFrom": 6,
      "unitTo": 10,
      "revisionNumber": 2,
      "orderIndex": 2
    },
    {
      "moduleId": "627f926c-f077-4eaf-b214-6f007d32a087",
      "unitFrom": 11,
      "unitTo": 15,
      "revisionNumber": 3,
      "orderIndex": 3
    }
  ]
}
```

Meaning:

- `regularUnitLessonPlanCount`: so lesson plan mac dinh cho moi Unit thuong.
- `revisionLessonPlanCount`: so lesson plan mac dinh cho moi Revision.
- `unitFrom`, `unitTo`: khoang Unit map vao module, co the bat dau tu `0`.
- `revisionNumber`: Revision map vao module.
- `expectedLessonPlanCount`: backend tinh tu config, FE dung de hien preview.

FE validate:

- Count fields phai `> 0`.
- `rules` khong rong.
- `moduleId` unique.
- `orderIndex` unique.
- `unitFrom` va `unitTo` phai di cung nhau.
- `unitFrom <= unitTo`.
- Unit range khong overlap.
- `revisionNumber` unique neu co.

## 3. Import Full Zip

### POST `/api/syllabuses/import-archive`

Import 1 zip gom syllabus Word va lesson plan Word.

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: string`
- `overwriteExisting: boolean`, default `true`

Form-data:

- `file: .zip`

Curl:

```bash
curl -X POST \
  "https://localhost:7235/api/syllabuses/import-archive?programId=48eba459-7a08-4461-b1f9-acec097c6185&levelId=fab421d5-89e0-43e7-b058-ab37f9d48a87&code=Starters&version=1&overwriteExisting=true" \
  -H "Authorization: Bearer <token>" \
  -F "file=@LESSON PLAN GET READY STARTER 2ED.zip;type=application/x-zip-compressed"
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "78448399-1933-49f1-a3fd-492a922d674f",
    "importedLessonPlans": 50,
    "skippedFiles": 0,
    "importedEntries": [
      {
        "entryName": "LESSON PLAN GET READY STARTER 2ED/UNIT STARTER/Unit starter Hello lesson 2 done.docx",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "lessonPlanTemplateId": "f4758abc-a965-4205-8f8f-d3b776463b55",
        "sessionTemplateId": "bd548eca-a5ea-4f7b-a583-9172f1115dc6",
        "sessionIndex": 2,
        "created": true,
        "title": "UNIT STARTER: HELLO! - Lesson 2"
      }
    ],
    "skippedEntries": []
  }
}
```

Backend behavior:

- Tim syllabus Word trong PPCT va import truoc.
- Tao/cap nhat `Syllabuses`, `SyllabusUnits`, `SyllabusLessons`, `SessionTemplates`.
- Scan lesson plan Word trong zip.
- Map theo `Unit Starter`, `Unit n`, `Revision n`, `lesson n`.
- Tu nang count toi thieu theo file that trong zip de khong skip file hop le.
- Ghi de `LessonPlanTemplate` cu theo `ModuleId + SessionIndex` khi `overwriteExisting=true`.
- Link lesson plan vao `SessionTemplate` neu resolve duoc `ModuleId + SessionIndex`.

Common errors:

- `404 Syllabus.ImportConfigurationNotFound`
- `400 Syllabus.InvalidImportFile`
- `400 LessonPlanTemplate.SessionIndexOutOfRange`
- `409 Syllabus.DuplicateVersion` khi `overwriteExisting=false`

## 4. Import Syllabus Word Only

### POST `/api/syllabuses/import-word`

Dung khi admin chi import file syllabus Word, khong import lesson plan Word.

Query params:

- `programId: Guid`
- `levelId: Guid`
- `code: string`
- `version: string`
- `overwriteExisting: boolean`, default `true`

Form-data:

- `file: .docx`

Response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "78448399-1933-49f1-a3fd-492a922d674f",
    "importedUnits": 19,
    "importedLessons": 50,
    "importedResources": 3,
    "importedSessionTemplates": 50
  }
}
```

## 5. Import Lesson Plan Word List

### POST `/api/syllabuses/import-lesson-plan-words`

Import thu cong nhieu file lesson plan Word.

Query params:

- `programId: Guid`
- `levelId: Guid`
- `moduleId: Guid?`, optional
- `overwriteExisting: boolean`, default `true`

Form-data:

- `files: File[]`

Curl khong gui `moduleId`:

```bash
curl -X POST \
  "https://localhost:7235/api/syllabuses/import-lesson-plan-words?programId=48eba459-7a08-4461-b1f9-acec097c6185&levelId=fab421d5-89e0-43e7-b058-ab37f9d48a87&overwriteExisting=true" \
  -H "Authorization: Bearer <token>" \
  -F "files=@Unit starter hello lesson 1 done.docx;type=application/vnd.openxmlformats-officedocument.wordprocessingml.document" \
  -F "files=@Unit starter Hello lesson 2 done.docx;type=application/vnd.openxmlformats-officedocument.wordprocessingml.document"
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "importedLessonPlans": 2,
    "skippedFiles": 0,
    "importedEntries": [
      {
        "fileName": "Unit starter Hello lesson 2 done.docx",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "lessonPlanTemplateId": "f4758abc-a965-4205-8f8f-d3b776463b55",
        "sessionTemplateId": "bd548eca-a5ea-4f7b-a583-9172f1115dc6",
        "sessionIndex": 2,
        "created": false,
        "title": "UNIT STARTER: HELLO! - Lesson 2"
      }
    ],
    "skippedEntries": []
  }
}
```

Mapping modes:

- Khong gui `moduleId`: backend dung active import configuration. FE nen dung mode nay.
- Co gui `moduleId`: backend import tat ca file vao module do. Dung cho override thu cong.

Filename patterns nen dung:

- `Unit starter hello lesson 1.docx`
- `Unit starter Hello lesson 2.docx`
- `Unit 4 Food lesson 2.docx`
- `Unit 10 Your day lesson 3.docx`
- `Revision 1.docx`
- `Revision 2 lesson 1.docx`

## 6. View Syllabus Detail

### GET `/api/syllabuses/{syllabusId}`

Dung de xem syllabus detail, units, lessons, resources, session templates, va summary lesson plan template da link.

Path params:

- `syllabusId: Guid`

## 7. View Imported Lesson Plans By Unit

### GET `/api/syllabuses/{syllabusId}/unit-lesson-plans`

Dung de hien lesson plan Word da import, group theo `Unit Starter`, `Unit n`, `Revision n`.

Path params:

- `syllabusId: Guid`

Response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "78448399-1933-49f1-a3fd-492a922d674f",
    "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
    "programName": "Kids English",
    "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
    "levelName": "Starters",
    "totalGroups": 19,
    "totalLessonPlans": 50,
    "groups": [
      {
        "groupKey": "unit-starter",
        "groupType": "UnitStarter",
        "unitNumber": null,
        "revisionNumber": null,
        "displayName": "Unit Starter",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "moduleCode": "STATERS_STATE01",
        "moduleName": "Stater01",
        "moduleOrder": 1,
        "lessonPlanCount": 2,
        "lessonPlans": [
          {
            "lessonPlanTemplateId": "2aa7f41d-7ef0-4eb9-b655-90abec1f8d33",
            "sessionTemplateId": "fbd0ff0d-8667-4e56-8bc4-a7b7c7d3d9cf",
            "title": "UNIT STARTER: HELLO! - Lesson 1",
            "lessonNumber": 1,
            "sessionIndex": 1,
            "sessionOrder": 1,
            "sessionIndexInModule": 1,
            "sessionTitle": "Hello",
            "sessionTopic": "Hello",
            "sourceFileName": "Unit starter hello lesson 1 done.docx",
            "isActive": true,
            "createdAt": "2026-05-20T02:30:00Z",
            "updatedAt": "2026-05-20T02:30:00Z"
          }
        ]
      }
    ]
  }
}
```

Important:

- `totalGroups`: voi Starters mong doi la `19`.
- `totalLessonPlans`: voi Starters mong doi la `50`.
- API nay chi tra lesson plan da link voi syllabus qua `SessionTemplateId`.
- Neu data cu bi overwrite/null link, import lai zip voi `overwriteExisting=true`.

## 8. UI Recommendations

- Disable import zip neu chua co config.
- Hien `expectedLessonPlanCount` theo module tren man hinh config.
- Sau import zip, hien `importedLessonPlans`, `skippedFiles`, `skippedEntries`.
- Neu co skipped entry `Could not resolve session index from import configuration`, bao admin kiem tra filename pattern va config count.
- Sau import thanh cong, refresh bang `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.
