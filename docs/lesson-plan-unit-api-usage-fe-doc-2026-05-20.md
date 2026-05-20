# Lesson Plan Unit API Usage For FE

> Ngay tao: 2026-05-20  
> Muc dich: FE dung Unit that trong DB thay vi parse `lessonPlanTemplate.title`.

---

## 1. Tong Quan

BE da them hierarchy moi:

```text
Program -> Level -> Module -> LessonPlanUnit -> LessonPlanTemplate
```

FE khong nen group lesson plan bang title nua. Dung cac field moi:

```ts
lessonPlanUnitId?: string | null;
lessonPlanUnitName?: string | null;
orderIndexInUnit: number;
```

Tat ca response van theo wrapper hien tai:

```json
{
  "isSuccess": true,
  "data": {}
}
```

Header:

```http
Authorization: Bearer <token>
Content-Type: application/json
```

Role can co: `Admin` hoac `ManagementStaff`.

---

## 2. Flow Khuyen Nghi Cho FE

### Flow sau khi import zip

1. Admin cau hinh import rule neu can.
2. FE goi `POST /api/syllabuses/import-archive`.
3. Lay `data.syllabusId` tu response.
4. FE goi `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.
5. Render theo `groups -> units -> lessons`.
6. Neu co `orphanLessons`, hien thi khu vuc "Chua gan unit" de admin move thu cong.

### Flow quan ly Unit trong Module

1. FE lay danh sach module theo level bang API module hien co.
2. FE goi `GET /api/modules/{moduleId}/lesson-plan-units`.
3. Tao/sua/xoa/reorder unit bang cac API ben duoi.
4. Drag-drop lesson trong unit thi goi `PATCH /api/lesson-plan-units/{unitId}/lessons/reorder`.
5. Move lesson sang unit khac thi goi `PATCH /api/lesson-plan-templates/{templateId}/unit`.

---

## 3. API: Lay Hierarchy Theo Syllabus

### `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`

Dung de render man hinh Unit -> Lesson sau khi import.

```http
GET /api/syllabuses/78448399-1933-49f1-a3fd-492a922d674f/unit-lesson-plans
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "syllabusId": "78448399-1933-49f1-a3fd-492a922d674f",
    "programId": "48eba459-7a08-4461-b1f9-acec097c6185",
    "programName": "Kids English",
    "levelId": "fab421d5-89e0-43e7-b058-ab37f9d48a87",
    "levelName": "Staters",
    "totalModules": 3,
    "totalUnits": 19,
    "totalGroups": 3,
    "totalLessonPlans": 50,
    "groups": [
      {
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "moduleCode": "STATERS_STATER01",
        "moduleName": "Stater01",
        "moduleOrder": 1,
        "unitCount": 7,
        "lessonPlanCount": 19,
        "units": [
          {
            "unitId": "uuid-unit-starter",
            "unitName": "UNIT STARTER: HELLO",
            "orderIndex": 0,
            "lessonPlanCount": 2,
            "lessons": [
              {
                "lessonPlanTemplateId": "uuid-template-1",
                "lessonPlanUnitId": "uuid-unit-starter",
                "sessionTemplateId": "uuid-session-template-1",
                "title": "UNIT STARTER: HELLO - Lesson 1",
                "lessonNumber": 1,
                "sessionIndex": 1,
                "sessionOrder": 1,
                "sessionIndexInModule": 1,
                "sessionTitle": "Hello Unit",
                "sessionTopic": "Hello",
                "sourceFileName": "Unit starter hello lesson 1 done.docx",
                "orderIndexInUnit": 0,
                "isActive": true,
                "createdAt": "2026-05-20T16:50:00Z",
                "updatedAt": "2026-05-20T16:50:00Z"
              }
            ]
          }
        ]
      }
    ],
    "orphanLessons": []
  }
}
```

FE render rule:

- Sidebar/module group: `data.groups`.
- Unit accordion: `group.units`.
- Lesson rows: `unit.lessons`.
- Neu `orphanLessons.length > 0`, hien thi section rieng de admin move vao unit.

---

## 4. API: Lay Unit Trong Module

### `GET /api/modules/{moduleId}/lesson-plan-units`

```http
GET /api/modules/a4850df1-5ce3-4f97-a63c-365d4aea5318/lesson-plan-units
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "uuid-unit-1",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "name": "UNIT STARTER: HELLO",
        "orderIndex": 0,
        "lessonCount": 2,
        "isActive": true
      },
      {
        "id": "uuid-unit-2",
        "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
        "name": "UNIT 1: I LOVE ANIMALS",
        "orderIndex": 1,
        "lessonCount": 3,
        "isActive": true
      }
    ]
  }
}
```

Dung cho:

- Dropdown chon unit khi move lesson.
- Man hinh CRUD unit trong module.
- Drag-drop reorder unit.

---

## 5. API: Tao Unit

### `POST /api/modules/{moduleId}/lesson-plan-units`

```http
POST /api/modules/a4850df1-5ce3-4f97-a63c-365d4aea5318/lesson-plan-units
```

Request:

```json
{
  "name": "UNIT 6: NEW UNIT"
}
```

Response `201`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-new-unit",
    "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
    "name": "UNIT 6: NEW UNIT",
    "orderIndex": 6,
    "isActive": true
  }
}
```

