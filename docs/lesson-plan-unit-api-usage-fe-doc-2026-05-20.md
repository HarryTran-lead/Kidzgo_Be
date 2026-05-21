# Curriculum Hierarchy And Lesson Plan API Usage For FE

> Updated: 2026-05-21  
> Muc dich: chot lai model dung cho man hinh hien tai va contract FE/BE de khong group lesson bang title mot cach tam thoi.

---

## 1. Model Chuan Can Hieu

Hierarchy dung:

```text
Program
  -> Level
    -> CurriculumModule
      -> CurriculumUnit
        -> LessonPlanTemplate
```

Mapping voi ten entity hien tai trong he thong:

- `Program` = `Program`
- `Level` = `Level`
- `CurriculumModule` = entity/module hoc lon, hien tai dang la `Module`
- `CurriculumUnit` = unit group ben trong module, FE dang quen goi la Unit
- `LessonPlanTemplate` = giao an tung buoi

Vi du:

```text
Program: Kids English
Level: Starters

CurriculumModule:
- Starter01
- Starter02
- Starter03

CurriculumUnit trong Starter01:
- UNIT STARTER: HELLO
- UNIT 1: I LOVE ANIMALS
- UNIT 2: AT HOME

LessonPlanTemplate:
- UNIT STARTER: HELLO - Lesson 1 -> Buoi 1
- UNIT STARTER: HELLO - Lesson 2 -> Buoi 2
- UNIT 1: I LOVE ANIMALS - Lesson 1 -> Buoi 3
- UNIT 1: I LOVE ANIMALS - Lesson 2 -> Buoi 4
```

Quan trong:

- `Starter01`, `Starter02`, `Starter03` la module hoc lon.
- `UNIT 1`, `UNIT 2`, `UNIT 3` la unit group nam trong 1 module.
- `LessonPlanTemplate` la buoi day cu the.

---

## 2. Rule Data Quan Trong Nhat

FE khong duoc coi lesson chi co `Lesson 1`, `Lesson 2` la du.

Phai phan biet:

- `lessonIndexInUnit`: lesson may trong 1 unit
- `curriculumSessionIndex`: buoi may trong module

Vi du:

```json
{
  "moduleCode": "STARTERS_STARTER01",
  "unitCode": "UNIT_01",
  "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
  "lessonIndexInUnit": 1,
  "curriculumSessionIndex": 3
}
```

Khong duoc dung moi `lessonIndexInUnit`, vi unit nao cung co `Lesson 1`, `Lesson 2`.

FE hien thi:

- Nhan "Lesson 1", "Lesson 2" dua theo `lessonIndexInUnit`
- Nhan "Buoi 3", "Buoi 4" dua theo `curriculumSessionIndex`

---

## 3. Core DB Contract Cho Curriculum

### 3.1 Program

```ts
type Program = {
  id: string;
  code: string;
  name: string;
  status: "active" | "inactive";
  createdAt: string;
  updatedAt: string;
};
```

### 3.2 Level

```ts
type Level = {
  id: string;
  programId: string;
  code: string;
  name: string;
  orderIndex: number;
  totalSessions: number;
  status: "active" | "inactive";
  createdAt: string;
  updatedAt: string;
};
```

Vi du:

```json
{
  "code": "STARTERS",
  "name": "Starters",
  "totalSessions": 50
}
```

Luu y:

- Theo du lieu hien tai dang co `50` template cho 1 level.
- UI dang hien `50 mau - 3 module`.
- Khong duoc hard-code `72`.

### 3.3 CurriculumModule

```ts
type CurriculumModule = {
  id: string;
  levelId: string;
  code: string;
  name: string;
  orderIndex: number;
  totalSessions: number;
  status: "active" | "inactive";
  createdAt: string;
  updatedAt: string;
};
```

Vi du:

```json
[
  {
    "code": "STARTERS_STARTER01",
    "name": "Starter01",
    "orderIndex": 1,
    "totalSessions": 18
  },
  {
    "code": "STARTERS_STARTER02",
    "name": "Starter02",
    "orderIndex": 2,
    "totalSessions": 16
  },
  {
    "code": "STARTERS_STARTER03",
    "name": "Starter03",
    "orderIndex": 3,
    "totalSessions": 16
  }
]
```

### 3.4 CurriculumUnit

```ts
type CurriculumUnit = {
  id: string;
  moduleId: string;
  code: string;
  name: string;
  orderIndex: number;
  status: "active" | "inactive";
  createdAt: string;
  updatedAt: string;
};
```

Vi du:

```json
{
  "code": "UNIT_01",
  "name": "UNIT 1: I LOVE ANIMALS",
  "orderIndex": 2
}
```

### 3.5 LessonPlanTemplate

```ts
type LessonPlanTemplate = {
  id: string;
  moduleId: string;
  unitId: string;
  code: string;
  title: string;
  lessonIndexInUnit: number;
  curriculumSessionIndex: number;
  sourceFileName?: string | null;
  sourceFileUrl?: string | null;
  objectivesJson?: unknown;
  vocabularyJson?: unknown;
  grammarJson?: unknown;
  rawProcedureText?: string | null;
  rawHomeworkText?: string | null;
  status: "active" | "inactive";
  createdAt: string;
  updatedAt: string;
};
```

