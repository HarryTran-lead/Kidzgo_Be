# Lesson Plan Template CRUD API FE Doc

Updated: 2026-06-08
Scope: file nay chi mo ta cac flow CRUD va endpoint lien quan truc tiep den `lesson plan template` cho frontend.

## 1. Tong quan

Base route:

- `api/lesson-plan-templates`

Tat ca endpoint deu can bearer token.

Role theo controller:

- `GET /api/lesson-plan-templates` -> `Teacher`, `ManagementStaff`, `Admin`
- `GET /api/lesson-plan-templates/{id}` -> `Teacher`, `ManagementStaff`, `Admin`
- `POST /api/lesson-plan-templates` -> `ManagementStaff`, `Admin`
- `PUT /api/lesson-plan-templates/{id}` -> `ManagementStaff`, `Admin`
- `DELETE /api/lesson-plan-templates/{id}` -> `ManagementStaff`, `Admin`
- `DELETE /api/lesson-plan-templates/{id}/hard-delete` -> `ManagementStaff`, `Admin`
- `PATCH /api/lesson-plan-templates/{id}/unit` -> `ManagementStaff`, `Admin`

## 2. Response convention

Success response:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Business error response dung `ProblemDetails`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "LessonPlanTemplate.SessionIndexRequired",
  "status": 400,
  "detail": "SessionIndex is required and must be greater than 0",
  "errors": [
    {
      "code": "LessonPlanTemplate.SessionIndexRequired",
      "description": "SessionIndex is required and must be greater than 0"
    }
  ]
}
```

Status mapping hien tai:

- `400` cho `Validation`
- `404` cho `NotFound`
- `409` cho `Conflict`
- `401/403` van co the xay ra tu ASP.NET auth layer

Luu y:

- `LessonPlanTemplate.Unauthorized` trong handler hien tai bi map ve `400`, khong phai `401/403`.

## 3. Data shape FE can dung

Field xuat hien o create/update/detail/list:

- `id`
- `syllabusId`
- `syllabusCode`
- `syllabusVersion`
- `syllabusTitle`
- `moduleId`
- `moduleCode`
- `moduleName`
- `lessonPlanUnitId`
- `lessonPlanUnitName`
- `orderIndexInUnit`
- `levelId`
- `levelName`
- `programId`
- `programName`
- `title`
- `sessionIndex`
- `sessionOrder`
- `syllabusMetadata`
- `syllabusContent`
- `objectives`
- `languageContent`
- `vocabulary`
- `grammar`
- `teachingMethodology`
- `teacherMaterials`
- `studentMaterials`
- `procedure`
- `evaluation`
- `sourceFileName`
- `attachment`
- `isActive`
- `createdBy`
- `createdByName`
- `createdAt`
- `updatedAt`
- `usedCount`

Luu y field:

- API dung `attachment`, khong dung `attachmentUrl`.
- `usedCount` = so `lesson plan` thuc te dang tham chieu template va chua bi delete.
- `lessonPlanUnitId` co the `null`.

## 4. Flow 1: List/Search templates

Endpoint:

- `GET /api/lesson-plan-templates`

Query params:

- `syllabusId: guid?`
- `moduleId: guid?`
- `title: string?`
- `isActive: boolean?`
- `includeDeleted: boolean = false`
- `pageNumber: number = 1`
- `pageSize: number = 10`

Vi du:

```http
GET /api/lesson-plan-templates?syllabusId=11111111-1111-1111-1111-111111111111&moduleId=22222222-2222-2222-2222-222222222222&pageNumber=1&pageSize=20
```

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "templates": {
      "items": [
        {
          "id": "33333333-3333-3333-3333-333333333333",
          "syllabusId": "11111111-1111-1111-1111-111111111111",
          "syllabusCode": "GRS-STARTERS",
          "syllabusVersion": 1,
          "syllabusTitle": "Get Ready Starters",
          "moduleId": "22222222-2222-2222-2222-222222222222",
          "moduleCode": "M1",
          "moduleName": "Module 1",
          "lessonPlanUnitId": "44444444-4444-4444-4444-444444444444",
          "lessonPlanUnitName": "Unit 1",
          "orderIndexInUnit": 0,
          "levelId": "55555555-5555-5555-5555-555555555555",
          "levelName": "Starters",
          "programId": "66666666-6666-6666-6666-666666666666",
          "programName": "Kids English",
          "title": "Lesson 1",
          "sessionIndex": 1,
          "sessionOrder": 1,
          "syllabusMetadata": "{}",
          "syllabusContent": "{}",
          "objectives": "Warm up and vocabulary",
          "languageContent": null,
          "vocabulary": "hello, goodbye",
          "grammar": null,
          "teachingMethodology": null,
          "teacherMaterials": null,
          "studentMaterials": null,
          "procedure": "Step 1...",
          "evaluation": null,
          "sourceFileName": "lesson-1.docx",
          "attachment": "https://cdn.example.com/lesson-1.docx",
          "isActive": true,
          "createdBy": "77777777-7777-7777-7777-777777777777",
          "createdByName": "Admin A",
          "createdAt": "2026-06-08T10:00:00",
          "updatedAt": "2026-06-08T10:00:00",
          "usedCount": 3
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

Behavior FE can rely on:

- Neu `includeDeleted = false`, BE tu loai template da soft-delete.
- Ket qua duoc sort theo:
  - `level.order`
  - `module.order`
  - `sessionOrder`
  - `sessionIndex`
- Filter `title` hien tai la exact match khong phan biet hoa thuong, khong phai `contains`.

Luu y quan trong:

- `Teacher` duoc goi list API nay.
- Handler list hien tai khong scope theo teacher/class/session, nen teacher co the nhan tat ca template theo filter da gui.
- Response list hien tai khong co field `isDeleted`.
- Neu FE goi `includeDeleted = true`, FE van khong phan biet duoc ro rang giua:
  - template bi soft-delete
  - template chi bi `isActive = false`

## 5. Flow 2: Get detail template

Endpoint:

- `GET /api/lesson-plan-templates/{id}`

Vi du:

```http
GET /api/lesson-plan-templates/33333333-3333-3333-3333-333333333333
```

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "syllabusId": "11111111-1111-1111-1111-111111111111",
    "syllabusCode": "GRS-STARTERS",
    "syllabusVersion": 1,
    "syllabusTitle": "Get Ready Starters",
    "moduleId": "22222222-2222-2222-2222-222222222222",
    "moduleCode": "M1",
    "moduleName": "Module 1",
    "lessonPlanUnitId": "44444444-4444-4444-4444-444444444444",
    "lessonPlanUnitName": "Unit 1",
    "orderIndexInUnit": 0,
    "levelId": "55555555-5555-5555-5555-555555555555",
    "levelName": "Starters",
    "programId": "66666666-6666-6666-6666-666666666666",
    "programName": "Kids English",
    "title": "Lesson 1",
    "sessionIndex": 1,
    "sessionOrder": 1,
    "syllabusMetadata": "{}",
    "syllabusContent": "{}",
    "objectives": "Warm up and vocabulary",
    "languageContent": null,
    "vocabulary": "hello, goodbye",
    "grammar": null,
    "teachingMethodology": null,
    "teacherMaterials": null,
    "studentMaterials": null,
    "procedure": "Step 1...",
    "evaluation": null,
    "sourceFileName": "lesson-1.docx",
    "attachment": "https://cdn.example.com/lesson-1.docx",
    "isActive": true,
    "createdBy": "77777777-7777-7777-7777-777777777777",
    "createdByName": "Admin A",
    "createdAt": "2026-06-08T10:00:00",
    "updatedAt": "2026-06-08T10:00:00",
    "usedCount": 3
  }
}
```