Validation:

- `name` khong duoc rong.
- Trung ten sau normalize se tra `409`.
- Vi du `UNIT 03: FOOD` va `Unit 3: Food` duoc xem la trung.

---

## 6. API: Sua Unit

### `PATCH /api/lesson-plan-units/{unitId}`

```http
PATCH /api/lesson-plan-units/uuid-unit-1
```

Request:

```json
{
  "name": "UNIT 1: I LOVE ANIMALS",
  "isActive": true
}
```

Co the gui partial:

```json
{
  "name": "UNIT 1: I LOVE ANIMALS"
}
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-unit-1",
    "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
    "name": "UNIT 1: I LOVE ANIMALS",
    "orderIndex": 1,
    "isActive": true,
    "updatedAt": "2026-05-20T16:55:00Z"
  }
}
```

---

## 7. API: Xoa Unit

### `DELETE /api/lesson-plan-units/{unitId}`

```http
DELETE /api/lesson-plan-units/uuid-unit-1
```

Response:

```json
{
  "isSuccess": true,
  "data": null
}
```

Luu y:

- BE chi cho xoa unit rong.
- Neu unit con lesson plan template, BE tra `409`.
- FE nen disable nut xoa khi `lessonCount > 0`.

Error example:

```json
{
  "isSuccess": false,
  "message": "Cannot delete unit because it still has 3 lesson plan template(s)"
}
```

---

## 8. API: Reorder Units Trong Module

### `PATCH /api/modules/{moduleId}/lesson-plan-units/reorder`

Dung khi drag-drop unit.

```http
PATCH /api/modules/a4850df1-5ce3-4f97-a63c-365d4aea5318/lesson-plan-units/reorder
```

Request:

```json
[
  {
    "id": "uuid-unit-starter",
    "orderIndex": 0
  },
  {
    "id": "uuid-unit-1",
    "orderIndex": 1
  },
  {
    "id": "uuid-unit-2",
    "orderIndex": 2
  }
]
```

Response:

```json
{
  "isSuccess": true,
  "data": null
}
```

Validation:

- Tat ca unit ID phai thuoc dung `moduleId` tren route.
- `orderIndex >= 0`.
- FE nen gui toan bo list units sau khi reorder, khong chi item bi keo.

---

## 9. API: Move Lesson Sang Unit Khac

### `PATCH /api/lesson-plan-templates/{lessonPlanTemplateId}/unit`

Dung khi:

- Move lesson tu unit A sang unit B.
- Gan orphan lesson vao unit.
- Bo lesson khoi unit.

```http
PATCH /api/lesson-plan-templates/uuid-template-1/unit
```

Request gan vao unit:

```json
{
  "lessonPlanUnitId": "uuid-target-unit",
  "orderIndexInUnit": 2
}
```

Request bo khoi unit:

```json
{
  "lessonPlanUnitId": null,
  "orderIndexInUnit": 0
}
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-template-1",
    "moduleId": "a4850df1-5ce3-4f97-a63c-365d4aea5318",
    "lessonPlanUnitId": "uuid-target-unit",
    "orderIndexInUnit": 2,
    "updatedAt": "2026-05-20T17:00:00Z"
  }
}
```