Vi du:

```json
{
  "moduleId": "STARTERS_STARTER01",
  "unitId": "UNIT_01",
  "code": "STARTERS_STARTER01_UNIT01_L01",
  "title": "UNIT 1: I LOVE ANIMALS - Lesson 1",
  "lessonIndexInUnit": 1,
  "curriculumSessionIndex": 3,
  "sourceFileName": "Unit 1 I love animals lesson 1 done.docx",
  "status": "active"
}
```

---

## 4. API Shape FE Nen Dung Khi Render Hierarchy

FE nen nhan du lieu theo dung module -> unit -> lesson.

Vi du response goi y:

```json
{
  "isSuccess": true,
  "data": {
    "programId": "uuid-program",
    "programName": "Kids English",
    "levelId": "uuid-level",
    "levelName": "Starters",
    "totalModules": 3,
    "totalUnits": 19,
    "totalLessonPlans": 50,
    "modules": [
      {
        "moduleId": "uuid-starter01",
        "moduleCode": "STARTERS_STARTER01",
        "moduleName": "Starter01",
        "moduleOrderIndex": 1,
        "totalSessions": 18,
        "units": [
          {
            "unitId": "uuid-unit-starter",
            "unitCode": "UNIT_STARTER",
            "unitName": "UNIT STARTER: HELLO",
            "orderIndex": 1,
            "lessons": [
              {
                "lessonPlanTemplateId": "uuid-template-1",
                "title": "UNIT STARTER: HELLO - Lesson 1",
                "lessonIndexInUnit": 1,
                "curriculumSessionIndex": 1,
                "sourceFileName": "Unit starter hello lesson 1 done.docx",
                "status": "active"
              },
              {
                "lessonPlanTemplateId": "uuid-template-2",
                "title": "UNIT STARTER: HELLO - Lesson 2",
                "lessonIndexInUnit": 2,
                "curriculumSessionIndex": 2,
                "sourceFileName": "Unit starter hello lesson 2 done.docx",
                "status": "active"
              }
            ]
          }
        ]
      }
    ]
  }
}
```

FE render rule:

- Sidebar/group: `modules`
- Trong module: render `units`
- Trong unit: render `lessons`
- Label buoi: dung `curriculumSessionIndex`

---

## 5. Rule Import Va CRUD Lesson Template

BE can dam bao:

- `LessonPlanTemplate` luon thuoc 1 `moduleId`
- `LessonPlanTemplate` luon thuoc 1 `unitId`
- `curriculumSessionIndex` unique trong cung module
- `lessonIndexInUnit` chi unique trong pham vi 1 unit

FE can hieu:

- Drag-drop lesson trong unit co the doi `lessonIndexInUnit`
- Doi vi tri buoi curriculum theo module can cap nhat `curriculumSessionIndex`
- Khong parse title de tim unit nua

Validation nghiep vu quan trong:

- Module khong duoc thieu session index neu da duoc chot `totalSessions`
- Neu module khai `totalSessions = 16` nhung chi co 14 `LessonPlanTemplate`, BE nen bao loi generate class

Error goi y:

```json
{
  "code": "MISSING_LESSON_PLAN_TEMPLATE",
  "module": "STARTERS_STARTER02",
  "missingSessionIndexes": [15, 16]
}
```

---

## 6. FE Type Goi Y

```ts
export type CurriculumModuleDto = {
  moduleId: string;
  moduleCode: string;
  moduleName: string;
  moduleOrderIndex: number;
  totalSessions: number;
  units: CurriculumUnitDto[];
};

export type CurriculumUnitDto = {
  unitId: string;
  unitCode: string;
  unitName: string;
  orderIndex: number;
  lessons: LessonPlanTemplateDto[];
};

export type LessonPlanTemplateDto = {
  lessonPlanTemplateId: string;
  moduleId: string;
  unitId: string;
  code: string;
  title: string;
  lessonIndexInUnit: number;
  curriculumSessionIndex: number;
  sourceFileName?: string | null;
  sourceFileUrl?: string | null;
  status: "active" | "inactive";
};
```

---

## 7. FE Hien Thi Va Terminology

De tranh nham:

- `Starter01` = module
- `UNIT 1: I LOVE ANIMALS` = unit
- `Lesson 1` = lesson trong unit
- `Buoi 3` = session index trong module

Goi y label UI:

- Badge module: `Starter01`
- Header unit: `UNIT 1: I LOVE ANIMALS`
- Tieu de lesson: `Lesson 1`
- Sub label: `Buoi 3`

---

## 8. Chot Cho FE

FE can follow 5 diem nay:

1. Khong group lesson bang `title` nua.
2. Dung dung hierarchy `Program -> Level -> CurriculumModule -> CurriculumUnit -> LessonPlanTemplate`.
3. Dung `curriculumSessionIndex` de hien thi buoi.
4. Dung `lessonIndexInUnit` de hien thi lesson number trong unit.
5. San sang cho logic class/session se map lesson theo `moduleId + curriculumSessionIndex`, khong phai theo title.