Teacher access rule:

- Teacher khong duoc doc detail moi template mot cach tu do.
- Teacher chi doc duoc template neu template do lien quan den session/class ma teacher dang duoc gan.
- Neu khong du dieu kien, handler tra `LessonPlanTemplate.Unauthorized` voi status `400`.
- Template da soft-delete se khong doc detail duoc nua va se tra `LessonPlanTemplate.NotFound`.

## 6. Flow 3: Create template

Endpoint:

- `POST /api/lesson-plan-templates`

Role:

- `ManagementStaff`
- `Admin`

Body:

```json
{
  "syllabusId": "11111111-1111-1111-1111-111111111111",
  "moduleId": "22222222-2222-2222-2222-222222222222",
  "lessonPlanUnitId": "44444444-4444-4444-4444-444444444444",
  "orderIndexInUnit": 0,
  "title": "Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "{}",
  "syllabusContent": "{}",
  "objectives": "Warm up and vocabulary",
  "languageContent": "",
  "vocabulary": "hello, goodbye",
  "grammar": "",
  "teachingMethodology": "",
  "teacherMaterials": "",
  "studentMaterials": "",
  "procedure": "Step 1...",
  "evaluation": "",
  "sourceFileName": "lesson-1.docx",
  "attachment": "https://cdn.example.com/lesson-1.docx"
}
```

Response:

- `201 Created`

Behavior:

- `sessionOrder` neu bo trong se mac dinh bang `sessionIndex`.
- Neu co `lessonPlanUnitId` nhung khong gui `orderIndexInUnit`, BE tu gan order tiep theo trong unit.
- Neu khong co `lessonPlanUnitId`, BE set:
  - `lessonPlanUnitId = null`
  - `orderIndexInUnit = 0`