Validation:

- Target unit phai ton tai.
- Lesson va target unit phai cung `moduleId`.
- Neu khong gui `orderIndexInUnit`, BE se append vao cuoi unit.

---

## 10. API: Reorder Lessons Trong Unit

### `PATCH /api/lesson-plan-units/{unitId}/lessons/reorder`

Dung khi drag-drop lesson trong cung mot unit.

```http
PATCH /api/lesson-plan-units/uuid-unit-1/lessons/reorder
```

Request:

```json
[
  {
    "id": "uuid-template-lesson-1",
    "orderIndexInUnit": 0
  },
  {
    "id": "uuid-template-lesson-2",
    "orderIndexInUnit": 1
  },
  {
    "id": "uuid-template-lesson-3",
    "orderIndexInUnit": 2
  }
]
```

Response:

```json
{
  "isSuccess": true,
  "data": null
}
```

Validation:

- Tat ca lesson IDs phai dang thuoc `unitId`.
- `orderIndexInUnit >= 0`.
- FE nen gui toan bo lessons trong unit sau khi reorder.

---

## 11. API: Lesson Plan Template List/Detail Co Field Moi

### `GET /api/lesson-plan-templates`

Moi item co them:

```json
{
  "id": "uuid-template",
  "moduleId": "uuid-module",
  "moduleCode": "STATERS_STATER01",
  "moduleName": "Stater01",
  "lessonPlanUnitId": "uuid-unit",
  "lessonPlanUnitName": "UNIT 1: I LOVE ANIMALS",
  "orderIndexInUnit": 0,
  "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
  "sessionIndex": 3,
  "sessionOrder": 3
}
```

### `GET /api/lesson-plan-templates/{id}`

Detail cung co:

```json
{
  "lessonPlanUnitId": "uuid-unit",
  "lessonPlanUnitName": "UNIT 1: I LOVE ANIMALS",
  "orderIndexInUnit": 0
}
```

---

## 12. FE Type Goi Y

```ts
export type LessonPlanUnit = {
  id: string;
  moduleId: string;
  name: string;
  orderIndex: number;
  lessonCount: number;
  isActive: boolean;
};

export type LessonPlanTemplateInUnit = {
  lessonPlanTemplateId: string;
  lessonPlanUnitId: string | null;
  sessionTemplateId: string | null;
  title: string | null;
  lessonNumber: number | null;
  sessionIndex: number;
  sessionOrder: number;
  sessionIndexInModule: number | null;
  sessionTitle: string | null;
  sessionTopic: string | null;
  sourceFileName: string | null;
  orderIndexInUnit: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type LessonPlanUnitGroup = {
  unitId: string;
  unitName: string;
  orderIndex: number;
  lessonPlanCount: number;
  lessons: LessonPlanTemplateInUnit[];
};

export type ModuleUnitLessonPlanGroup = {
  moduleId: string;
  moduleCode: string;
  moduleName: string;
  moduleOrder: number;
  unitCount: number;
  lessonPlanCount: number;
  units: LessonPlanUnitGroup[];
};
```

---

## 13. Luu Y Migration/Import

Sau khi BE deploy migration:

```powershell
dotnet ef database update --project Kidzgo.Infrastructure --startup-project Kidzgo.API
```

Voi data cu:

- Import lai zip voi `overwriteExisting=true` de BE gan `LessonPlanUnitId`.
- Hoac lam backfill rieng neu muon giu data cu ma khong import lai.

Neu FE thay `orphanLessons` co data:

- Do lesson chua match duoc unit.
- Cho admin chon unit va goi `PATCH /api/lesson-plan-templates/{id}/unit`.

---

## 14. CRUD LessonPlan Va LessonPlanTemplate

Section nay bo sung cac API trong Swagger group `LessonPlan` va `LessonPlanTemplate`.

Role:

- `LessonPlan`: `Teacher`, `ManagementStaff`, `Admin`
- `LessonPlanTemplate`: `ManagementStaff`, `Admin`

Tat ca request can header:

```http
Authorization: Bearer <token>
Content-Type: application/json
```

### 14.1 `POST /api/lesson-plans`

