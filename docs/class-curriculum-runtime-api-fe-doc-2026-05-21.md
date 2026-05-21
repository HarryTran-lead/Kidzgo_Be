# FE API Doc - Class Curriculum Runtime Flow

> Updated: 2026-05-21  
> Scope: cac API va field moi lien quan den flow `Program -> Level -> Module -> Unit -> LessonPlanTemplate`, tao lop theo `startModuleId + startSessionIndex`, runtime teaching log, va resync lesson tuong lai.

---

## 1. Flow Tong Quan

BE dang chay theo tu duy:

- Class duoc tao tu `programId + levelId + startModuleId + startSessionIndex`
- BE tu generate sessions va gan lesson template theo curriculum
- Runtime lesson consume duoc quyet dinh boi `teaching-log`
- `review`, `partial`, `not_started` co the giu lesson lai cho buoi sau
- Future sessions co the duoc resync theo cursor moi

---

## 2. Luu Y Quan Trong Cho FE

### 2.1 Tao lich hoc

`POST /api/classes` va `POST /api/classes/preview-sessions` ho tro 2 cach gui lich:

1. Gui `schedule`
2. Gui `weeklyScheduleSlots`

Neu gui ca hai, BE uu tien `weeklyScheduleSlots`.

### 2.2 Mapping `daysOfWeek`

Neu `schedule.daysOfWeek` co chua `0`, BE hieu theo quy uoc `0-6`:

- `0 = Sunday`
- `1 = Monday`
- `2 = Tuesday`
- `3 = Wednesday`
- `4 = Thursday`
- `5 = Friday`
- `6 = Saturday`

Neu khong co `0`, BE hieu theo quy uoc `1-7`:

- `1 = Sunday`
- `2 = Monday`
- `3 = Tuesday`
- `4 = Wednesday`
- `5 = Thursday`
- `6 = Friday`
- `7 = Saturday`

### 2.3 Runtime progress

FE can tach ro 2 loai progress:

- `completedClassSessions`: so buoi hoc thuc te da hoc
- `completedLessonPlans`: so lesson curriculum da consume

Auto chuyen module dua tren `completedLessonPlans`, khong dua tren `completedClassSessions`.

---

## 3. API Moi Va API Co Field Moi

### Nhom Class

- `POST /api/classes`
- `POST /api/classes/preview-sessions`
- `GET /api/classes`
- `GET /api/classes/{id}`
- `PUT /api/classes/{id}`
- `POST /api/classes/{id}/resync-future-lessons`

### Nhom Session Runtime

- `GET /api/sessions`
- `GET /api/sessions/{id}`
- `POST /api/sessions/{sessionId}/teaching-log`
- `GET /api/sessions/{sessionId}/teaching-log`
- `PUT /api/sessions/{sessionId}/teaching-log`

---

## 4. Create Class

### `POST /api/classes`

Role:

- `Admin`
- `ManagementStaff`

Request:

```json
{
  "branchId": "uuid",
  "programId": "uuid",
  "levelId": "uuid",
  "startModuleId": "uuid",
  "startSessionIndex": 1,
  "code": "CLS_STARTERS_02_EVENING",
  "name": "Starters 02 - Toi 2-4-6",
  "roomId": "uuid-or-null",
  "mainTeacherId": "uuid-or-null",
  "assistantTeacherId": "uuid-or-null",
  "slotTypeId": "uuid-or-null",
  "startDate": "2026-06-01",
  "endDate": null,
  "capacity": 16,
  "sessionsToGenerate": 24,
  "skipHolidays": true,
  "schedule": {
    "daysOfWeek": [2, 4, 6],
    "startTime": "18:00",
    "endTime": "19:30"
  },
  "weeklyScheduleSlots": null,
  "description": "Optional note"
}
```

Request fields:

- `startModuleId`: module bat dau
- `startSessionIndex`: buoi bat dau trong module
- `sessionsToGenerate`: so sessions BE se generate ngay luc tao class
- `skipHolidays`: bo qua ngay nghi khi generate
- `schedule`: input dang don gian cho FE
- `weeklyScheduleSlots`: input nang cao, neu gui field nay thi BE se dung field nay

Response data:

```json
{
  "id": "uuid",
  "branchId": "uuid",
  "programId": "uuid",
  "levelId": "uuid",
  "startModuleId": "uuid",
  "startSessionIndex": 1,
  "currentModuleId": "uuid",
  "currentSessionIndex": 1,
  "currentLessonPlanTemplateId": "uuid-or-null",
  "code": "CLS_STARTERS_02_EVENING",
  "title": "Starters 02 - Toi 2-4-6",
  "roomId": "uuid-or-null",
  "mainTeacherId": "uuid-or-null",
  "assistantTeacherId": "uuid-or-null",
  "slotTypeId": "uuid-or-null",
  "slotTypeCode": "MAIN-or-null",
  "startDate": "2026-06-01",
  "expectedEndDate": "2026-07-24",
  "actualEndDate": null,
  "endDate": null,
  "status": "Active",
  "capacity": 16,
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "MO",
      "startTime": "18:00",
      "durationMinutes": 90
    }
  ],
  "description": "Optional note",
  "name": "Starters 02 - Toi 2-4-6",
  "scheduleText": "Mon 18:00 (90m), Wed 18:00 (90m), Fri 18:00 (90m)"
}
```

---

## 5. Preview Sessions

### `POST /api/classes/preview-sessions`

Role:

- `Admin`
- `ManagementStaff`

Request body giong `POST /api/classes`.

Khac biet:

- `sessionsToGenerate` nen > 0
- endpoint nay khong tao class that

Response data:

```json
{
  "expectedEndDate": "2026-07-24",
  "sessions": [
    {
      "classSessionNo": 1,
      "date": "2026-06-02",
      "moduleName": "Starter02",
      "unitName": "UNIT 6: LOOK AT US",
      "lessonTitle": "UNIT 6: LOOK AT US - Lesson 1",
      "curriculumSessionIndex": 1
    }
  ],
  "warnings": []
}
```

FE nen dung endpoint nay truoc khi submit create that.

---

## 6. Get Classes

### `GET /api/classes`

Role:

- `Admin`
- `ManagementStaff`
- `Parent`

Query params:

- `branchId?: string`
- `programId?: string`
- `teacherId?: string`
- `studentId?: string`
- `status?: string`
- `searchTerm?: string`
- `pageNumber?: number`
- `pageSize?: number`

Response data shape:

```json
{
  "classes": {
    "items": [
      {
        "id": "uuid",
        "branchId": "uuid",
        "branchName": "Branch A",
        "programId": "uuid",
        "programName": "Kids English",
        "levelId": "uuid",
        "levelName": "Starters",
        "startModuleId": "uuid",
        "startSessionIndex": 1,
        "startModuleName": "Starter02",
        "currentModuleId": "uuid",
        "currentSessionIndex": 7,
        "currentLessonPlanTemplateId": "uuid-or-null",
        "currentLessonTitle": "UNIT 7 - Lesson 1",
        "currentModuleName": "Starter02",
        "slotTypeId": "uuid-or-null",
        "slotTypeCode": "MAIN-or-null",
        "code": "CLS_STARTERS_02_EVENING",
        "title": "Starters 02 - Toi 2-4-6",
        "mainTeacherId": "uuid-or-null",
        "mainTeacherName": "Teacher A",
        "assistantTeacherId": "uuid-or-null",
        "assistantTeacherName": "Teacher B",
        "startDate": "2026-06-01",
        "expectedEndDate": "2026-07-24",
        "actualEndDate": null,
        "endDate": null,
        "status": "Active",
        "capacity": 16,
        "currentEnrollmentCount": 12,
        "weeklyScheduleSlots": [],
        "description": "Optional note",
        "name": "Starters 02 - Toi 2-4-6",
        "roomId": "uuid-or-null",
        "roomName": "Room A1",
        "scheduleText": "Mon 18:00 (90m), Wed 18:00 (90m), Fri 18:00 (90m)",
        "studentCount": 12,
        "totalSessions": 24,
        "completedSessions": 8,
        "totalCurriculumSessions": 50,
        "completedClassSessions": 8,
        "completedLessonPlans": 6,
        "progressPercent": 33.33,
        "operationalProgressPercent": 16.00,
        "curriculumProgressPercent": 12.00
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 1
  }
}
```

Field moi FE can dung:

- `startSessionIndex`
- `currentSessionIndex`
- `currentLessonPlanTemplateId`
- `currentLessonTitle`
- `expectedEndDate`
- `actualEndDate`
- `totalCurriculumSessions`
- `completedClassSessions`
- `completedLessonPlans`
- `operationalProgressPercent`
- `curriculumProgressPercent`

