# FE API Doc - Create Class With Curriculum Start Point

> Updated: 2026-05-21  
> Muc dich: chot lai contract tao lop theo `startModuleId + startSessionIndex`, generate `ClassSession` tu curriculum, va tach ro curriculum progress voi billing progress.

---

## 1. Core Nghiep Vu

Class khong chon tung lesson thu cong.

Class duoc tao bang:

- `programId`
- `levelId`
- `startModuleId`
- `startSessionIndex`
- `schedule`
- `startDate`
- `sessionsToGenerate`

BE tu dong:

1. Tao `Class`
2. Tao `ClassSchedule`
3. Tao `ClassModuleProgress`
4. Generate `ClassSession`
5. Gan `LessonPlanTemplate` dua theo `moduleId + curriculumSessionIndex`
6. Tinh `expectedEndDate` tu session cuoi cung duoc generate

---

## 2. Class Data Model FE Can Hieu

```ts
type Class = {
  id: string;
  code: string;
  name: string;
  programId: string;
  levelId: string;
  startModuleId: string;
  startSessionIndex: number;
  currentModuleId: string;
  currentSessionIndex: number;
  currentLessonPlanTemplateId?: string | null;
  startDate: string;
  expectedEndDate?: string | null;
  actualEndDate?: string | null;
  status: "planned" | "active" | "completed" | "cancelled";
  createdAt: string;
  updatedAt: string;
};
```

Y nghia:

- `startModuleId`: lop bat dau tu module nao
- `startSessionIndex`: bat dau tu buoi may trong module do
- `currentModuleId`: module hien tai cua lop
- `currentSessionIndex`: buoi/lesson tiep theo can day trong module hien tai

Vi du tao lop bat dau tu Starter02 buoi 1:

```json
{
  "programId": "KIDS_ENGLISH",
  "levelId": "STARTERS",
  "startModuleId": "STARTERS_STARTER02",
  "startSessionIndex": 1,
  "currentModuleId": "STARTERS_STARTER02",
  "currentSessionIndex": 1
}
```

Vi du bat dau giua module:

```json
{
  "startModuleId": "STARTERS_STARTER02",
  "startSessionIndex": 7,
  "currentModuleId": "STARTERS_STARTER02",
  "currentSessionIndex": 7
}
```

---

## 3. ClassModuleProgress Model

Khuyen dung model sau:

```ts
type ClassModuleProgress = {
  id: string;
  classId: string;
  moduleId: string;
  orderIndex: number;
  requiredSessions: number;
  completedClassSessions: number;
  completedLessonPlans: number;
  startSessionIndex: number;
  currentSessionIndex: number;
  status: "skipped" | "pending" | "active" | "completed";
  startedAt?: string | null;
  completedAt?: string | null;
  createdAt: string;
  updatedAt: string;
};
```

Phan biet:

- `completedClassSessions` = so buoi hoc thuc te da dien ra
- `completedLessonPlans` = so lesson da consume

Khong dung 1 field chung vi:

- Review van la buoi hoc that
- Nhung review co the khong consume lesson plan

Vi du module co 16 lesson, lop hoc 16 buoi nhung co 2 buoi review:

```json
{
  "completedClassSessions": 16,
  "completedLessonPlans": 14
}
```

Rule:

- Auto chuyen module dua tren `completedLessonPlans >= requiredSessions`
- Khong dua tren `completedClassSessions`

---

## 4. ClassSchedule Va ClassSession

### 4.1 ClassSchedule

```ts
type ClassSchedule = {
  id: string;
  classId: string;
  daysOfWeek: number[];
  startTime: string;
  endTime: string;
  effectiveFrom: string;
  effectiveTo?: string | null;
  status: "active" | "inactive";
};
```

### 4.2 ClassSession

```ts
type ClassSession = {
  id: string;
  classId: string;
  classSessionNo: number;
  moduleId: string;
  unitId?: string | null;
  lessonPlanTemplateId?: string | null;
  plannedCurriculumSessionIndex: number;
  sessionDate: string;
  startTime: string;
  endTime: string;
  status: "scheduled" | "completed" | "cancelled" | "postponed" | "makeup";
  teachingLogId?: string | null;
  createdAt: string;
  updatedAt: string;
};
```

