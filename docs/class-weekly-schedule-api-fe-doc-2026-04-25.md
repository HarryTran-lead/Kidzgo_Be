# Class / Weekly Schedule API FE Doc

## Muc tieu

Tai lieu nay tong hop cac API da thay doi sau khi backend chuyen sang model `weeklyScheduleSlots` cho class.

Frontend can luu y:

- `schedulePattern` da bo khoi cac API class.
- Class schedule dung `weeklyScheduleSlots`.
- Enrollment/assign/transfer/reassign da bo `sessionSelectionPattern`.
- Tat ca API public lien quan den chon buoi hoc cua hoc vien da chot sang `weeklyPattern`.
- Class co the co nhieu slot/tuần, moi slot co ngay + gio rieng.
- Class co the tao voi ngay trong qua khu.
- Khi tao class, backend dang mac dinh `status = "Active"`.
- Generate sessions co the tao ca buoi trong qua khu neu `onlyFutureSessions = false`.

## Wrapper chung

### Success

Tat ca API dung `MatchOk()` / `MatchCreated()` tra ve:

```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error

Da so loi tra ve Problem Details:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Class.CodeExists",
  "status": 409,
  "detail": "Class code already exists"
}
```

Neu loi validator/pipeline, co them `errors`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation.General",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "errors": [
    {
      "code": "WeeklySchedule.Empty",
      "description": "Weekly schedule cannot be empty"
    }
  ]
}
```

## Kieu du lieu chung

### `ScheduleSlot`

```json
{
  "dayOfWeek": "TH",
  "startTime": "18:00",
  "durationMinutes": 90
}
```

Rule:

- `dayOfWeek`: `MO`, `TU`, `WE`, `TH`, `FR`, `SA`, `SU`
- `startTime`: dinh dang `HH:mm`
- `durationMinutes` > 0
- khong duoc duplicate cung `dayOfWeek + startTime`

### `weeklyPattern`

`weeklyPattern` la subset cua class schedule, xac dinh hoc vien hoc nhung buoi nao.

Vi du:

```json
[
  { "dayOfWeeks": ["TU", "TH"], "startTime": "18:00", "durationMinutes": 90 },
  { "dayOfWeeks": ["SA"], "startTime": "17:00", "durationMinutes": 60 }
]
```

Rule:

- `dayOfWeeks`: 1 hoac nhieu ngay, dung `MO`, `TU`, `WE`, `TH`, `FR`, `SA`, `SU`
- `startTime`: dinh dang `HH:mm`
- `durationMinutes` > 0, phai khop voi lich class
- co the 1 buoi/tuần, 2 buoi/tuần hoac nhieu hon
- neu nhieu ngay cung gio va duration thi gop chung vao 1 entry
- neu khac gio hoac khac duration thi tach thanh nhieu entry
- FE nen lay `durationMinutes` tu `weeklyScheduleSlots` cua class de pre-fill vao weeklyPattern

## API da thay doi

### 1. POST `/api/classes`

- Role: `Admin`, `ManagementStaff`
- Muc dich: tao class bang `weeklyScheduleSlots`

Request:

```json
{
  "branchId": "guid",
  "programId": "guid",
  "code": "APPLE-A2",
  "title": "Apple A2",
  "roomId": "guid",
  "mainTeacherId": "guid",
  "assistantTeacherId": "guid",
  "startDate": "2026-04-01",
  "endDate": "2026-12-31",
  "capacity": 8,
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TH", "startTime": "18:00", "durationMinutes": 60 },
    { "dayOfWeek": "SA", "startTime": "17:00", "durationMinutes": 60 }
  ],
  "description": "Apple 2"
}
```

Response `data`:

```json
{
  "id": "guid",
  "branchId": "guid",
  "programId": "guid",
  "code": "APPLE-A2",
  "title": "Apple A2",
  "roomId": "guid",
  "mainTeacherId": "guid",
  "assistantTeacherId": "guid",
  "startDate": "2026-04-01",
  "endDate": "2026-12-31",
  "status": "Active",
  "capacity": 8,
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TH", "startTime": "18:00", "durationMinutes": 60 },
    { "dayOfWeek": "SA", "startTime": "17:00", "durationMinutes": 60 }
  ],
  "description": "Apple 2",
  "name": "Apple A2",
  "scheduleText": "Thu 5 18:00-19:00, Thu 7 17:00-18:00"
}
```

Loi hay gap:

- `Class.BranchNotFound`: `Branch not found or inactive`
- `Class.ProgramNotFound`: `Program not found, deleted, or inactive`
- `Class.ProgramNotAvailableInBranch`: `Program is not assigned to the selected branch`
- `Class.CodeExists`: `Class code already exists`
- `Class.RoomNotFound`: `Room not found or inactive`
- `Class.RoomBranchMismatch`: `Room must belong to the same branch as the class`
- `Class.MainTeacherNotFound`
- `Class.MainTeacherBranchMismatch`
- `Class.AssistantTeacherNotFound`
- `Class.AssistantTeacherBranchMismatch`
- `Class.TeacherAndAssistantMustDiffer`
- `WeeklySchedule.Empty`
- `SchedulePattern.InvalidDayOfWeek`
- `SchedulePattern.InvalidStartTime`
- `SchedulePattern.InvalidDuration`
- `SchedulePattern.DuplicateSlot`
- `Class.RoomConflict`
- `Class.TeacherConflict`
- `Class.AssistantConflict`

Luu y:

- Backend se lay `title = name ?? title ?? code`
- Co the tao class voi `startDate` trong qua khu
- Neu co `weeklyScheduleSlots` thi FE nen gui `endDate`

### 2. PUT `/api/classes/{id}`

- Role: `Admin`, `ManagementStaff`
- Muc dich: cap nhat class, payload giong tao class

Request: giong `POST /api/classes`

Response `data`: giong `CreateClassResponse`

Loi hay gap:

- tat ca loi cua create class
- `Class.NotFound`
- `Class.HasOperationalDependencies`
- `Class.CapacityBelowActiveEnrollments`
- `Class.HasFutureSessions`

Luu y:

- Neu class da co future sessions scheduled, backend chan doi lich class level bang loi `Class.HasFutureSessions`
- Non-terminal status se duoc normalize lai theo lifecycle hien tai; class tao moi thi van default `Active`

### 3. GET `/api/classes`

- Role: `Admin`, `ManagementStaff`, `Parent`
- Query:
  - `branchId`
  - `programId`
  - `teacherId`
  - `studentId`
  - `status`
  - `searchTerm`
  - `pageNumber`
  - `pageSize`

Response `data.classes`:

```json
{
  "items": [
    {
      "id": "guid",
      "branchId": "guid",
      "branchName": "CN 1",
      "programId": "guid",
      "programName": "Apple 2",
      "code": "APPLE-A2",
      "title": "Apple A2",
      "mainTeacherId": "guid",
      "mainTeacherName": "Teacher A",
      "assistantTeacherId": "guid",
      "assistantTeacherName": "Teacher B",
      "startDate": "2026-04-01",
      "endDate": "2026-12-31",
      "status": "Active",
      "capacity": 8,
      "currentEnrollmentCount": 5,
      "weeklyScheduleSlots": [
        { "dayOfWeek": "TH", "startTime": "18:00", "durationMinutes": 60 },
        { "dayOfWeek": "SA", "startTime": "17:00", "durationMinutes": 60 }
      ],
      "description": "Apple 2",
      "name": "Apple A2",
      "roomId": "guid",
      "roomName": "Room APPLE-A2",
      "scheduleText": "Thu 5 18:00-19:00, Thu 7 17:00-18:00",
      "studentCount": 5,
      "totalSessions": 48,
      "completedSessions": 4,
      "progressPercent": 8.33
    }
  ],
  "pageNumber": 1,
  "totalPages": 1,
  "totalCount": 1
}
```

Luu y:

- `weeklyScheduleSlots` la lich effective hien tai, da tinh theo `schedule segment` neu class co segment

### 4. GET `/api/classes/{id}`

- Role: `Admin`, `ManagementStaff`

Response `data`:

```json
{
  "id": "guid",
  "branchId": "guid",
  "branchName": "CN 1",
  "programId": "guid",
  "programName": "Apple 2",
  "code": "APPLE-A2",
  "title": "Apple A2",
  "mainTeacherId": "guid",
  "mainTeacherName": "Teacher A",
  "assistantTeacherId": "guid",
  "assistantTeacherName": "Teacher B",
  "startDate": "2026-04-01",
  "endDate": "2026-12-31",
  "status": "Active",
  "capacity": 8,
  "currentEnrollmentCount": 5,
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TH", "startTime": "18:00", "durationMinutes": 60 },
    { "dayOfWeek": "SA", "startTime": "17:00", "durationMinutes": 60 }
  ],
  "scheduleSegments": [
    {
      "id": "guid",
      "effectiveFrom": "2026-06-01",
      "effectiveTo": null,
      "weeklyScheduleSlots": [
        { "dayOfWeek": "TH", "startTime": "18:30", "durationMinutes": 60 }
      ]
    }
  ]
}
```

Luu y:

- `weeklyScheduleSlots` o root = lich effective hien tai
- `scheduleSegments[].weeklyScheduleSlots` = lich cua tung segment
- khong con `schedulePattern`

### 5. POST `/api/classes/{id}/schedule-segments`

- Role: `Admin`, `ManagementStaff`
- Chi support supplementary program

Request:

```json
{
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TH", "startTime": "18:30", "durationMinutes": 60 },
    { "dayOfWeek": "SA", "startTime": "17:30", "durationMinutes": 60 }
  ],
  "generateSessions": true,
  "onlyFutureSessions": true
}
```

Response `data`:

```json
{
  "id": "guid",
  "classId": "guid",
  "programId": "guid",
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "weeklyScheduleSlots": [
    { "dayOfWeek": "TH", "startTime": "18:30", "durationMinutes": 60 },
    { "dayOfWeek": "SA", "startTime": "17:30", "durationMinutes": 60 }
  ],
  "generatedSessionsCount": 8
}
```

Loi hay gap:

- `Class.NotFound`
- `Class.SupplementaryProgramRequired`
- `Class.ScheduleSegmentInvalidEffectiveDate`
- `Class.ScheduleSegmentAlreadyExists`
- `Class.FutureScheduleSegmentExists`
- `WeeklySchedule.Empty`
- `SchedulePattern.InvalidDayOfWeek`
- `SchedulePattern.InvalidStartTime`
- `SchedulePattern.InvalidDuration`
- `SchedulePattern.DuplicateSlot`
- `Session.MissingSchedulePattern`
- `Session.MissingClassEndDate`
- `Session.InvalidClassStatus`
- `Class.RoomConflict`
- `Class.TeacherConflict`
- `Class.AssistantConflict`

Luu y:

- Neu `generateSessions = true` va `onlyFutureSessions = false` thi backend co the tao ca session trong qua khu
- Backend da fix transaction: neu generate session fail, segment moi khong bi luu nua

### 6. GET `/api/teacher/classes`

- Role: `Teacher`
- Query:
  - `pageNumber`
  - `pageSize`
  - `teachingDate`

Response `data.classes.items[*]` da co:

- `weeklyScheduleSlots`
- `role`: `MainTeacher` | `AssistantTeacher`

Luu y:

- `weeklyScheduleSlots` da la lich effective hien tai, tinh theo segment

### 7. GET `/api/students/classes`

- Role: endpoint student authenticated
- Query:
  - `pageNumber`
  - `pageSize`

Response `data.classes.items[*]` da co:

- `weeklyScheduleSlots`
- `scheduleText`
- `totalSessions`
- `completedSessions`
- `progressPercent`

Loi hay gap:

- `Profile.StudentNotFound`
- `Profile.NotFound`

### 8. POST `/api/sessions/generate-from-pattern`

- Role: `Admin`, `ManagementStaff`

Request:

```json
{
  "classId": "guid",
  "onlyFutureSessions": false
}
```

Response `data`:

```json
{
  "createdSessionsCount": 20
}
```

Loi hay gap:

- `Class.NotFound`
- `Session.InvalidClassStatus`
- `Session.MissingSchedulePattern`
- `Session.MissingClassEndDate`
- `Session.InvalidDuration`
- `Session.InvalidBranch`
- `Session.InvalidRoom`
- `Session.InvalidTeacher`
- `Session.InvalidAssistant`
- `Class.RoomConflict`
- `Class.TeacherConflict`
- `Class.AssistantConflict`
- `Session.SaveFailed`

Luu y:

- `onlyFutureSessions = true`: chi tao tu thoi diem hien tai tro di
- `onlyFutureSessions = false`: tao tat ca tu `StartDate` cua class den `EndDate`

### 9. GET `/api/registrations/{id}/suggest-classes`

- Role: `Admin`, `ManagementStaff`

Response `data`:

- `suggestedClasses[*].weeklyScheduleSlots`
- `alternativeClasses[*].weeklyScheduleSlots`
- `secondarySuggestedClasses[*].weeklyScheduleSlots`
- `secondaryAlternativeClasses[*].weeklyScheduleSlots`

Luu y:

- API nay da match theo schedule effective hien tai cua class
- Neu class co segment tuong lai chua active thi frontend chua thay lich tuong lai o day

### 10. GET `/api/registrations/{id}`

- Role: `Admin`, `ManagementStaff`
- Phan thay doi quan trong: `actualStudySchedules`

Field moi/quan trong cho frontend:

```json
{
  "actualStudySchedules": [
    {
      "track": "primary",
      "classId": "guid",
      "className": "Apple A2",
      "programId": "guid",
      "programName": "Apple 2",
      "usesClassDefaultSchedule": true,
      "classWeeklyScheduleSlots": [
        { "dayOfWeek": "TH", "startTime": "18:00", "durationMinutes": 60 },
        { "dayOfWeek": "SA", "startTime": "17:00", "durationMinutes": 60 }
      ],
      "weeklyPattern": [
        { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
      ],
      "effectiveWeeklyPattern": [
        { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
      ],
      "studyDayCodes": ["TH"],
      "studyDays": ["Thu 5"],
      "studyDaySummary": "Thu 5",
      "scheduleSegments": [
        {
          "id": "guid",
          "effectiveFrom": "2026-06-01",
          "effectiveTo": null,
          "weeklyPattern": null,
          "effectiveWeeklyPattern": [
            { "dayOfWeeks": ["TH", "SA"], "startTime": "18:00", "durationMinutes": 60 }
          ],
          "studyDayCodes": ["TH", "SA"],
          "studyDays": ["Thu 5", "Thu 7"],
          "studyDaySummary": "Thu 5, Thu 7"
        }
      ]
    }
  ]
}
```

Luu y:

- `classWeeklyScheduleSlots` la lich class effective hien tai
- `weeklyPattern = null` nghia la hoc vien hoc toan bo lich class
- `effectiveWeeklyPattern` la lich hoc thuc te sau khi merge voi lich class

### 11. POST `/api/registrations/{id}/assign-class`

- Role: `Admin`, `ManagementStaff`

Request:

```json
{
  "classId": "guid",
  "entryType": "immediate",
  "track": "primary",
  "firstStudyDate": "2026-05-03",
  "weeklyPattern": [
    { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
  ]
}
```

Response `data`:

```json
{
  "registrationId": "guid",
  "registrationStatus": "Studying",
  "classId": "guid",
  "classCode": "APPLE-A2",
  "classTitle": "Apple A2",
  "track": "primary",
  "entryType": "immediate",
  "classAssignedDate": "2026-04-25T10:00:00Z",
  "firstStudyDate": "2026-05-03",
  "firstStudySessionAt": "2026-05-03T11:00:00Z",
  "warningMessage": "Class da bat dau. Hoc vien se tham gia giua chung."
}
```

Loi hay gap:

- `Registration.NotFound`
- `Registration.InvalidStatus`
- `Registration.InvalidEntryType`
- `Registration.ClassIdRequired`
- `Registration.ClassNotFound`
- `Registration.ClassNotMatchingBranch`
- `Registration.ClassNotMatchingProgram`
- `Registration.ClassFull`
- `Registration.SecondaryProgramMissing`
- `Registration.ClassAlreadyAssigned`
- `Registration.FirstStudyDateNotAllowed`
- `Registration.FirstStudyDateInPast`
- `Registration.FirstStudyDateBeforeClassStart`
- `Registration.FirstStudyDateAfterClassEnd`
- `Registration.FirstStudyDateNoSession`
- `Enrollment.SessionSelectionPatternInvalid`
- `Enrollment.SessionSelectionPatternEmpty`
- `Enrollment.SessionSelectionPatternMismatch`
- `Enrollment.StudentScheduleConflict`

Luu y:

- `weeklyPattern` la subset cua lich class
- Validation subset duoc tinh tren datetime cua cac buoi hoc (dayOfWeek + startTime), khong so sanh duration
- `durationMinutes` trong `weeklyPattern` bat buoc > 0 (validation), nhung khong can phai khop chinh xac voi duration cua class slot
- Khuyen nghi FE luon dung dung `durationMinutes` tu `weeklyScheduleSlots` cua class khi tao `weeklyPattern`
- Neu `weeklyPattern = null` hoac khong gui, hoc vien hoc toan bo lich class

### 12. POST `/api/registrations/{id}/transfer-class`

- Role: `Admin`, `ManagementStaff`
- Request body:
  - `newClassId`
  - `track`
  - `weeklyPattern`
  - `effectiveDate`

Vi du:

```json
{
  "newClassId": "guid",
  "track": "primary",
  "weeklyPattern": [
    { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
  ],
  "effectiveDate": "2026-05-01T00:00:00Z"
}
```

Loi hay gap:

- `Registration.NotFound`
- `Registration.InvalidStatus`
- `Registration.ClassNotFound`
- `Registration.ClassNotMatchingBranch`
- `Registration.ClassNotMatchingProgram`
- `Registration.CannotTransferToSameClass`
- `Enrollment.SessionSelectionPatternInvalid`
- `Enrollment.SessionSelectionPatternEmpty`
- `Enrollment.SessionSelectionPatternMismatch`
- `Enrollment.StudentScheduleConflict`

### 13. POST `/api/enrollments`

- Role: `Admin`, `ManagementStaff`

Request:

```json
{
  "classId": "guid",
  "studentProfileId": "guid",
  "enrollDate": "2026-04-25",
  "tuitionPlanId": "guid",
  "track": "primary",
  "weeklyPattern": [
    { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
  ]
}
```

Loi hay gap:

- `Enrollment.ClassNotFound`
- `Enrollment.ClassNotAvailable`
- `Enrollment.StudentNotFound`
- `Enrollment.AlreadyEnrolled`
- `Enrollment.ClassFull`
- `Enrollment.TuitionPlanNotFound`
- `Enrollment.TuitionPlanNotAvailable`
- `Enrollment.TuitionPlanProgramMismatch`
- `Enrollment.TuitionPlanBranchMismatch`
- `Enrollment.SessionSelectionPatternInvalid`
- `Enrollment.SessionSelectionPatternEmpty`
- `Enrollment.SessionSelectionPatternMismatch`
- `Enrollment.StudentScheduleConflict`

### 14. PUT `/api/enrollments/{id}`

- Role: `Admin`, `ManagementStaff`

Request:

```json
{
  "enrollDate": "2026-04-25",
  "tuitionPlanId": "guid",
  "track": "primary",
  "weeklyPattern": [
    { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
  ],
  "clearWeeklyPattern": false
}
```

Loi hay gap:

- `Enrollment.NotFound`
- `Enrollment.TuitionPlanNotFound`
- `Enrollment.TuitionPlanNotAvailable`
- `Enrollment.TuitionPlanProgramMismatch`
- `Enrollment.SessionSelectionPatternInvalid`
- `Enrollment.SessionSelectionPatternEmpty`
- `Enrollment.SessionSelectionPatternMismatch`
- `Enrollment.StudentScheduleConflict`

### 15. GET `/api/enrollments/{id}`

- Role: hien tai controller chua bat authorize role comment out

Response `data`:

```json
{
  "id": "guid",
  "classId": "guid",
  "classCode": "APPLE-A2",
  "classTitle": "Apple A2",
  "programId": "guid",
  "programName": "Apple 2",
  "branchId": "guid",
  "branchName": "CN 1",
  "studentProfileId": "guid",
  "studentName": "Student A",
  "enrollDate": "2026-04-25",
  "status": "Active",
  "tuitionPlanId": "guid",
  "tuitionPlanName": "Basic Plan",
  "weeklyPattern": [
    { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
  ],
  "scheduleSegments": [
    {
      "id": "guid",
      "effectiveFrom": "2026-06-01",
      "effectiveTo": null,
      "weeklyPattern": [
        { "dayOfWeeks": ["SA"], "startTime": "17:00", "durationMinutes": 60 }
      ]
    }
  ],
  "createdAt": "2026-04-25T10:00:00Z",
  "updatedAt": "2026-04-25T10:00:00Z"
}
```

### 16. POST `/api/enrollments/{id}/schedule-segments`

- Role: `Admin`, `ManagementStaff`
- Chi support supplementary enrollment

Request:

```json
{
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "weeklyPattern": [
    { "dayOfWeeks": ["SA"], "startTime": "17:00", "durationMinutes": 60 }
  ],
  "clearWeeklyPattern": false
}
```

Response `data`:

```json
{
  "id": "guid",
  "enrollmentId": "guid",
  "classId": "guid",
  "programId": "guid",
  "effectiveFrom": "2026-06-01",
  "effectiveTo": null,
  "weeklyPattern": [
    { "dayOfWeeks": ["SA"], "startTime": "17:00", "durationMinutes": 60 }
  ],
  "activeWeeklyPattern": [
    { "dayOfWeeks": ["TH"], "startTime": "18:00", "durationMinutes": 60 }
  ]
}
```

Loi hay gap:

- `Enrollment.NotFound`
- `Enrollment.SupplementaryProgramRequired`
- `Enrollment.AlreadyDropped`
- `Enrollment.ScheduleSegmentInvalidEffectiveDate`
- `Enrollment.ScheduleSegmentAlreadyExists`
- `Enrollment.FutureScheduleSegmentExists`
- `Enrollment.SessionSelectionPatternInvalid`
- `Enrollment.SessionSelectionPatternEmpty`
- `Enrollment.SessionSelectionPatternMismatch`
- `Enrollment.ClassSchedulePatternInvalid`
- `Enrollment.StudentScheduleConflict`

## Error code can FE map nhanh

### Weekly schedule cua class

- `WeeklySchedule.Empty`
- `SchedulePattern.Invalid`
- `SchedulePattern.InvalidDayOfWeek`
- `SchedulePattern.InvalidStartTime`
- `SchedulePattern.InvalidDuration`
- `SchedulePattern.DuplicateSlot`

### Class schedule segment

- `Class.SupplementaryProgramRequired`
- `Class.ScheduleSegmentInvalidEffectiveDate`
- `Class.ScheduleSegmentAlreadyExists`
- `Class.FutureScheduleSegmentExists`

### Enrollment selection pattern

API public da doi ten thanh `weeklyPattern`, nhung backend van giu cac error code cu de tranh pha vo luong hien co.

- `Enrollment.SessionSelectionPatternInvalid`
- `Enrollment.SessionSelectionPatternEmpty`
- `Enrollment.SessionSelectionPatternMismatch`
- `Enrollment.ClassSchedulePatternInvalid`
- `Enrollment.ScheduleSegmentInvalidEffectiveDate`
- `Enrollment.ScheduleSegmentAlreadyExists`
- `Enrollment.FutureScheduleSegmentExists`

### Session generation

- `Session.InvalidClassStatus`
- `Session.MissingSchedulePattern`
- `Session.MissingClassEndDate`
- `Session.InvalidDuration`
- `Session.InvalidBranch`
- `Session.InvalidRoom`
- `Session.InvalidTeacher`
- `Session.InvalidAssistant`
- `Session.SaveFailed`

## Goi y FE implementation

- Khong dung field `schedulePattern` nua o class APIs.
- Render lich class tu `weeklyScheduleSlots`.
- Neu can hien thi text, uu tien render o FE tu `dayOfWeek + startTime + durationMinutes`; backend co san `scheduleText` o mot so response.
- Enrollment/assign/transfer/reassign dung `weeklyPattern`.
- `weeklyPattern` bat buoc phai co `durationMinutes` > 0. FE nen lay gia tri nay tu `weeklyScheduleSlots` cua class khi pre-fill.
- Neu nhieu ngay hoc cung gio va cung duration, gop chung vao 1 entry `weeklyPattern`.
- Neu class co nhieu gio hoac duration khac nhau trong tuan, tach thanh nhieu entry `weeklyPattern`.
- Khi can tao buoi hoc lich su, FE goi `POST /api/sessions/generate-from-pattern` voi `onlyFutureSessions = false`.
- `weeklyPattern = null` trong response co nghia la hoc vien hoc toan bo lich class (dung `classWeeklyScheduleSlots`).
- `effectiveWeeklyPattern = null` trong response segment co nghia la segment do dung toan bo lich class.