Tao lesson plan thuc te cho 1 buoi hoc cua lop.

```http
POST /api/lesson-plans
```

Request:

```json
{
  "classId": "uuid-class",
  "sessionId": "uuid-session",
  "templateId": "uuid-template-or-null",
  "plannedContent": "Noi dung du kien",
  "actualContent": "Noi dung da day",
  "actualHomework": "Bai tap ve nha",
  "teacherNotes": "Ghi chu giao vien",
  "completionPercent": 80,
  "carryForwardContent": "Noi dung can chuyen sang buoi sau"
}
```

Response `201`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-lesson-plan",
    "classId": "uuid-class",
    "sessionId": "uuid-session",
    "templateId": "uuid-template",
    "plannedContent": "Noi dung du kien",
    "actualContent": "Noi dung da day",
    "actualHomework": "Bai tap ve nha",
    "teacherNotes": "Ghi chu giao vien",
    "completionPercent": 80,
    "carryForwardContent": "Noi dung can chuyen sang buoi sau",
    "submittedBy": "uuid-user",
    "submittedAt": "2026-05-21T10:00:00Z",
    "createdAt": "2026-05-21T10:00:00Z"
  }
}
```

Luu y:

- `templateId` co the null neu buoi hoc chua gan template.
- `completionPercent` duoc BE clamp trong khoang `0..100`.

### 14.2 `GET /api/lesson-plans/{id}`

Lay chi tiet lesson plan.

```http
GET /api/lesson-plans/uuid-lesson-plan
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-lesson-plan",
    "classId": "uuid-class",
    "classCode": "S1",
    "sessionId": "uuid-session",
    "sessionTitle": "Buoi 1",
    "sessionDate": "2026-05-21T00:00:00Z",
    "templateId": "uuid-template",
    "templateLevel": "Starters",
    "templateSessionIndex": 1,
    "plannedContent": "Noi dung du kien",
    "actualContent": "Noi dung da day",
    "actualHomework": "Bai tap ve nha",
    "teacherNotes": "Ghi chu giao vien",
    "completionPercent": 80,
    "carryForwardContent": "Noi dung can chuyen sang buoi sau",
    "submittedBy": "uuid-user",
    "submittedByName": "Teacher Name",
    "submittedAt": "2026-05-21T10:00:00Z",
    "createdAt": "2026-05-21T10:00:00Z"
  }
}
```

### 14.3 `PUT /api/lesson-plans/{id}`

Cap nhat lesson plan. BE chi update field nao FE gui khac `null`.

```http
PUT /api/lesson-plans/uuid-lesson-plan
```

Request:

```json
{
  "templateId": "uuid-template",
  "plannedContent": "Noi dung du kien moi",
  "actualContent": "Noi dung da day moi",
  "actualHomework": "Bai tap moi",
  "teacherNotes": "Ghi chu moi",
  "completionPercent": 100,
  "carryForwardContent": ""
}
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-lesson-plan",
    "sessionId": "uuid-session",
    "templateId": "uuid-template",
    "plannedContent": "Noi dung du kien moi",
    "actualContent": "Noi dung da day moi",
    "actualHomework": "Bai tap moi",
    "teacherNotes": "Ghi chu moi",
    "completionPercent": 100,
    "carryForwardContent": ""
  }
}
```

Luu y:

- Muon clear text field thi gui chuoi rong `""`.
- Gui `null` se khong doi gia tri hien tai.

### 14.4 `GET /api/lesson-plans/classes/{classId}/syllabus`

API nay dung cho man hinh lesson plan theo lop: FE lay danh sach sessions, template dang gan, noi dung lesson plan va quyen edit.

```http
GET /api/lesson-plans/classes/uuid-class/syllabus
```

Response rut gon:

```json
{
  "isSuccess": true,
  "data": {
    "classId": "uuid-class",
    "classCode": "S1",
    "classTitle": "Starters 1",
    "programId": "uuid-program",
    "programName": "Kids English",
    "syllabusMetadata": "Starters syllabus metadata",
    "sessions": [
      {
        "sessionId": "uuid-session",
        "sessionIndex": 1,
        "moduleId": "uuid-module",
        "sessionIndexInModule": 1,
        "sessionDate": "2026-05-21T00:00:00Z",
        "plannedTeacherId": "uuid-teacher",
        "plannedTeacherName": "Teacher Name",
        "actualTeacherId": null,
        "actualTeacherName": null,
        "lessonPlanId": "uuid-lesson-plan",
        "templateId": "uuid-template",
        "templateTitle": "UNIT STARTER: HELLO! - Lesson 1",
        "templateSyllabusContent": "Template content",
        "plannedContent": "Noi dung du kien",
        "actualContent": "Noi dung da day",
        "actualHomework": "Bai tap",
        "teacherNotes": "Ghi chu",
        "canEdit": true
      }
    ]
  }
}
```

FE nen dung `canEdit` de bat/tat nut sua lesson plan theo tung session.

---

## 15. CRUD LessonPlanTemplate

`LessonPlanTemplate` la template giao an theo module/unit/session. Import zip/import word se tao cac template nay.

### 15.1 `POST /api/lesson-plan-templates`

Tao template thu cong.

```http
POST /api/lesson-plan-templates
```

Request:

```json
{
  "moduleId": "uuid-module",
  "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "UNIT 1: I LOVE ANIMALS",
  "syllabusContent": "Noi dung giao an",
  "objectives": "Muc tieu",
  "languageContent": "Language content",
  "vocabulary": "cat, dog",
  "grammar": "This is...",
  "teachingMethodology": "PPP",
  "teacherMaterials": "Teacher book",
  "studentMaterials": "Student book",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observation",
  "sourceFileName": "Unit 1 lesson 1.docx",
  "attachment": "https://cdn.example.com/file.docx"
}
```

Response `201`:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-template",
    "moduleId": "uuid-module",
    "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
    "sessionIndex": 1,
    "sessionOrder": 1,
    "syllabusMetadata": "UNIT 1: I LOVE ANIMALS",
    "syllabusContent": "Noi dung giao an",
    "objectives": "Muc tieu",
    "languageContent": "Language content",
    "vocabulary": "cat, dog",
    "grammar": "This is...",
    "teachingMethodology": "PPP",
    "teacherMaterials": "Teacher book",
    "studentMaterials": "Student book",
    "procedure": "Warm-up, practice, production",
    "evaluation": "Observation",
    "sourceFileName": "Unit 1 lesson 1.docx",
    "attachment": "https://cdn.example.com/file.docx",
    "isActive": true,
    "createdAt": "2026-05-21T10:00:00Z",
    "updatedAt": "2026-05-21T10:00:00Z"
  }
}
```