Vi du lop bat dau tu Starter02:

```json
[
  {
    "classSessionNo": 1,
    "moduleId": "STARTERS_STARTER02",
    "unitId": "UNIT_06",
    "lessonPlanTemplateId": "UNIT06_L01",
    "plannedCurriculumSessionIndex": 1,
    "status": "scheduled"
  },
  {
    "classSessionNo": 2,
    "moduleId": "STARTERS_STARTER02",
    "unitId": "UNIT_06",
    "lessonPlanTemplateId": "UNIT06_L02",
    "plannedCurriculumSessionIndex": 2,
    "status": "scheduled"
  }
]
```

---

## 5. Create Class API

### `POST /api/classes`

Request:

```json
{
  "code": "CLS_STARTERS_02_EVENING",
  "name": "Starters 02 - Toi 2-4-6",
  "programId": "KIDS_ENGLISH",
  "levelId": "STARTERS",
  "startModuleId": "STARTERS_STARTER02",
  "startSessionIndex": 1,
  "schedule": {
    "daysOfWeek": [2, 4, 6],
    "startTime": "18:00",
    "endTime": "19:30"
  },
  "startDate": "2026-06-01",
  "sessionsToGenerate": 24,
  "skipHolidays": true
}
```

Validation BE phai co:

1. `programId` ton tai
2. `levelId` thuoc `programId`
3. `startModuleId` thuoc `levelId`
4. `startSessionIndex` nam trong `1..module.totalSessions`
5. Module co du `LessonPlanTemplate` cho cac `curriculumSessionIndex` can dung

Neu tao thanh cong, response nen bao gom:

```json
{
  "isSuccess": true,
  "data": {
    "id": "uuid-class",
    "code": "CLS_STARTERS_02_EVENING",
    "name": "Starters 02 - Toi 2-4-6",
    "programId": "uuid-program",
    "levelId": "uuid-level",
    "startModuleId": "uuid-starter02",
    "startSessionIndex": 1,
    "currentModuleId": "uuid-starter02",
    "currentSessionIndex": 1,
    "expectedEndDate": "2026-07-24",
    "status": "active"
  }
}
```

---

## 6. Preview Sessions Truoc Khi Tao

Rat nen co endpoint:

### `POST /api/classes/preview-sessions`

Input giong create class.

Response goi y:

```json
{
  "isSuccess": true,
  "data": {
    "expectedEndDate": "2026-07-24",
    "sessions": [
      {
        "classSessionNo": 1,
        "date": "2026-06-01",
        "moduleName": "Starter02",
        "unitName": "UNIT 6: LOOK AT US",
        "lessonTitle": "UNIT 06: LOOK AT US - Lesson 1",
        "curriculumSessionIndex": 1
      }
    ],
    "warnings": []
  }
}
```

Dung cho FE:

- Preview lich truoc khi submit that
- Bao cho user neu se chay sang module tiep theo
- Bao warning neu khong du curriculum session

---

## 7. Rule Generate ClassSession

Input:

- `startModuleId`
- `startSessionIndex`
- `sessionsToGenerate`
- `daysOfWeek`
- `startDate`
- holiday calendar

Algorithm:

```text
cursorModule = startModuleId
cursorSessionIndex = startSessionIndex
classSessionNo = 1

while classSessionNo <= sessionsToGenerate:
  lesson = find LessonPlanTemplate where:
    moduleId = cursorModule
    curriculumSessionIndex = cursorSessionIndex

  create ClassSession:
    classSessionNo
    moduleId = cursorModule
    unitId = lesson.unitId
    lessonPlanTemplateId = lesson.id
    plannedCurriculumSessionIndex = cursorSessionIndex
    sessionDate = next valid schedule date

  cursorSessionIndex += 1

  if cursorSessionIndex > currentModule.totalSessions:
    cursorModule = next module by orderIndex
    cursorSessionIndex = 1

  classSessionNo += 1
```

Rule neu generate vuot curriculum:

- Khuyen dung stop va tra error
- Khong khuyen generate session rong

Error goi y:

```json
{
  "code": "NOT_ENOUGH_CURRICULUM_SESSIONS",
  "message": "Only 16 lessons available from Starter03, cannot generate 24 sessions."
}
```

---

