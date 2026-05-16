# Phase 2 API Usage (Frontend)

Tài liệu này mô tả `API mới` và `API đã chỉnh sửa` trong Phase 2 (Academic Progression).

## 1. Response Envelope

### Success (`200`, `201`)
```json
{
  "isSuccess": true,
  "data": {}
}
```

### Error (`400`, `404`, `409`, `500`)
Backend trả `ProblemDetails` + `extensions.errors`:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "AcademicProgression.ModuleNotFound",
  "status": 404,
  "detail": "Module '...' was not found.",
  "errors": [
    {
      "code": "AcademicProgression.ModuleNotFound",
      "description": "Module '...' was not found."
    }
  ]
}
```

## 2. Auth & Roles

- `401 Unauthorized`: thiếu/invalid token.
- `403 Forbidden`: đúng token nhưng sai role.

Role theo controller:
- `Admin, ManagementStaff`: level/module + lesson-plan-template.
- `Admin, ManagementStaff, Teacher`: student-progress, assessment, teacher-evaluation, promotion-decision, remedial-plan, lesson-plan.

## 3. API Mới (Academic Progression)

## 3.1 Levels

### `GET /api/levels`
- Role: `Admin, ManagementStaff`
- Query:
  - `programId` (optional, guid)
  - `isActive` (optional, bool)
  - `searchTerm` (optional, string)
- Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "programId": "guid",
        "code": "STARTERS",
        "name": "Starters",
        "order": 1,
        "description": "Cambridge Starters",
        "isActive": true
      }
    ]
  }
}
```

### `POST /api/levels`
- Role: `Admin, ManagementStaff`
- Request:
```json
{
  "programId": "guid",
  "code": "STARTERS",
  "name": "Starters",
  "order": 1,
  "description": "Cambridge Starters",
  "isActive": true
}
```
- Response `201`: `LevelDto`
- Error thường gặp:
  - `AcademicProgression.ProgramNotFound` (`404`)
  - `AcademicProgression.LevelDuplicate` (`409`)

### `PUT /api/levels/{id}`
- Role: `Admin, ManagementStaff`
- Request:
```json
{
  "code": "STARTERS",
  "name": "Starters",
  "order": 1,
  "description": "Updated",
  "isActive": true
}
```
- Response `200`: `LevelDto`
- Error thường gặp:
  - `AcademicProgression.LevelNotFound` (`404`)
  - `AcademicProgression.LevelDuplicate` (`409`)

## 3.2 Modules

### `GET /api/modules`
- Role: `Admin, ManagementStaff`
- Query:
  - `levelId` (optional, guid)
  - `isActive` (optional, bool)
  - `searchTerm` (optional, string)
- Response `200`: `data.items[]` kiểu `ModuleDto`

### `POST /api/modules`
- Role: `Admin, ManagementStaff`
- Request:
```json
{
  "levelId": "guid",
  "code": "STARTERS_M1",
  "name": "Alphabet",
  "order": 1,
  "description": "A-D, E-H",
  "plannedSessionCount": 6,
  "isActive": true
}
```
- Response `201`: `ModuleDto`
- Error:
  - `AcademicProgression.LevelNotFound` (`404`)
  - `AcademicProgression.ModuleDuplicate` (`409`)

### `PUT /api/modules/{id}`
- Role: `Admin, ManagementStaff`
- Request giống `POST` nhưng không có `levelId`.
- Response `200`: `ModuleDto`
- Error:
  - `AcademicProgression.ModuleNotFound` (`404`)
  - `AcademicProgression.ModuleDuplicate` (`409`)

## 3.3 Student Progress