- `isActive` luc create luon la `true`.

Validation/rule:

- `syllabusId` phai ton tai, active, va chua bi delete.
- `moduleId` phai ton tai.
- `syllabus` va `module` phai cung `level`.
- `sessionIndex` bat buoc `> 0`.
- `sessionIndex` khong duoc vuot `module.plannedSessionCount`.
- Khong duoc trung `(moduleId, syllabusId, sessionIndex)` voi template chua delete.
- Neu co `lessonPlanUnitId`, unit do phai active va thuoc cung `moduleId`.

Error FE nen map:

- `LessonPlanTemplate.ModuleNotFound` -> `404`
- `LessonPlanTemplate.SyllabusNotFound` -> `404`
- `LessonPlanTemplate.SyllabusModuleMismatch` -> `400`
- `LessonPlanTemplate.SessionIndexRequired` -> `400`
- `LessonPlanTemplate.SessionIndexOutOfRange` -> `400`
- `LessonPlanTemplate.DuplicateSessionIndex` -> `409`

Luu y:

- Hien tai CRUD handler khong validate `sessionOrder` range hay duplicate.
- FE nen tu validate `sessionOrder` theo UI neu muon tranh du lieu khong hop ly.

## 7. Flow 4: Update template

Endpoint:

- `PUT /api/lesson-plan-templates/{id}`

Role:

- `ManagementStaff`
- `Admin`

Body la partial update. Chi field nao khac `null` moi duoc ghi de.

Vi du:

```json
{
  "title": "Lesson 1 - updated",
  "sessionIndex": 2,
  "sessionOrder": 2,
  "objectives": "Review and practice",
  "procedure": "Step A...",
  "isActive": true
}
```

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "syllabusId": "11111111-1111-1111-1111-111111111111",
    "moduleId": "22222222-2222-2222-2222-222222222222",
    "lessonPlanUnitId": "44444444-4444-4444-4444-444444444444",
    "orderIndexInUnit": 0,
    "title": "Lesson 1 - updated",
    "sessionIndex": 2,
    "sessionOrder": 2,
    "objectives": "Review and practice",
    "procedure": "Step A...",
    "isActive": true,
    "updatedAt": "2026-06-08T11:00:00"
  }
}
```

Behavior quan trong:

- Neu field khong gui len, BE giu nguyen gia tri cu.
- Neu field gui len la `null`, BE cung bo qua, khong clear field.
- Muon xoa noi dung string, FE chi co the gui chuoi rong `""`, khong clear ve `null`.
- `attachment`, `title`, `objectives`, `procedure`... deu theo quy tac tren.

Rule khi update module/unit:

- Neu doi `moduleId`, BE validate module moi ton tai.
- Neu doi `syllabusId`, BE validate syllabus moi ton tai va cung level voi module dich.
- Neu doi `sessionIndex`, BE validate y het create flow.
- Neu gui `lessonPlanUnitId`, unit moi phai thuoc cung module dich.
- Neu doi `lessonPlanUnitId` bang `PUT` ma khong gui `orderIndexInUnit`, BE giu nguyen order cu, khong tu cap order tiep theo.
- Neu chi gui `orderIndexInUnit` ma template dang co unit, BE cap nhat order trong unit do.
- Neu doi `moduleId` ma khong gui `lessonPlanUnitId`, BE tu:
  - clear `lessonPlanUnitId`
  - set `orderIndexInUnit = 0`

Luu y rat quan trong:

- `PUT` khong phai endpoint phu hop de bo template ra khoi unit.
- Neu FE muon detach template khoi unit trong cung module, dung:
  - `PATCH /api/lesson-plan-templates/{id}/unit`
  - body: `{ "lessonPlanUnitId": null }`

Error FE nen map:

- `LessonPlanTemplate.NotFound` -> `404`
- `LessonPlanTemplate.ModuleNotFound` -> `404`
- `LessonPlanTemplate.SyllabusNotFound` -> `404`
- `LessonPlanTemplate.SyllabusModuleMismatch` -> `400`
- `LessonPlanTemplate.SessionIndexRequired` -> `400`
- `LessonPlanTemplate.SessionIndexOutOfRange` -> `400`
- `LessonPlanTemplate.DuplicateSessionIndex` -> `409`
- `LessonPlanTemplate.Unauthorized` -> `400`

## 8. Flow 5: Soft delete template

Endpoint:

- `DELETE /api/lesson-plan-templates/{id}`

Role:

- `ManagementStaff`
- `Admin`

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "isDeleted": true,
    "updatedAt": "2026-06-08T12:00:00"
  }
}
```

Behavior:

- Day la soft delete, khong xoa row khoi DB.
- BE set:
  - `isDeleted = true`
  - `isActive = false`