---

## 7. Get Class Detail

### `GET /api/classes/{id}`

Role:

- `Admin`
- `ManagementStaff`

Response data shape:

```json
{
  "id": "uuid",
  "branchId": "uuid",
  "branchName": "Branch A",
  "programId": "uuid",
  "programName": "Kids English",
  "levelId": "uuid",
  "levelName": "Starters",
  "startModuleId": "uuid",
  "startSessionIndex": 1,
  "startModuleName": "Starter02",
  "currentModuleId": "uuid",
  "currentSessionIndex": 7,
  "currentLessonPlanTemplateId": "uuid-or-null",
  "currentLessonTitle": "UNIT 7 - Lesson 1",
  "currentModuleName": "Starter02",
  "code": "CLS_STARTERS_02_EVENING",
  "title": "Starters 02 - Toi 2-4-6",
  "startDate": "2026-06-01",
  "expectedEndDate": "2026-07-24",
  "actualEndDate": null,
  "status": "Active",
  "capacity": 16,
  "currentEnrollmentCount": 12,
  "teacherIds": ["uuid1", "uuid2"],
  "teacherNames": ["Teacher A", "Teacher B"],
  "totalSessions": 24,
  "completedSessions": 8,
  "totalCurriculumSessions": 50,
  "completedClassSessions": 8,
  "completedLessonPlans": 6,
  "progressPercent": 33.33,
  "operationalProgressPercent": 16.00,
  "curriculumProgressPercent": 12.00,
  "moduleProgresses": [
    {
      "moduleId": "uuid",
      "moduleName": "Starter02",
      "orderIndex": 2,
      "requiredSessions": 16,
      "completedClassSessions": 8,
      "completedLessonPlans": 6,
      "startSessionIndex": 1,
      "currentSessionIndex": 7,
      "status": "Active",
      "startedAt": "2026-06-01T11:00:00Z",
      "completedAt": null
    }
  ],
  "scheduleSegments": [
    {
      "id": "uuid",
      "effectiveFrom": "2026-06-01",
      "effectiveTo": null,
      "weeklyScheduleSlots": [
        {
          "dayOfWeek": "MO",
          "startTime": "18:00",
          "durationMinutes": 90
        }
      ]
    }
  ]
}
```

Field `moduleProgresses` la field chinh de FE render runtime progress tung module.

---

## 8. Update Class

### `PUT /api/classes/{id}`

Role:

- `Admin`
- `ManagementStaff`

Request:

```json
{
  "branchId": "uuid",
  "programId": "uuid",
  "levelId": "uuid",
  "startModuleId": "uuid",
  "startSessionIndex": 1,
  "code": "CLS_STARTERS_02_EVENING",
  "name": "Starters 02 - Toi 2-4-6",
  "roomId": "uuid-or-null",
  "mainTeacherId": "uuid-or-null",
  "assistantTeacherId": "uuid-or-null",
  "slotTypeId": "uuid-or-null",
  "startDate": "2026-06-01",
  "endDate": null,
  "capacity": 16,
  "weeklyScheduleSlots": [
    {
      "dayOfWeek": "MO",
      "startTime": "18:00",
      "durationMinutes": 90
    }
  ],
  "description": "Optional note"
}
```

Response data gan giong `CreateClassResponse`.

Luu y:

- update `startModuleId` hoac `startSessionIndex` se bi chan neu class da co sessions trong mot so case
- endpoint nay khong co `sessionsToGenerate`

---

## 9. Resync Future Lessons

### `POST /api/classes/{id}/resync-future-lessons`

Role:

- `Admin`
- `ManagementStaff`

Muc dich:

- tinh lai cursor curriculum tu teaching logs da co
- day lai lesson cho cac sessions tuong lai chua completed/cancelled

Response:

```json
{
  "classId": "uuid",
  "updatedSessionCount": 10,
  "currentModuleId": "uuid",
  "currentSessionIndex": 7,
  "currentLessonPlanTemplateId": "uuid-or-null"
}
```

FE nen goi endpoint nay khi:

- admin muon force sync lai lesson tuong lai
- sau mot loat thay doi teaching log can reload lich

---

## 10. Get Sessions

### `GET /api/sessions`

Role:

- `Admin`
- `ManagementStaff`

Query params:

- `classId?: string`
- `branchId?: string`
- `status?: Scheduled | Completed | Cancelled`
- `from?: datetime`
- `to?: datetime`
- `pageNumber?: number`
- `pageSize?: number`

Response data shape:

```json
{
  "sessions": {
    "items": [
      {
        "id": "uuid",
        "classId": "uuid",
        "moduleId": "uuid-or-null",
        "lessonPlanTemplateId": "uuid-or-null",
        "sessionIndexInModule": 7,
        "classCode": "CLS_STARTERS_02_EVENING",
        "classTitle": "Starters 02 - Toi 2-4-6",
        "branchId": "uuid",
        "branchName": "Branch A",
        "plannedDatetime": "2026-06-10T18:00:00",
        "actualDatetime": "2026-06-10T18:05:00",
        "durationMinutes": 90,
        "participationType": "Main",
        "sectionType": "Normal",
        "status": "Completed",
        "plannedTeacherId": "uuid-or-null",
        "plannedTeacherName": "Teacher A",
        "actualTeacherId": "uuid-or-null",
        "actualTeacherName": "Teacher A",
        "teachingLogId": "uuid-or-null",
        "teachingLogStatus": "Submitted",
        "teachingProgressStatus": "Partial",
        "actualTeachingType": "Review"
      }
    ]
  }
}
```

Field moi FE can dung o list:

- `teachingLogId`
- `teachingLogStatus`
- `teachingProgressStatus`
- `actualTeachingType`

---

## 11. Get Session Detail

### `GET /api/sessions/{id}`

Response data shape:

```json
{
  "session": {
    "id": "uuid",
    "classId": "uuid",
    "moduleId": "uuid-or-null",
    "lessonPlanTemplateId": "uuid-or-null",
    "sessionIndexInModule": 7,
    "classCode": "CLS_STARTERS_02_EVENING",
    "classTitle": "Starters 02 - Toi 2-4-6",
    "plannedDatetime": "2026-06-10T18:00:00",
    "actualDatetime": "2026-06-10T18:05:00",
    "status": "Completed",
    "plannedLessonTitle": "UNIT 7 - Lesson 1",
    "actualLessonPlanTemplateId": "uuid-or-null",
    "actualLessonTitle": "UNIT 7 - Lesson 1",
    "teachingLogId": "uuid-or-null",
    "teachingLogStatus": "Submitted",
    "teachingProgressStatus": "Partial",
    "actualTeachingType": "Review",
    "actualContent": "Reviewed phonics and warm-up only",
    "actualHomework": "Workbook page 20",
    "teacherNote": "Lesson not completed",
    "attendanceSummary": {
      "totalStudents": 12,
      "presentCount": 11,
      "absentCount": 1,
      "makeupCount": 0,
      "notMarkedCount": 0
    }
  }
}
```

---

## 12. Submit Teaching Log

### `POST /api/sessions/{sessionId}/teaching-log`

Role:

- `Admin`
- `ManagementStaff`
- `Teacher`

Request:

```json
{
  "actualLessonPlanTemplateId": "uuid-or-null",
  "actualTeachingType": "review",
  "progressStatus": "partial",
  "actualContent": "Reviewed old content only",
  "actualHomework": "Workbook page 20",
  "teacherNote": "Lesson not finished"
}
```

Accepted `actualTeachingType`:

- `normal`
- `review`
- `test`
- `makeup`
- `event`
- `other`

Accepted `progressStatus`:

- `completed`
- `partial`
- `not_started`
- `skipped`

Rule:

- `skipped` bat buoc co `teacherNote`
- `partial` va `not_started` khong consume lesson
- `completed` va `skipped` consume lesson
- submit xong BE se auto recalculate progress va resync future lessons

Response:

```json
{
  "teachingLogId": "uuid",
  "sessionId": "uuid",
  "plannedLessonPlanTemplateId": "uuid-or-null",
  "actualLessonPlanTemplateId": "uuid-or-null",
  "actualTeachingType": "Review",
  "progressStatus": "Partial",
  "classId": "uuid",
  "currentModuleId": "uuid-or-null",
  "currentSessionIndex": 7,
  "currentLessonPlanTemplateId": "uuid-or-null",
  "updatedFutureSessionCount": 10
}
```

---

## 13. Get Teaching Log

### `GET /api/sessions/{sessionId}/teaching-log`

Role:

- `Admin`
- `ManagementStaff`
- `Teacher`