Luu y:

- API tao thu cong hien chua nhan `lessonPlanUnitId`.
- Sau khi tao, neu can gan unit thi goi `PATCH /api/lesson-plan-templates/{id}/unit`.

### 15.2 `GET /api/lesson-plan-templates`

Lay danh sach template co phan trang.

```http
GET /api/lesson-plan-templates?moduleId=uuid-module&isActive=true&pageNumber=1&pageSize=20
```

Query:

| Param | Bat buoc | Mo ta |
|---|---:|---|
| `moduleId` | No | Loc theo module |
| `title` | No | Tim theo title |
| `isActive` | No | Loc active/inactive |
| `includeDeleted` | No | Mac dinh `false` |
| `pageNumber` | No | Mac dinh `1` |
| `pageSize` | No | Mac dinh `10` |

Response:

```json
{
  "isSuccess": true,
  "data": {
    "templates": {
      "items": [
        {
          "id": "uuid-template",
          "moduleId": "uuid-module",
          "moduleName": "Stater01",
          "levelId": "uuid-level",
          "levelName": "Starters",
          "programId": "uuid-program",
          "programName": "Kids English",
          "lessonPlanUnitId": "uuid-unit",
          "lessonPlanUnitName": "UNIT 1: I LOVE ANIMALS",
          "orderIndexInUnit": 0,
          "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
          "sessionIndex": 1,
          "sessionOrder": 1,
          "syllabusMetadata": "UNIT 1: I LOVE ANIMALS",
          "syllabusContent": "Noi dung giao an",
          "objectives": "Muc tieu",
          "languageContent": "Language content",
          "vocabulary": "cat, dog",
          "grammar": "This is...",
          "teachingMethodology": "PPP",
          "teacherMaterials": "Teacher book",
          "studentMaterials": "Student book",
          "procedure": "Warm-up, practice, production",
          "evaluation": "Observation",
          "sourceFileName": "Unit 1 lesson 1.docx",
          "attachment": "https://cdn.example.com/file.docx",
          "isActive": true,
          "createdBy": "uuid-user",
          "createdByName": "Admin",
          "createdAt": "2026-05-21T10:00:00Z",
          "updatedAt": "2026-05-21T10:00:00Z",
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

### 15.3 `GET /api/lesson-plan-templates/{id}`

Lay chi tiet 1 template.

```http
GET /api/lesson-plan-templates/uuid-template
```

Response co cung field voi item trong API list, bao gom:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-template",
    "moduleId": "uuid-module",
    "lessonPlanUnitId": "uuid-unit",
    "lessonPlanUnitName": "UNIT 1: I LOVE ANIMALS",
    "orderIndexInUnit": 0,
    "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
    "sessionIndex": 1,
    "sessionOrder": 1,
    "isActive": true,
    "usedCount": 0
  }
}
```