- BE dong thoi clear `SessionTemplates.LessonPlanTemplateId` dang tro vao template bi xoa.

Rule chan delete:

- Neu van con `LessonPlan` thuc te dang tham chieu template va chua bi delete, API tra:
  - `LessonPlanTemplate.HasActiveLessonPlans`
  - status `409`

FE flow goi y:

1. Goi detail hoac list de doc `usedCount`.
2. Neu `usedCount > 0`, canh bao user kha nang soft delete se that bai.
3. Neu user van muon xoa toan bo, xem flow hard delete ben duoi.

## 9. Flow 6: Hard delete template

Endpoint:

- `DELETE /api/lesson-plan-templates/{id}/hard-delete`

Role:

- `ManagementStaff`
- `Admin`

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "deletedLessonPlanCount": 3,
    "deletedLessonPlanUnitCount": 1
  }
}
```

Behavior:

- Xoa template khoi DB.
- Xoa ca `LessonPlans` thuc te dang tham chieu template do.
- Clear cac tham chieu template trong:
  - `Sessions`
  - `ClassSessionLessons`
  - `TeachingLogs`
  - `TeachingLogLessons`
  - `Classes`
  - `StudentProgresses`
  - `SessionTemplates`
- Xoa du lieu phu thuoc:
  - `LessonPlanTemplateActivities`
  - `LessonPlanTemplateMaterials`
  - `HomeworkTemplates`
- Neu unit chua template nao sau khi xoa, BE co the xoa luon unit do.

Khi nao FE nen dung:

- Chi dung khi user da xac nhan xoa manh tay.
- Nen hien confirm text ro rang vi API nay xoa ca `lesson plan` thuc te.

## 10. Flow 7: Move template vao unit / bo khoi unit

Endpoint:

- `PATCH /api/lesson-plan-templates/{id}/unit`

Role:

- `ManagementStaff`
- `Admin`

Body:

```json
{
  "lessonPlanUnitId": "44444444-4444-4444-4444-444444444444",
  "orderIndexInUnit": 2
}
```

Detach khoi unit:

```json
{
  "lessonPlanUnitId": null
}
```

Behavior:

- Neu gui `lessonPlanUnitId` moi nhung bo `orderIndexInUnit`, BE tu lay order tiep theo trong unit moi.
- Neu gui `lessonPlanUnitId = null`, BE set:
  - `lessonPlanUnitId = null`
  - `orderIndexInUnit = 0`
- Unit dich phai thuoc cung module voi template.

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "33333333-3333-3333-3333-333333333333",
    "moduleId": "22222222-2222-2222-2222-222222222222",
    "lessonPlanUnitId": "44444444-4444-4444-4444-444444444444",
    "orderIndexInUnit": 2,
    "updatedAt": "2026-06-08T12:30:00"
  }
}
```

## 11. FE integration notes

1. List page nen filter it nhat theo `syllabusId` hoac `moduleId`, vi teacher list hien tai khong scope theo class/session.
2. Search theo `title` neu muon tim gan dung thi FE phai tu loc client-side hoac can BE bo sung endpoint khac, vi API hien tai chi exact match.
3. Form update khong the clear field ve `null`. Neu can "xoa" text, gui `""`.
4. Form update khong phu hop de detach unit. Dung endpoint `PATCH /unit`.
5. Truoc soft delete, FE nen doc `usedCount` de hien warning.
6. Hard delete phai co confirm UI rieng vi no xoa ca du lieu runtime.

## 12. Suggested TS models

```ts
export type LessonPlanTemplateListItem = {
  id: string;
  syllabusId: string;
  syllabusCode: string;
  syllabusVersion: number;
  syllabusTitle: string;
  moduleId: string;
  moduleCode: string;
  moduleName: string;
  lessonPlanUnitId: string | null;
  lessonPlanUnitName: string | null;
  orderIndexInUnit: number;
  levelId: string;
  levelName: string;
  programId: string;
  programName: string;
  title: string | null;
  sessionIndex: number;
  sessionOrder: number;
  syllabusMetadata: string | null;
  syllabusContent: string | null;
  objectives: string | null;
  languageContent: string | null;
  vocabulary: string | null;
  grammar: string | null;
  teachingMethodology: string | null;
  teacherMaterials: string | null;
  studentMaterials: string | null;
  procedure: string | null;
  evaluation: string | null;
  sourceFileName: string | null;
  attachment: string | null;
  isActive: boolean;
  createdBy: string | null;
  createdByName: string | null;
  createdAt: string;
  updatedAt: string;
  usedCount: number;
};

export type Paged<T> = {
  items: T[];
  pageNumber: number | null;
  totalPages: number | null;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
};
```
