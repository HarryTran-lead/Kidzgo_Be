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