Response:

```json
{
  "teachingLogId": "uuid",
  "sessionId": "uuid",
  "plannedLessonPlanTemplateId": "uuid-or-null",
  "plannedLessonTitle": "UNIT 7 - Lesson 1",
  "actualLessonPlanTemplateId": "uuid-or-null",
  "actualLessonTitle": "UNIT 7 - Lesson 1",
  "teachingLogStatus": "Submitted",
  "progressStatus": "Partial",
  "actualTeachingType": "Review",
  "actualContent": "Reviewed old content only",
  "actualHomework": "Workbook page 20",
  "teacherNote": "Lesson not finished",
  "submittedBy": "uuid-or-null",
  "submittedAt": "2026-06-10T11:10:00Z",
  "updatedAt": "2026-06-10T11:10:00Z"
}
```

---

## 14. Update Teaching Log

### `PUT /api/sessions/{sessionId}/teaching-log`

Role:

- `Admin`
- `ManagementStaff`
- `Teacher`

Request body giong `POST /api/sessions/{sessionId}/teaching-log`.

Response:

```json
{
  "teachingLogId": "uuid",
  "sessionId": "uuid",
  "classId": "uuid",
  "plannedLessonPlanTemplateId": "uuid-or-null",
  "actualLessonPlanTemplateId": "uuid-or-null",
  "actualTeachingType": "Completed",
  "progressStatus": "Completed",
  "currentModuleId": "uuid-or-null",
  "currentSessionIndex": 8,
  "currentLessonPlanTemplateId": "uuid-or-null",
  "updatedFutureSessionCount": 9
}
```

Luu y:

- endpoint nay se bi chan neu teaching log da `Approved` hoac `Locked`
- update xong BE tiep tuc auto recalculate va resync future sessions

---

## 15. Frontend Types Goi Y

```ts
export type CreateClassRequest = {
  branchId: string;
  programId: string;
  levelId: string;
  startModuleId: string;
  startSessionIndex: number;
  code: string;
  name?: string | null;
  title?: string | null;
  roomId?: string | null;
  mainTeacherId?: string | null;
  assistantTeacherId?: string | null;
  slotTypeId?: string | null;
  startDate: string;
  endDate?: string | null;
  capacity: number;
  sessionsToGenerate?: number | null;
  skipHolidays?: boolean;
  schedule?: {
    daysOfWeek: number[];
    startTime: string;
    endTime: string;
  } | null;
  weeklyScheduleSlots?: {
    dayOfWeek: string;
    startTime: string;
    durationMinutes: number;
  }[] | null;
  description?: string | null;
};

export type SubmitTeachingLogRequest = {
  actualLessonPlanTemplateId?: string | null;
  actualTeachingType?: "normal" | "review" | "test" | "makeup" | "event" | "other" | null;
  progressStatus: "completed" | "partial" | "not_started" | "skipped";
  actualContent?: string | null;
  actualHomework?: string | null;
  teacherNote?: string | null;
};
```

---

## 16. Error Cases FE Nen Handle

### Curriculum / class creation

- `MODULE_NOT_IN_LEVEL`
- `INVALID_START_SESSION_INDEX`
- `MISSING_LESSON_PLAN_TEMPLATE`
- `NOT_ENOUGH_CURRICULUM_SESSIONS`

### Teaching log

- `Session.TeachingLogAlreadyExists`
- `Session.TeachingLogNotFound`
- `Session.TeachingLogLocked`
- `Session.MissingLessonTemplateForTeachingLog`
- `Session.InvalidTeachingProgressStatus`
- `Session.SkippedRequiresReason`

---

## 17. Chot Ngan Cho FE

FE nen giu 8 diem sau:

1. Class duoc tao bang `startModuleId + startSessionIndex`, khong chon lesson thu cong.
2. `preview-sessions` nen duoc goi truoc create that.
3. `currentSessionIndex` la cursor curriculum hien tai, khong phai so buoi da hoc.
4. `completedClassSessions` va `completedLessonPlans` la 2 progress khac nhau.
5. `teaching-log` moi quyet dinh lesson co consume hay khong.
6. `partial` va `not_started` se giu lesson lai cho buoi sau.
7. `resync-future-lessons` dung de dong bo lai lesson future sessions.
8. O session list/detail, FE da co du field de render runtime status ma khong can goi them endpoint phu trong phan lon case.