## 8. Cac Case Tao Lop FE Can Hieu

### 8.1 Bat dau tu Starter01

Input:

```json
{
  "startModuleId": "STARTERS_STARTER01",
  "startSessionIndex": 1,
  "sessionsToGenerate": 24
}
```

Generate:

- Session 1 -> Starter01 / Buoi 1 / UNIT STARTER Lesson 1
- Session 2 -> Starter01 / Buoi 2 / UNIT STARTER Lesson 2
- Session 3 -> Starter01 / Buoi 3 / UNIT 1 Lesson 1
- ...
- Session 18 -> Starter01 / Buoi 18
- Session 19 -> Starter02 / Buoi 1
- Session 24 -> Starter02 / Buoi 6

### 8.2 Bat dau tu Starter02

Input:

```json
{
  "startModuleId": "STARTERS_STARTER02",
  "startSessionIndex": 1,
  "sessionsToGenerate": 16
}
```

Generate:

- Session 1-16 -> Starter02

Neu `sessionsToGenerate = 24`:

- Session 1-16 -> Starter02
- Session 17-24 -> Starter03 / Buoi 1-8

### 8.3 Bat dau giua Starter02

Input:

```json
{
  "startModuleId": "STARTERS_STARTER02",
  "startSessionIndex": 7,
  "sessionsToGenerate": 10
}
```

Generate:

- Session 1 -> Starter02 / Buoi 7
- Session 2 -> Starter02 / Buoi 8
- ...
- Session 10 -> Starter02 / Buoi 16

Khong generate buoi 1-6.

---

## 9. ClassModuleProgress Khi Moi Tao Lop

Neu lop bat dau tu Starter02:

```json
[
  {
    "module": "Starter01",
    "status": "skipped",
    "completedClassSessions": 0,
    "completedLessonPlans": 0
  },
  {
    "module": "Starter02",
    "status": "active",
    "startSessionIndex": 1,
    "currentSessionIndex": 1
  },
  {
    "module": "Starter03",
    "status": "pending"
  }
]
```

Neu bat dau Starter02 buoi 7:

```json
[
  {
    "module": "Starter01",
    "status": "skipped"
  },
  {
    "module": "Starter02",
    "status": "active",
    "startSessionIndex": 7,
    "currentSessionIndex": 7,
    "completedClassSessions": 0,
    "completedLessonPlans": 0
  },
  {
    "module": "Starter03",
    "status": "pending"
  }
]
```

Quan trong:

- Lop moi bat dau tu buoi 7 khong co nghia la da hoc xong 6 buoi dau.
- Khong auto set `completedLessonPlans = 6`.

---

## 10. TeachingLog Va Runtime Consume Lesson

Runtime logic khac create-class logic.

Ban dau generate:

- Buoi 1 -> Lesson 1
- Buoi 2 -> Lesson 2
- Buoi 3 -> Lesson 3

Neu buoi 2 la review:

```json
{
  "classSessionId": "S2",
  "actualTeachingType": "review",
  "plannedLessonPlanId": "LESSON_2",
  "progressStatus": "not_started"
}
```

Khi do:

- Lesson 2 chua duoc consume
- Buoi sau van phai day lai Lesson 2

Khuyen dung huong hybrid:

- `ClassSession` van giu `plannedLessonPlanTemplateId` ban dau
- `TeachingLog` quyet dinh lesson co consume hay khong
- BE co the tra `suggestedNextLessonPlanId` hoac co endpoint resync

---

## 11. TeachingLog Model Goi Y

```ts
type TeachingLog = {
  id: string;
  classSessionId: string;
  plannedLessonPlanId?: string | null;
  actualLessonPlanId?: string | null;
  actualTeachingType: "normal" | "review" | "test" | "makeup" | "event" | "other";
  progressStatus: "completed" | "partial" | "not_started" | "skipped";
  actualContent?: string | null;
  actualHomework?: string | null;
  teacherNote?: string | null;
  submittedBy: string;
  submittedAt: string;
  approvedBy?: string | null;
  approvedAt?: string | null;
};
```

Endpoint can co:

### `POST /api/class-sessions/{id}/teaching-log`

---

## 12. Rule Consume Lesson

Khi submit `TeachingLog`:

- `completed`
  - Lesson duoc tinh la da day
  - `currentSessionIndex += 1`