### 15.4 `PUT /api/lesson-plan-templates/{id}`

Cap nhat template. BE chi update field nao FE gui khac `null`.

```http
PUT /api/lesson-plan-templates/uuid-template
```

Request:

```json
{
  "moduleId": "uuid-module",
  "title": "UNIT 1: I LOVE ANIMALS - Lesson 2",
  "sessionIndex": 2,
  "sessionOrder": 2,
  "syllabusMetadata": "UNIT 1: I LOVE ANIMALS",
  "syllabusContent": "Noi dung moi",
  "objectives": "Muc tieu moi",
  "languageContent": "Language content moi",
  "vocabulary": "cat, dog, bird",
  "grammar": "These are...",
  "teachingMethodology": "PPP",
  "teacherMaterials": "Teacher book",
  "studentMaterials": "Student book",
  "procedure": "Warm-up, practice, production",
  "evaluation": "Observation",
  "sourceFileName": "Unit 1 lesson 2.docx",
  "attachment": "https://cdn.example.com/file.docx",
  "isActive": true
}
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-template",
    "moduleId": "uuid-module",
    "title": "UNIT 1: I LOVE ANIMALS - Lesson 2",
    "sessionIndex": 2,
    "sessionOrder": 2,
    "isActive": true,
    "updatedAt": "2026-05-21T10:10:00Z"
  }
}
```

Luu y:

- `sessionIndex` phai nam trong `module.plannedSessionCount`.
- Khong duoc trung `sessionIndex` trong cung module voi template khac dang active/not deleted.
- API nay khong move unit. Muon move unit dung API ben duoi.

### 15.5 `PATCH /api/lesson-plan-templates/{id}/unit`

Gan template vao unit, chuyen unit, reorder trong unit, hoac bo khoi unit.

```http
PATCH /api/lesson-plan-templates/uuid-template/unit
```

Request:

```json
{
  "lessonPlanUnitId": "uuid-unit-or-null",
  "orderIndexInUnit": 2
}
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-template",
    "moduleId": "uuid-module",
    "lessonPlanUnitId": "uuid-unit",
    "orderIndexInUnit": 2,
    "updatedAt": "2026-05-21T10:10:00Z"
  }
}
```

Luu y:

- Gui `"lessonPlanUnitId": null` de dua template ve orphan/chua gan unit.
- `lessonPlanUnitId` phai thuoc cung module voi template.

### 15.6 `POST /api/lesson-plan-templates/import`

Import template tu file dang bang du lieu, dung cho luong import cu.

```http
POST /api/lesson-plan-templates/import?moduleId=uuid-module&overwriteExisting=true
Content-Type: multipart/form-data
```

Form-data:

| Field | Type | Bat buoc | Mo ta |
|---|---|---:|---|
| `file` | file | Yes | File import |

Query:

| Param | Bat buoc | Mo ta |
|---|---:|---|
| `moduleId` | No | Module dich neu file khong tu xac dinh duoc module |
| `overwriteExisting` | No | Mac dinh `true` |

### 15.7 `POST /api/lesson-plan-templates/import-word`