### `GET /api/student-progress/{studentId}`
- Role: `Admin, ManagementStaff, Teacher`
- Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "items": [
      {
        "id": "guid",
        "studentProfileId": "guid",
        "moduleId": "guid",
        "moduleCode": "STARTERS_M1",
        "moduleName": "Alphabet",
        "levelCode": "STARTERS",
        "status": "InProgress",
        "completionPercent": 85.5,
        "assessmentStatus": "Passed",
        "promotionStatus": "Pending",
        "lastAssessmentId": "guid",
        "currentLessonPlanTemplateId": "guid",
        "startedAt": "2026-05-16T10:00:00Z",
        "completedAt": null
      }
    ]
  }
}
```

### `POST /api/student-progress/update`
- Role: `Admin, ManagementStaff, Teacher`
- Request:
```json
{
  "studentProfileId": "guid",
  "moduleId": "guid",
  "currentLessonPlanTemplateId": "guid",
  "completionPercent": 50
}
```
- Response `200`: `StudentProgressDto`
- Error:
  - `AcademicProgression.ModuleNotFound` (`404`)
  - `AcademicProgression.StudentNotFound` (`404`)

### `GET /api/student-progress/dashboard`
- Role: `Admin, ManagementStaff, Teacher`
- Response `200`:
```json
{
  "isSuccess": true,
  "data": {
    "inProgressStudents": 32,
    "completedStudents": 18,
    "remedialRequiredStudents": 4,
    "failedPromotions": 2,
    "weakModules": [
      {
        "moduleId": "guid",
        "moduleCode": "STARTERS_M1",
        "moduleName": "Alphabet",
        "remedialCount": 3,
        "averageCompletionPercent": 64.2
      }
    ]
  }
}
```

## 3.4 Assessments

### `POST /api/assessments`
- Role: `Admin, ManagementStaff, Teacher`
- Request:
```json
{
  "studentProfileId": "guid",
  "moduleId": "guid",
  "sessionId": "guid",
  "type": "Module Assessment",
  "score": 78,
  "teacherComment": "Good progress",
  "assessedAt": "2026-05-16T10:00:00Z"
}
```
- Rule hiện tại:
  - `score >= 70` => `PASS`
  - `score < 70` => `FAIL`
- Response `201`: `AssessmentDto`
- Error:
  - `AcademicProgression.ModuleNotFound` (`404`)
  - `AcademicProgression.StudentNotFound` (`404`)
  - `AcademicProgression.SessionNotFound` (`404`)

### `GET /api/assessments/{studentId}`
- Role: `Admin, ManagementStaff, Teacher`
- Response `200`: `data.items[]` kiểu `AssessmentDto` (order mới nhất trước)

## 3.5 Teacher Evaluations

### `POST /api/teacher-evaluations`
- Role: `Admin, ManagementStaff, Teacher`
- Request:
```json
{
  "studentProfileId": "guid",
  "moduleId": "guid",
  "speaking": 4,
  "listening": 4,
  "reading": 3,
  "writing": 3,
  "participation": 5,
  "confidence": 4,
  "behavior": 5,
  "notes": "Ready for next module",
  "evaluatedAt": "2026-05-16T10:00:00Z"
}
```
- Response `201`: `TeacherEvaluationDto`
- Error:
  - `AcademicProgression.ModuleNotFound` (`404`)
  - `AcademicProgression.StudentNotFound` (`404`)

### `GET /api/teacher-evaluations/{studentId}`
- Role: `Admin, ManagementStaff, Teacher`
- Response `200`: `data.items[]` kiểu `TeacherEvaluationDto` (order mới nhất trước)

## 3.6 Promotion Decision

### `POST /api/promotion-decisions`
- Role: `Admin, ManagementStaff, Teacher`
- Request:
```json
{
  "studentProfileId": "guid",
  "moduleId": "guid",
  "reason": "Manual override note",
  "approvedAt": "2026-05-16T10:00:00Z"
}
```
- Logic quyết định:
  - Assessment fail => `REMEDIAL_REQUIRED`
  - Assessment pass + completion >= 80 + confidence >= 3 => `PASS`
  - Confidence/Speaking thấp => `REMEDIAL_REQUIRED`
  - Còn lại => `FAIL`
- Response `201`: `PromotionDecisionDto`
- Side effects:
  - `PASS`: đánh dấu module hiện tại `Completed`, tự tạo progress cho module kế tiếp nếu có.
  - `REMEDIAL_REQUIRED`: tự tạo `RemedialPlan` với `recommendedSessionCount = 2`.
- Error:
  - `AcademicProgression.ModuleNotFound` (`404`)
  - `AcademicProgression.StudentNotFound` (`404`)

## 3.7 Remedial Plans

### `POST /api/remedial-plans`
- Role: `Admin, ManagementStaff, Teacher`
- Request:
```json
{
  "studentProfileId": "guid",
  "moduleId": "guid",
  "weakSkills": "speaking, confidence",
  "recommendedSessionCount": 2,
  "notes": "Need speaking reinforcement"
}
```
- Response `201`: `RemedialPlanDto`
- Error:
  - `AcademicProgression.ModuleNotFound` (`404`)
  - `AcademicProgression.StudentNotFound` (`404`)

### `GET /api/remedial-plans/{studentId}`
- Role: `Admin, ManagementStaff, Teacher`
- Response `200`: `data.items[]` kiểu `RemedialPlanDto` (order mới nhất trước)

## 4. API Đã Chỉnh Sửa (Phase 2)

## 4.1 Lesson Plan Templates

### `POST /api/lesson-plan-templates`
- Role: `ManagementStaff, Admin`
- Field mới:
  - `moduleId` (optional)
  - `sessionOrder` (optional, nếu null sẽ lấy `sessionIndex`)
- Request:
```json
{
  "programId": "guid",
  "moduleId": "guid",
  "level": "STARTERS",
  "title": "Lesson 1",
  "sessionIndex": 1,
  "sessionOrder": 1,
  "syllabusMetadata": "{}",
  "syllabusContent": "Alphabet A-D",
  "sourceFileName": "starters.xlsx",
  "attachment": "https://..."
}
```
- Response `201`: `CreateLessonPlanTemplateResponse` có thêm `moduleId`, `sessionOrder`.
- Error:
  - `LessonPlanTemplate.ProgramNotFound` (`404`)
  - `LessonPlanTemplate.ModuleInvalid` (`400`)
  - `LessonPlanTemplate.SessionIndexRequired` (`400`)
  - `LessonPlanTemplate.DuplicateSessionIndex` (`409`)

### `PUT /api/lesson-plan-templates/{id}`
- Role: `ManagementStaff, Admin`
- Field mới cho update: `moduleId`, `sessionOrder`.
- Response `200`: `UpdateLessonPlanTemplateResponse` có `moduleId`, `sessionOrder`.
- Error:
  - `LessonPlanTemplate.NotFound` (`404`)
  - `LessonPlanTemplate.Unauthorized` (`400`)
  - `LessonPlanTemplate.ModuleInvalid` (`400`)
  - `LessonPlanTemplate.SessionIndexRequired` (`400`)
  - `LessonPlanTemplate.DuplicateSessionIndex` (`409`)

### `GET /api/lesson-plan-templates/{id}`
- Response có thêm:
  - `moduleId`, `moduleCode`, `moduleName`, `sessionOrder`

### `GET /api/lesson-plan-templates`
- Response item có thêm:
  - `moduleId`, `moduleCode`, `moduleName`, `sessionOrder`

## 4.2 Lesson Plans

### `POST /api/lesson-plans`
- Role: `Teacher, ManagementStaff, Admin`
- Field mới:
  - `completionPercent` (optional, `0..100`)
  - `carryForwardContent` (optional)
- Request:
```json
{
  "classId": "guid",
  "sessionId": "guid",
  "templateId": "guid",
  "plannedContent": "P38-39",
  "actualContent": "P38 only",
  "actualHomework": "Workbook p40",
  "teacherNotes": "Need review",
  "completionPercent": 50,
  "carryForwardContent": "P39 carry forward"
}
```
- Response `201`: có thêm `completionPercent`, `carryForwardContent`.
- Side effects:
  - Nếu lesson plan có template và template map module, backend tự recalculate `StudentProgress` cho học viên có attendance `Present/Makeup` ở session đó.
- Error:
  - `LessonPlan.ClassNotFound` (`404`)
  - `Session.NotFound` (`404`)
  - `LessonPlan.SessionClassMismatch` (`400`)
  - `LessonPlan.SessionAlreadyHasLessonPlan` (`409`)
  - `LessonPlan.TemplateNotFound` (`404`)
  - `LessonPlan.Unauthorized` (`400`)

### `PUT /api/lesson-plans/{id}`
- Role: `Teacher, ManagementStaff, Admin`
- Field mới update:
  - `completionPercent` (auto clamp `0..100`)
  - `carryForwardContent`
- Response `200`: có thêm `completionPercent`, `carryForwardContent`.
- Side effects: recalculate `StudentProgress` tương tự create.
- Error:
  - `LessonPlan.NotFound` (`404`)
  - `LessonPlan.TemplateNotFound` (`404`)
  - `LessonPlan.Unauthorized` (`400`)

### `GET /api/lesson-plans/{id}`
- Response có thêm:
  - `completionPercent`, `carryForwardContent`

## 5. FE Integration Notes

1. Tất cả enum đang trả về dạng `string` (`Pass`, `Fail`, `RemedialRequired`, `InProgress`...), FE nên map case-insensitive.
2. Với error, FE nên ưu tiên hiển thị `detail`, và fallback từ `errors[0].description`.
3. Khi gọi `POST /api/promotion-decisions`, không cần gọi thêm API remedial nếu decision là `REMEDIAL_REQUIRED` vì backend đã tự tạo plan.