- `partial`
  - Lesson chua xong
  - `currentSessionIndex` giu nguyen
- `not_started`
  - Lesson chua bat dau
  - `currentSessionIndex` giu nguyen
- `skipped`
  - Bo qua lesson
  - `currentSessionIndex += 1`
  - Bat buoc nhap ly do

Rule khac:

- `cancelled` session khong tang `completedClassSessions`, khong tang `completedLessonPlans`
- `postponed` khong tinh completed
- `makeup` co the consume hoac khong consume, tuy `TeachingLog`

---

## 13. Rule Auto Chuyen Module

Khi lesson duoc consume:

1. `completedLessonPlans += 1`
2. `currentSessionIndex += 1`

Neu:

```text
currentSessionIndex > module.totalSessions
```

BE phai:

1. set current module = `completed`
2. set next module = `active`
3. update `class.currentModuleId = nextModuleId`
4. update `class.currentSessionIndex = 1`

Neu la module cuoi:

- `class.status = completed`

---

## 14. Billing Progress Khac Curriculum Progress

Billing/session balance khong duoc dong nhat voi lesson consumed.

Vi du buoi review:

- `ClassSession.status = completed`
- `TeachingLog.actualTeachingType = review`
- `progressStatus = not_started`

Khi do:

- Co the van tru `remainingSessions` neu hoc vien co hoc that
- Nhung curriculum van chua consume lesson

Nho:

- `Billing progress != Curriculum progress`

---

## 15. Error Cases FE Phai Handle

### Case 1: Start module khong thuoc level

```json
{
  "code": "MODULE_NOT_IN_LEVEL"
}
```

### Case 2: `startSessionIndex` vuot module

```json
{
  "code": "INVALID_START_SESSION_INDEX"
}
```

### Case 3: Module thieu lesson plan template

```json
{
  "code": "MISSING_LESSON_PLAN_TEMPLATE",
  "module": "STARTERS_STARTER02",
  "missingSessionIndexes": [15, 16]
}
```

### Case 4: Generate vuot curriculum con lai

```json
{
  "code": "NOT_ENOUGH_CURRICULUM_SESSIONS"
}
```

### Case 5: Review/partial/not_started

- Khong tang `currentSessionIndex`

### Case 6: Skipped

- Tang `currentSessionIndex`
- Bat buoc co reason

---

## 16. API Nen Co Them

Ngoai `POST /api/classes`, nen co:

1. `POST /api/classes/preview-sessions`
2. `POST /api/class-sessions/{id}/teaching-log`
3. `POST /api/classes/{id}/resync-future-lessons`

`resync-future-lessons` dung khi:

- lesson bi `partial`
- lesson bi `not_started`
- admin muon BE day lesson tuong lai lui lai theo cursor moi

---

## 17. FE Types Goi Y

```ts
export type CreateClassRequest = {
  code: string;
  name: string;
  programId: string;
  levelId: string;
  startModuleId: string;
  startSessionIndex: number;
  schedule: {
    daysOfWeek: number[];
    startTime: string;
    endTime: string;
  };
  startDate: string;
  sessionsToGenerate: number;
  skipHolidays?: boolean;
};

export type PreviewClassSessionsResponse = {
  expectedEndDate: string | null;
  sessions: {
    classSessionNo: number;
    date: string;
    moduleName: string;
    unitName: string | null;
    lessonTitle: string | null;
    curriculumSessionIndex: number;
  }[];
  warnings: string[];
};
```

---

## 18. Chot Ngan Cho FE

FE can giu 9 diem sau:

1. Class chon `startModuleId + startSessionIndex`, khong chon lesson thu cong.
2. BE generate `ClassSession` dua tren `LessonPlanTemplate.curriculumSessionIndex`.
3. `sessionsToGenerate` co the chay tiep sang module sau.
4. `ClassModuleProgress` phai giu ca progress hoc that va progress curriculum.
5. `TeachingLog` moi quyet dinh lesson co consume hay khong.
6. `review`, `partial`, `not_started` co the day lesson sang buoi sau.
7. Billing/session balance khong bang curriculum progress.
8. Auto chuyen module dua tren `completedLessonPlans`.
9. FE nen co preview truoc khi create that.
