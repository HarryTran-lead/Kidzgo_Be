# FE Error Mapping - New Business Validations

Date: 2026-04-23

Scope: API errors added or newly reachable from the latest BE validation guards.

## Response shape

Most handlers return the existing API error format:

```json
{
  "success": false,
  "code": "Class.HasActiveEnrollments",
  "message": "Cannot change class while it has active or paused enrollments"
}
```

Status toggle blockers return the detailed conflict contract:

```json
{
  "success": false,
  "code": "STATUS_CHANGE_BLOCKED",
  "message": "Cannot deactivate because the entity is currently in use.",
  "details": {
    "entity": "Program",
    "entityId": "...",
    "reasons": ["ACTIVE_CLASSES_EXIST"],
    "counts": {
      "activeClasses": 1
    }
  }
}
```

## HTTP status

- `Conflict` errors map to HTTP `409`.
- `Validation` errors map to HTTP `400`.
- `NotFound` errors map to HTTP `404`.
- `STATUS_CHANGE_BLOCKED` maps to HTTP `409`.

## Status toggle endpoints

### `PATCH /api/programs/{id}/toggle-status`

When deactivating a program:

| Code | Message | Details reasons |
| --- | --- | --- |
| `STATUS_CHANGE_BLOCKED` | `Cannot deactivate because the entity is currently in use.` | `ACTIVE_CLASSES_EXIST`, `ACTIVE_STUDENTS_EXIST` |

Counts may include:

| Count key | Meaning |
| --- | --- |
| `activeClasses` | Active or planned classes using the program |
| `activeStudents` | Distinct active or paused students/enrollments using the program |

### `PATCH /api/tuition-plans/{id}/toggle-status`

When deactivating a tuition plan:

| Code | Message | Details reasons |
| --- | --- | --- |
| `STATUS_CHANGE_BLOCKED` | `Cannot deactivate because the entity is currently in use.` | `ACTIVE_ENROLLMENTS_EXIST` |

Counts may include:

| Count key | Meaning |
| --- | --- |
| `activeEnrollments` | Active or paused enrollments using the tuition plan |

### `PATCH /api/classrooms/{id}/toggle-status`

When deactivating a classroom:

| Code | Message | Details reasons |
| --- | --- | --- |
| `STATUS_CHANGE_BLOCKED` | `Cannot deactivate because the entity is currently in use.` | `ACTIVE_CLASSES_EXIST`, `FUTURE_SESSIONS_EXIST` |

Counts may include:

| Count key | Meaning |
| --- | --- |
| `activeClasses` | Active classes using the classroom |
| `futureSessions` | Scheduled sessions from now onward using the classroom |

### `PATCH /api/branches/{id}/status`

When deactivating a branch:

| Code | Message | Details reasons |
| --- | --- | --- |
| `STATUS_CHANGE_BLOCKED` | `Cannot deactivate because the entity is currently in use.` | `ACTIVE_CLASSES_EXIST`, `ACTIVE_STUDENTS_EXIST`, `ACTIVE_STAFF_EXIST`, `ACTIVE_ROOMS_EXIST` |

Counts may include:

| Count key | Meaning |
| --- | --- |
| `activeClasses` | Active classes in the branch |
| `activeStudents` | Distinct active or paused students/enrollments in the branch |
| `activeStaff` | Active management/accountant/teacher users in the branch |
| `activeRooms` | Active classrooms in the branch |

## Class endpoints

Affected endpoints:

- `POST /api/classes`
- `PUT /api/classes/{id}`
- `DELETE /api/classes/{id}`
- `PATCH /api/classes/{id}/status`
- `PATCH /api/classes/{id}/assign-teacher`

| Code | HTTP | Message |
| --- | --- | --- |
| `Class.RoomNotFound` | `404` | `Room not found or inactive` |
| `Class.RoomBranchMismatch` | `409` | `Room must belong to the same branch as the class` |
| `Class.TeacherAndAssistantMustDiffer` | `400` | `Main teacher and assistant teacher must be different users` |
| `Class.HasActiveEnrollments` | `409` | `Cannot change class while it has active or paused enrollments` |
| `Class.HasFutureSessions` | `409` | `Cannot change class while it has future sessions` |
| `Class.HasOperationalDependencies` | `409` | `Cannot change branch or program while class has enrollments or sessions` |
| `Class.CapacityBelowActiveEnrollments` | `409` | `Capacity cannot be lower than the number of active enrollments` |
| `Class.InvalidActiveDependencies` | `409` | `Class cannot be activated while branch, program, room, or teachers are inactive` |
| `Class.CannotCloseWithActiveEnrollments` | `409` | `Cannot close, suspend, complete, or cancel class with active or paused enrollments` |
| `Class.CannotCloseWithFutureSessions` | `409` | `Cannot close, suspend, complete, or cancel class with future sessions` |
| `Class.RoomConflict` | `409` | `Room is already booked by class '{classCode} - {classTitle}' at {datetime}` |
| `Class.TeacherConflict` | `409` | `Teacher is already assigned to class '{classCode} - {classTitle}' at {datetime}` |
| `Class.AssistantConflict` | `409` | `Assistant teacher is already assigned to class '{classCode} - {classTitle}' at {datetime}` |

Notes:

- `Class.MainTeacherNotFound` and `Class.AssistantTeacherNotFound` are now also returned when the teacher user is inactive or deleted.
- `PUT /api/classes/{id}` blocks branch/program changes if the class already has enrollments or sessions.
- `PUT /api/classes/{id}` blocks schedule changes if the class already has future scheduled sessions.

## Session endpoints

Affected endpoints:

- `POST /api/sessions`
- `PUT /api/sessions/{sessionId}`
- `PUT /api/sessions/by-class`
- `POST /api/sessions/{sessionId}/cancel`

| Code | HTTP | Message |
| --- | --- | --- |
| `Session.InvalidRoom` | `400` | `Room with ID {roomId} does not exist, is inactive, or does not belong to this branch` |
| `Session.InvalidTeacher` | `400` | `Main teacher with ID {teacherId} does not exist, is inactive, is not a teacher, or does not belong to this branch` |
| `Session.InvalidAssistant` | `400` | `Assistant teacher with ID {assistantId} does not exist, is inactive, is not a teacher, or does not belong to this branch` |
| `Session.TeacherAndAssistantMustDiffer` | `400` | `Main teacher and assistant teacher must be different users` |
| `Session.RoomOccupied` | `409` | `Room is already occupied by class '{classCode} - {className}' at {datetime}` |
| `Session.TeacherOccupied` | `409` | `Teacher is already assigned to class '{classCode} - {className}' at {datetime}` |
| `Session.AssistantOccupied` | `409` | `Assistant teacher is already assigned to class '{classCode} - {className}' at {datetime}` |
| `Session.AlreadyCompleted` | `400` | `Completed sessions cannot be cancelled` |
| `Session.HasAttendance` | `409` | `Session cannot be cancelled because attendance has already been recorded` |
| `Session.HasReports` | `409` | `Session cannot be cancelled because reports have already been created` |

Notes:

- Single session create/update now blocks conflicts instead of allowing them as warnings.
- Bulk update by class still returns per-session conflict text in `errors` for skipped sessions.

## Program and Branch delete endpoints

Affected endpoints:

- `DELETE /api/programs/{id}`
- `DELETE /api/branches/{id}`

| Code | HTTP | Message |
| --- | --- | --- |
| `Program.HasActiveClasses` | `409` | `Cannot delete program with active or planned classes` |
| `Program.HasActiveEnrollments` | `409` | `Cannot delete program with active or paused enrollments` |
| `Branch.HasActiveDependencies` | `409` | `Cannot deactivate branch while it has active classes, students, staff, or rooms` |

## Enrollment endpoint

Affected endpoint:

- `POST /api/enrollments`

| Code | HTTP | Message |
| --- | --- | --- |
| `Enrollment.StudentNotFound` | `404` | `Student profile not found or is not a student` |
| `Enrollment.AlreadyEnrolled` | `409` | `Student is already enrolled in this class` |

Notes:

- `Enrollment.StudentNotFound` is now also returned when the student profile is inactive or deleted.
- `Enrollment.AlreadyEnrolled` is now also returned when the existing enrollment is paused.

## Profile endpoints

Affected endpoints:

- `PUT /api/profiles/{id}`
- `DELETE /api/profiles/{id}`

| Code | HTTP | Message |
| --- | --- | --- |
| `Profile.HasActiveEnrollments` | `409` | `Cannot deactivate or delete a student profile with active or paused enrollments` |
| `Profile.HasFutureSessions` | `409` | `Cannot deactivate or delete a student profile with future assigned sessions` |
| `Profile.HasActiveStudentLinks` | `409` | `Cannot deactivate or delete a parent profile with active student links` |

## Admin user endpoints

Affected endpoints:

- `PUT /api/admin/users/{id}`
- `DELETE /api/admin/users/{id}`
- `PATCH /api/admin/users/{id}/assign-branch`

| Code | HTTP | Message |
| --- | --- | --- |
| `Users.HasActiveAssignments` | `409` | `Cannot deactivate, delete, or change role while the user has active assignments` |
| `Users.BranchInactive` | `409` | `User cannot be assigned to an inactive branch` |
| `Users.EmailNotUnique` | `409` | `The provided email is not unique` |
| `Profile.HasActiveEnrollments` | `409` | `Cannot deactivate or delete a student profile with active or paused enrollments` |
| `Profile.HasFutureSessions` | `409` | `Cannot deactivate or delete a student profile with future assigned sessions` |
| `Profile.HasActiveStudentLinks` | `409` | `Cannot deactivate or delete a parent profile with active student links` |

Notes:

- `PUT /api/admin/users/{id}` now validates email uniqueness when changing email.
- `DELETE /api/admin/users/{id}` reuses the update-user path and can return the same dependency errors.

## Exam endpoint

Affected endpoint:

- `DELETE /api/exams/{id}`

| Code | HTTP | Message |
| --- | --- | --- |
| `Exam.HasSubmissions` | `409` | `Cannot delete exam with submissions` |
| `Exam.HasResults` | `409` | `Cannot delete exam with results` |

## Homework endpoint

Affected endpoint:

- `DELETE /api/homework/{id}`

| Code | HTTP | Message |
| --- | --- | --- |
| `Homework.CannotDeleteWithStudentWork` | `409` | `Cannot delete homework assignment with started, submitted, graded, late, or missing student work` |

