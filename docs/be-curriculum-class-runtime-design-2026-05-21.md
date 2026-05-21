# BE Design - Curriculum Class Runtime

> Created: 2026-05-21  
> Muc dich: chot implementation target cho BE theo model curriculum moi va ghi ro cac gap voi code hien tai.

---

## 1. Final Model

Hierarchy dung:

```text
Program
  -> Level
    -> CurriculumModule
      -> CurriculumUnit
        -> LessonPlanTemplate
```

Runtime class:

```text
Class
  -> ClassModuleProgress
  -> ClassSchedule
  -> ClassSession
  -> TeachingLog
```

---

## 2. Entity Target

### 2.1 Curriculum

- `Program`
- `Level.totalSessions`
- `CurriculumModule.totalSessions`
- `CurriculumUnit`
- `LessonPlanTemplate`

`LessonPlanTemplate` target fields:

- `moduleId`
- `unitId`
- `code`
- `title`
- `lessonIndexInUnit`
- `curriculumSessionIndex`
- `sourceFileName`
- `sourceFileUrl`
- `objectivesJson`
- `vocabularyJson`
- `grammarJson`
- `rawProcedureText`
- `rawHomeworkText`
- `status`

Constraints:

- unique `(moduleId, curriculumSessionIndex)`
- `lessonIndexInUnit` phan biet trong pham vi `unitId`

### 2.2 Class

`Class` can them hoac doi sematics:

- `startModuleId`
- `startSessionIndex`
- `currentModuleId`
- `currentSessionIndex`
- `currentLessonPlanTemplateId`
- `expectedEndDate`
- `actualEndDate`

### 2.3 ClassModuleProgress

Target:

```text
requiredSessions
completedClassSessions
completedLessonPlans
startSessionIndex
currentSessionIndex
status
```

### 2.4 ClassSchedule

Target fields:

- `classId`
- `daysOfWeek`
- `startTime`
- `endTime`
- `effectiveFrom`
- `effectiveTo`
- `status`

### 2.5 ClassSession

Target fields:

- `classId`
- `classSessionNo`
- `moduleId`
- `unitId`
- `lessonPlanTemplateId`
- `plannedCurriculumSessionIndex`
- `sessionDate`
- `startTime`
- `endTime`
- `status`
- `teachingLogId`

### 2.6 TeachingLog

Target fields:

- `classSessionId`
- `plannedLessonPlanId`
- `actualLessonPlanId`
- `actualTeachingType`
- `progressStatus`
- `actualContent`
- `actualHomework`
- `teacherNote`
- `submittedBy`
- `submittedAt`
- `approvedBy`
- `approvedAt`

---

## 3. Create Class Flow

`POST /api/classes`

Input can co:

- `code`
- `name`
- `programId`
- `levelId`
- `startModuleId`
- `startSessionIndex`
- `schedule`
- `startDate`
- `sessionsToGenerate`
- `skipHolidays`

BE flow:

1. Validate `programId`
2. Validate `levelId` thuoc `programId`
3. Validate `startModuleId` thuoc `levelId`
4. Validate `startSessionIndex in 1..module.totalSessions`
5. Validate module khong thieu `LessonPlanTemplate`
6. Tao `Class`
7. Tao `ClassSchedule`
8. Tao `ClassModuleProgress`
9. Generate `ClassSession`
10. Set `expectedEndDate`

---

## 4. Session Generation Rule

Algorithm:

```text
cursorModule = startModuleId
cursorSessionIndex = startSessionIndex
classSessionNo = 1

while classSessionNo <= sessionsToGenerate:
  lesson = LessonPlanTemplate(moduleId = cursorModule, curriculumSessionIndex = cursorSessionIndex)
  create ClassSession
  cursorSessionIndex += 1
  if cursorSessionIndex > module.totalSessions:
    cursorModule = next module
    cursorSessionIndex = 1
  classSessionNo += 1
```

Neu het curriculum:

- Khuyen dung tra error `NOT_ENOUGH_CURRICULUM_SESSIONS`
- Khong nen generate session khong co lesson template

---

## 5. Runtime Progression Rule

`TeachingLog.progressStatus` la quyet dinh lesson co consume khong:

- `completed` -> consume
- `partial` -> khong consume
- `not_started` -> khong consume
- `skipped` -> consume theo nghia bo qua lesson, bat buoc co reason

Khi consume:

1. `completedLessonPlans += 1`
2. `currentSessionIndex += 1`
3. Neu vuot `module.totalSessions` -> complete module, move sang module tiep theo

`completedClassSessions` chi tang khi session hoc that duoc complete.

---

## 6. Auto Move Module

Rule dung:

```text
if completedLessonPlans >= requiredSessions:
  complete current module
  activate next module
```

Khong dung:

```text
if completedClassSessions >= requiredSessions
```

Vi review/makeup/event co the tieu ton buoi hoc that nhung chua consume lesson.

---

## 7. API Nen Co

- `POST /api/classes`
- `POST /api/classes/preview-sessions`
- `POST /api/class-sessions/{id}/teaching-log`
- `POST /api/classes/{id}/resync-future-lessons`

---

## 8. Error Codes Nen Chot

- `MODULE_NOT_IN_LEVEL`
- `INVALID_START_SESSION_INDEX`
- `MISSING_LESSON_PLAN_TEMPLATE`
- `NOT_ENOUGH_CURRICULUM_SESSIONS`

---

## 9. Gap So Với Code Hien Tai

Code hien tai dang co cac chenh lech chinh:

1. `CreateClassCommand` chua co `startSessionIndex`, `sessionsToGenerate`, `skipHolidays`.
2. `Class` moi co `StartModuleId`, `CurrentModuleId`, chua co `CurrentSessionIndex`, `CurrentLessonPlanTemplateId`, `ExpectedEndDate`.
3. `ClassModuleProgress` moi co `CompletedSessions`, chua tach `completedClassSessions` va `completedLessonPlans`, chua co `startSessionIndex`, `currentSessionIndex`.
4. `ClassSessionPlanningService` dang plan theo tong session tu dau module, chua support bat dau giua module.
5. `TeachingLog` hien tai chua mang du semantics `plannedLessonPlanId`, `actualLessonPlanId`, `actualTeachingType`, `progressStatus`.
6. Chua co preview sessions va resync future lessons.

---

## 10. Thu Tu Implement Khuyen Nghi

1. Update domain entities + EF configuration
2. Tao migration
3. Update import/de du lieu curriculum cho `curriculumSessionIndex`
4. Refactor create class command + validator + handler
5. Tao preview endpoint
6. Refactor session generation service
7. Them teaching log progression service
8. Them resync future lessons endpoint
9. Cap nhat response DTO va FE docs