Import 1 file Word lesson plan vao 1 module. API nay co the gan `LessonPlanUnitId` tu dong neu title/source file match unit.

```http
POST /api/lesson-plan-templates/import-word?moduleId=uuid-module&overwriteExisting=true
Content-Type: multipart/form-data
```

Form-data:

| Field | Type | Bat buoc | Mo ta |
|---|---|---:|---|
| `file` | file | Yes | File `.docx` lesson plan |

Response:

```json
{
  "isSuccess": true,
  "data": {
    "lessonPlanTemplateId": "uuid-template",
    "sessionTemplateId": "uuid-session-template",
    "sessionIndex": 1,
    "created": true,
    "title": "UNIT STARTER: HELLO! - Lesson 1"
  }
}
```

Sau khi import word thanh cong:

1. FE goi `GET /api/lesson-plan-templates/{lessonPlanTemplateId}` neu can xem `lessonPlanUnitId`.
2. FE goi `GET /api/syllabuses/{syllabusId}/unit-lesson-plans` neu can refresh hierarchy.

---

## 16. FE Flow Goi Y Cho Man Lesson Plan

### Quan ly template theo unit

1. Goi `GET /api/syllabuses/{syllabusId}/unit-lesson-plans` de render hierarchy.
2. Tao template thu cong bang `POST /api/lesson-plan-templates`.
3. Gan template vao unit bang `PATCH /api/lesson-plan-templates/{id}/unit`.
4. Sua noi dung template bang `PUT /api/lesson-plan-templates/{id}`.
5. Reorder lesson trong unit bang `PATCH /api/lesson-plan-units/{unitId}/lessons/reorder`.
6. Doi so buoi co dinh theo level bang `PATCH /api/levels/{levelId}/lesson-plan-templates/session-orders`.

### Giao vien cap nhat lesson plan cua lop

1. Goi `GET /api/lesson-plans/classes/{classId}/syllabus`.
2. Neu session chua co `lessonPlanId`, goi `POST /api/lesson-plans`.
3. Neu session da co `lessonPlanId`, goi `PUT /api/lesson-plans/{id}`.
4. FE chi cho sua khi `canEdit = true`.

---

## 17. FE Types Goi Y Cho CRUD

```ts
export type LessonPlanCreateRequest = {
  classId: string;
  sessionId: string;
  templateId?: string | null;
  plannedContent?: string | null;
  actualContent?: string | null;
  actualHomework?: string | null;
  teacherNotes?: string | null;
  completionPercent?: number | null;
  carryForwardContent?: string | null;
};

export type LessonPlanUpdateRequest = Partial<
  Pick<
    LessonPlanCreateRequest,
    | "templateId"
    | "plannedContent"
    | "actualContent"
    | "actualHomework"
    | "teacherNotes"
    | "completionPercent"
    | "carryForwardContent"
  >
>;

export type LessonPlanTemplateCreateRequest = {
  moduleId: string;
  title: string;
  sessionIndex: number;
  sessionOrder?: number | null;
  syllabusMetadata?: string | null;
  syllabusContent?: string | null;
  objectives?: string | null;
  languageContent?: string | null;
  vocabulary?: string | null;
  grammar?: string | null;
  teachingMethodology?: string | null;
  teacherMaterials?: string | null;
  studentMaterials?: string | null;
  procedure?: string | null;
  evaluation?: string | null;
  sourceFileName?: string | null;
  attachment?: string | null;
};

export type LessonPlanTemplateUpdateRequest =
  Partial<LessonPlanTemplateCreateRequest> & {
    isActive?: boolean | null;
  };

export type MoveLessonPlanTemplateUnitRequest = {
  lessonPlanUnitId: string | null;
  orderIndexInUnit?: number | null;
};
```

---

## 18. API Moi Bo Sung Cho CRUD/Import/Reorder

### 18.1 `DELETE /api/lesson-plan-templates/{id}`

Soft-delete lesson plan template. BE se set `isDeleted = true`, `isActive = false`.

```http
DELETE /api/lesson-plan-templates/uuid-template
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-template",
    "isDeleted": true,
    "updatedAt": "2026-05-21T10:30:00Z"
  }
}
```

Luu y:

- Neu template dang duoc lesson plan thuc te su dung, BE tra `409`.
- Khi delete thanh cong, BE cung go bo link `SessionTemplates.LessonPlanTemplateId`.

### 18.2 Import Word vao Unit cu the

Co 2 cach goi.

#### Cach 1: Dung endpoint template cu, them `lessonPlanUnitId`

```http
POST /api/lesson-plan-templates/import-word?moduleId=uuid-module&lessonPlanUnitId=uuid-unit&sessionIndexOverride=7&overwriteExisting=true
Content-Type: multipart/form-data
```

Form-data:

| Field | Type | Bat buoc | Mo ta |
|---|---|---:|---|
| `file` | file | Yes | File `.docx` lesson plan |

Query:

| Param | Bat buoc | Mo ta |
|---|---:|---|
| `moduleId` | Yes | Module chua unit |
| `lessonPlanUnitId` | No | Neu co, template se duoc gan thang vao unit nay |
| `sessionIndexOverride` | No | So buoi co dinh neu FE muon chi dinh |
| `overwriteExisting` | No | Mac dinh `true` |

#### Cach 2: Goi truc tiep theo unit

```http
POST /api/lesson-plan-units/uuid-unit/lesson-plan-templates/import-word?sessionIndexOverride=7&overwriteExisting=true
Content-Type: multipart/form-data
```

Form-data:

| Field | Type | Bat buoc | Mo ta |
|---|---|---:|---|
| `file` | file | Yes | File `.docx` lesson plan |

Response:

```json
{
  "isSuccess": true,
  "data": {
    "lessonPlanTemplateId": "uuid-template",
    "lessonPlanUnitId": "uuid-unit",
    "sessionTemplateId": "uuid-session-template-or-null",
    "sessionIndex": 7,
    "sessionOrder": 7,
    "orderIndexInUnit": 1,
    "created": true,
    "title": "UNIT STARTER: HELLO! - Lesson 2"
  }
}
```

Behavior:

- Neu co `lessonPlanUnitId`, BE validate unit phai active va thuoc dung module.
- Neu khong gui `sessionIndexOverride` khi import theo unit, BE tu lay session index con trong tiep theo cua module.
- `orderIndexInUnit` uu tien lay theo lesson number trong file/title, fallback la next order trong unit.

### 18.3 Reorder so buoi co dinh theo Level

API nay dung cho UI drag-drop toan bo template trong 1 level, de cap nhat cot `sessionOrder`. FE nen hien thi "Buoi" bang `sessionOrder`, khong nen dung `sessionIndex` neu can thu tu co dinh tren toan level.

```http
PATCH /api/levels/uuid-level/lesson-plan-templates/session-orders
Content-Type: application/json
```

Request:

```json
[
  {
    "id": "uuid-template-1",
    "sessionOrder": 1
  },
  {
    "id": "uuid-template-2",
    "sessionOrder": 2
  },
  {
    "id": "uuid-template-3",
    "sessionOrder": 3
  }
]
```

Response:

```json
{
  "isSuccess": true,
  "data": {
    "levelId": "uuid-level",
    "items": [
      {
        "id": "uuid-template-1",
        "moduleId": "uuid-module",
        "sessionIndex": 1,
        "sessionOrder": 1,
        "updatedAt": "2026-05-21T10:40:00Z"
      }
    ]
  }
}
```

Validation:

- Template phai thuoc level trong path.
- `sessionOrder` phai >= 1.
- `sessionOrder` khong duoc trung nhau trong request.
- Neu level co tong planned sessions > 0, `sessionOrder` khong duoc vuot tong planned sessions cua level.

FE flow khuyen nghi:

1. Goi `GET /api/syllabuses/{syllabusId}/unit-lesson-plans`.
2. Flatten `groups -> units -> lessons`.
3. Sau drag-drop, tao list `{ id: lesson.lessonPlanTemplateId, sessionOrder: index + 1 }`.
4. Goi `PATCH /api/levels/{levelId}/lesson-plan-templates/session-orders`.
5. Refresh lai hierarchy.

Type:

```ts
export type ReorderLessonPlanTemplateSessionOrderRequest = {
  id: string;
  sessionOrder: number;
};
```
